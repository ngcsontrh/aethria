import { useState, useRef, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import {
  getPageQuizzes,
  getQuizById,
  getQuizQuestionsForEdit,
  createBlankQuiz,
  updateQuiz,
  deleteQuiz,
  getResourceSelector,
  createAIQuizStream,
} from "../../services";
import type { QuizPageItem } from "../../services";
import type { QuizFormValues, EditingQuizData } from "./useQuizForm";

const quizKeys = {
  all: ["quizzes"] as const,
  pages: () => [...quizKeys.all, "page"] as const,
  page: (page: number, pageSize: number) =>
    [...quizKeys.pages(), page, pageSize] as const,
  detail: (id: string) => [...quizKeys.all, "detail", id] as const,
  questionsForEdit: (id: string) =>
    [...quizKeys.all, "questions-for-edit", id] as const,
};

export function useQuizPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [formOpened, setFormOpened] = useState(false);
  const [editingQuiz, setEditingQuiz] = useState<EditingQuizData | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<QuizPageItem | null>(null);

  // AI generation SSE state
  const [aiGenerating, setAiGenerating] = useState(false);
  const [aiStatus, setAiStatus] = useState("");
  const [aiMessage, setAiMessage] = useState("");

  // Attempt Quiz State
  const [attemptQuiz, setAttemptQuiz] = useState<QuizPageItem | null>(null);

  // History State
  const [historyQuiz, setHistoryQuiz] = useState<QuizPageItem | null>(null);

  const pageSize = 10;

  // 1. Quizzes list query
  const quizzesQuery = useQuery({
    queryKey: quizKeys.page(page, pageSize),
    queryFn: ({ signal }) => getPageQuizzes(page, pageSize, signal),
  });

  // 2. Resources select options query
  const resourcesQuery = useQuery({
    queryKey: ["resources", "selector"],
    queryFn: ({ signal }) => getResourceSelector(signal),
  });

  // 3. Save manual / Edit quiz mutation
  const saveQuizMutation = useMutation({
    mutationFn: async (data: QuizFormValues) => {
      if (editingQuiz) {
        // Map form values to UpdateQuizRequest
        const updateRequest = {
          name: data.name,
          description: data.description,
          questions: data.questions.map((q, idx) => ({
            text: q.text,
            explanation: q.explanation,
            orderIndex: idx,
            correctOptionIndex: q.correctOptionIndex,
            options: q.options.map((opt, optIdx) => ({
              text: opt.text,
              orderIndex: optIdx,
            })),
          })),
        };
        await updateQuiz(editingQuiz.id, updateRequest);
        return { isEdit: true };
      } else {
        await createBlankQuiz({
          name: data.name,
          description: data.description || undefined,
          resourceId: data.resourceId || undefined,
        });
        return { isEdit: false };
      }
    },
    onSuccess: async (data) => {
      await queryClient.invalidateQueries({ queryKey: quizKeys.pages() });
      notifications.show({
        title: t("notifications.success"),
        message: data.isEdit
          ? t("notifications.quiz.updateSuccess")
          : t("notifications.quiz.createSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: editingQuiz
          ? t("notifications.quiz.updateError")
          : t("notifications.quiz.createError"),
        color: "red",
      });
    },
  });

  // 4. Delete quiz mutation
  const deleteQuizMutation = useMutation({
    mutationFn: (id: string) => deleteQuiz(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: quizKeys.pages() });
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.quiz.deleteSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.quiz.deleteError"),
        color: "red",
      });
    },
  });

  const aiAbortControllerRef = useRef<AbortController | null>(null);

  const stopAiGeneration = useCallback(() => {
    aiAbortControllerRef.current?.abort();
    setAiGenerating(false);
    setAiStatus("Canceled");
    setAiMessage("AI Quiz generation was canceled.");
  }, []);

  useEffect(() => {
    return () => {
      aiAbortControllerRef.current?.abort();
    };
  }, []);

  // 5. SSE stream generation for AI quiz
  const generateAIQuiz = async (data: QuizFormValues) => {
    setAiGenerating(true);
    setAiStatus("Started");
    setAiMessage("");
    
    const abortController = new AbortController();
    aiAbortControllerRef.current = abortController;

    try {
      const stream = createAIQuizStream({
        name: data.name,
        description: data.description || undefined,
        resourceId: data.resourceId,
        prompt: data.prompt || undefined,
        numberOfQuestions: data.numberOfQuestions!,
      }, abortController.signal);

      for await (const event of stream) {
        if (abortController.signal.aborted) break;

        setAiStatus(event.status);
        setAiMessage(event.message || "");

        if (event.status === "Completed") {
          await queryClient.invalidateQueries({ queryKey: quizKeys.pages() });
          notifications.show({
            title: t("notifications.success"),
            message: t("notifications.quiz.createSuccess"),
            color: "green",
          });
          setTimeout(() => {
            closeForm();
            setAiGenerating(false);
          }, 1500);
          return;
        }
      }
    } catch (err) {
      if (!abortController.signal.aborted) {
        const error = err as Error;
        setAiStatus("Failed");
        setAiMessage(error.message || "Failed to generate quiz");
        notifications.show({
          title: t("notifications.error"),
          message: error.message || t("notifications.quiz.createError"),
          color: "red",
        });
        setTimeout(() => {
          setAiGenerating(false);
        }, 3000);
      }
    } finally {
      if (aiAbortControllerRef.current === abortController) {
        aiAbortControllerRef.current = null;
      }
    }
  };

  const openCreate = () => {
    setEditingQuiz(null);
    setFormOpened(true);
  };

  const openEdit = async (id: string) => {
    const quiz = await queryClient.fetchQuery({
      queryKey: quizKeys.detail(id),
      queryFn: ({ signal }) => getQuizById(id, signal),
    });

    const questionsResponse = await queryClient.fetchQuery({
      queryKey: quizKeys.questionsForEdit(id),
      queryFn: ({ signal }) => getQuizQuestionsForEdit(id, signal),
    });

    setEditingQuiz({
      ...quiz,
      questions: questionsResponse.questions,
    });

    setFormOpened(true);
  };

  const closeForm = () => {
    if (aiGenerating) {
      stopAiGeneration();
    }
    setFormOpened(false);
    setEditingQuiz(null);
  };

  const handleSubmit = async (data: QuizFormValues) => {
    if (!editingQuiz && data.mode === "ai") {
      await generateAIQuiz(data);
    } else {
      await saveQuizMutation.mutateAsync(data);
      closeForm();
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteQuizMutation.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  // Attempt actions
  const startAttempt = (quiz: QuizPageItem) => {
    setAttemptQuiz(quiz);
  };

  const closeAttempt = () => {
    setAttemptQuiz(null);
  };

  // History actions
  const openHistory = (quiz: QuizPageItem) => {
    setHistoryQuiz(quiz);
  };

  const closeHistory = () => {
    setHistoryQuiz(null);
  };

  const quizzes = quizzesQuery.data?.items ?? [];
  const totalPages = quizzesQuery.data?.totalPages ?? 0;
  const submitting = saveQuizMutation.isPending || deleteQuizMutation.isPending;

  // Format resources options for the Select dropdown
  const resourcesOptions = (resourcesQuery.data?.resources ?? []).map((r) => ({
    value: r.id,
    label: r.name,
  }));

  return {
    t,
    quizzes,
    totalPages,
    page,
    setPage,
    pageSize,
    loading: quizzesQuery.isLoading,
    resourcesOptions,
    formOpened,
    editingQuiz,
    deleteTarget,
    setDeleteTarget,
    submitting,
    aiGenerating,
    aiStatus,
    aiMessage,
    stopAiGeneration,
    openCreate,
    openEdit,
    closeForm,
    handleSubmit,
    handleDelete,
    // Attempt
    attemptQuiz,
    startAttempt,
    closeAttempt,
    // History
    historyQuiz,
    openHistory,
    closeHistory,
  };
}
