import { useState, useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";
import { notifications } from "@mantine/notifications";
import axios from "axios";
import { getQuizQuestions, submitQuizAnswers } from "../../../services";
import type {
  QuizPageItem,
  QuizQuestionItem,
  SubmitQuizAnswersResponse,
} from "../../../services";

export function useAttemptQuiz(quiz: QuizPageItem | null) {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [questions, setQuestions] = useState<QuizQuestionItem[]>([]);
  const [quizVersionId, setQuizVersionId] = useState<string | null>(null);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [result, setResult] = useState<SubmitQuizAnswersResponse | null>(null);

  const submitAbortControllerRef = useRef<AbortController | null>(null);

  /* eslint-disable react-hooks/set-state-in-effect */
  useEffect(() => {
    if (!quiz) {
      setQuestions([]);
      setQuizVersionId(null);
      setAnswers({});
      setResult(null);
      return;
    }

    const controller = new AbortController();
    const fetchQuestions = async () => {
      setLoading(true);
      try {
        const response = await getQuizQuestions(quiz.id, controller.signal);
        setQuestions(response.questions);
        setQuizVersionId(response.quizVersionId || null);
      } catch (err) {
        if (axios.isCancel(err)) {
          return;
        }
        console.error(err);
        notifications.show({
          title: t("notifications.error"),
          message: t("notifications.quiz.loadQuestionsError"),
          color: "red",
        });
      } finally {
        setLoading(false);
      }
    };

    fetchQuestions();

    return () => {
      controller.abort();
    };
  }, [quiz, t]);
  /* eslint-enable react-hooks/set-state-in-effect */

  useEffect(() => {
    return () => {
      submitAbortControllerRef.current?.abort();
    };
  }, []);

  const setAnswer = (questionSnapshotId: string, optionId: string) => {
    setAnswers((prev) => ({
      ...prev,
      [questionSnapshotId]: optionId,
    }));
  };

  const submitAttempt = async () => {
    if (!quiz || !quizVersionId) return;

    if (submitAbortControllerRef.current) {
      submitAbortControllerRef.current.abort();
    }
    const controller = new AbortController();
    submitAbortControllerRef.current = controller;

    setSubmitting(true);
    try {
      const answersList = Object.entries(answers).map(([qId, optId]) => ({
        questionSnapshotId: qId,
        selectedOptionId: optId,
      }));
      const response = await submitQuizAnswers(
        quiz.id,
        {
          quizVersionId,
          answers: answersList,
        },
        controller.signal,
      );
      setResult(response);
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.quiz.submitSuccess"),
        color: "green",
      });
    } catch (err) {
      if (axios.isCancel(err)) {
        return;
      }
      console.error(err);
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.quiz.submitError"),
        color: "red",
      });
    } finally {
      if (submitAbortControllerRef.current === controller) {
        submitAbortControllerRef.current = null;
        setSubmitting(false);
      }
    }
  };

  return {
    loading,
    submitting,
    questions,
    answers,
    result,
    setAnswer,
    submitAttempt,
  };
}
