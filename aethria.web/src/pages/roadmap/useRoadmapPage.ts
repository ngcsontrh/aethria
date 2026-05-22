import { useState, useRef, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import {
  getPageRoadmaps,
  getRoadmapById,
  deleteRoadmap,
  getResourceSelector,
  generateAIRoadmapStream,
} from "../../services";
import type { RoadmapListItemResponse } from "../../services";
import type { RoadmapFormValues } from "./useRoadmapForm";

const roadmapKeys = {
  all: ["roadmaps"] as const,
  pages: () => [...roadmapKeys.all, "page"] as const,
  page: (page: number, pageSize: number) =>
    [...roadmapKeys.pages(), page, pageSize] as const,
  detail: (id: string) => [...roadmapKeys.all, "detail", id] as const,
};

export function useRoadmapPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [formOpened, setFormOpened] = useState(false);
  const [deleteTarget, setDeleteTarget] =
    useState<RoadmapListItemResponse | null>(null);
  const [viewingRoadmapId, setViewingRoadmapId] = useState<string | null>(null);

  // AI generation SSE state
  const [aiGenerating, setAiGenerating] = useState(false);
  const [aiStatus, setAiStatus] = useState("");
  const [aiMessage, setAiMessage] = useState("");

  const aiAbortControllerRef = useRef<AbortController | null>(null);

  const stopAiGeneration = useCallback(() => {
    aiAbortControllerRef.current?.abort();
    setAiGenerating(false);
    setAiStatus("Canceled");
    setAiMessage("AI Roadmap generation was canceled.");
  }, []);

  useEffect(() => {
    return () => {
      aiAbortControllerRef.current?.abort();
    };
  }, []);

  const pageSize = 10;

  // 1. Roadmaps list query
  const roadmapsQuery = useQuery({
    queryKey: roadmapKeys.page(page, pageSize),
    queryFn: ({ signal }) => getPageRoadmaps(page, pageSize, signal),
  });

  // 2. Resources selector options query
  const resourcesQuery = useQuery({
    queryKey: ["resources", "selector"],
    queryFn: ({ signal }) => getResourceSelector(signal),
  });

  // 3. Roadmap detail query
  const roadmapDetailQuery = useQuery({
    queryKey: roadmapKeys.detail(viewingRoadmapId ?? ""),
    queryFn: ({ signal }) => getRoadmapById(viewingRoadmapId ?? "", signal),
    enabled: !!viewingRoadmapId,
  });

  // 4. Delete roadmap mutation
  const deleteRoadmapMutation = useMutation({
    mutationFn: (id: string) => deleteRoadmap(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: roadmapKeys.pages() });
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.roadmap.deleteSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.roadmap.deleteError"),
        color: "red",
      });
    },
  });

  // 5. SSE stream generation for AI roadmap
  const generateAIRoadmap = async (data: RoadmapFormValues) => {
    setAiGenerating(true);
    setAiStatus("Started");
    setAiMessage("");

    const abortController = new AbortController();
    aiAbortControllerRef.current = abortController;

    try {
      const stream = generateAIRoadmapStream({
        name: data.name,
        description: data.description || undefined,
        resourceId: data.resourceId,
        prompt: data.prompt || undefined,
      }, abortController.signal);

      for await (const event of stream) {
        if (abortController.signal.aborted) break;

        setAiStatus(event.status);
        setAiMessage(event.message || "");

        if (event.status === "Completed") {
          await queryClient.invalidateQueries({
            queryKey: roadmapKeys.pages(),
          });
          notifications.show({
            title: t("notifications.success"),
            message: t("roadmap.aiProgress.completed"),
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
        setAiMessage(error.message || "Failed to generate roadmap");
        notifications.show({
          title: t("notifications.error"),
          message: t("roadmap.aiProgress.failed"),
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
    setFormOpened(true);
  };

  const openViewDetail = (id: string) => {
    setViewingRoadmapId(id);
  };

  const closeViewDetail = () => {
    setViewingRoadmapId(null);
  };

  const closeForm = () => {
    if (aiGenerating) {
      stopAiGeneration();
    }
    setFormOpened(false);
  };

  const handleSubmit = async (data: RoadmapFormValues) => {
    await generateAIRoadmap(data);
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await deleteRoadmapMutation.mutateAsync(deleteTarget.id);
    } catch {
      // handled by mutation onError
    } finally {
      setDeleteTarget(null);
    }
  };

  const roadmaps = roadmapsQuery.data?.items ?? [];
  const totalPages = roadmapsQuery.data?.totalPages ?? 0;
  const submitting = deleteRoadmapMutation.isPending;

  // Format resources options for the Select dropdown
  const resourcesOptions = (resourcesQuery.data?.resources ?? []).map((r) => ({
    value: r.id,
    label: r.name,
  }));

  return {
    t,
    roadmaps,
    totalPages,
    page,
    setPage,
    pageSize,
    loading: roadmapsQuery.isLoading,
    resourcesOptions,
    formOpened,
    deleteTarget,
    setDeleteTarget,
    submitting,
    aiGenerating,
    aiStatus,
    aiMessage,
    stopAiGeneration,
    openCreate,
    closeForm,
    handleSubmit,
    handleDelete,
    // Detail View
    viewingRoadmapId,
    viewingRoadmap: roadmapDetailQuery.data ?? null,
    viewingRoadmapLoading: roadmapDetailQuery.isLoading,
    openViewDetail,
    closeViewDetail,
  };
}
