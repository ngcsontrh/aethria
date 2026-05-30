import { useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import axios from "axios";
import {
  getPageResources,
  getResourceById,
  createResource,
  updateResource,
  deleteResource,
  downloadResource,
} from "../../services";
import type {
  ResourcePageItemResponse,
  GetResourceByIdResponse,
} from "../../services";
import type { ResourceFormValues } from "./useResourceForm";

const resourceKeys = {
  all: ["resources"] as const,
  pages: () => [...resourceKeys.all, "page"] as const,
  page: (page: number, pageSize: number) =>
    [...resourceKeys.pages(), page, pageSize] as const,
  detail: (id: string) => [...resourceKeys.all, "detail", id] as const,
};

export function useResourcePage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [formOpened, setFormOpened] = useState(false);
  const [editingResource, setEditingResource] =
    useState<GetResourceByIdResponse | null>(null);
  const [deleteTarget, setDeleteTarget] =
    useState<ResourcePageItemResponse | null>(null);
  const [downloadingId, setDownloadingId] = useState<string | null>(null);
  const [uploadProcessing, setUploadProcessing] = useState(false);
  const [uploadStatus, setUploadStatus] = useState("Started");
  const [uploadMessageIndex, setUploadMessageIndex] = useState(0);
  const pageSize = 10;

  const uploadAbortControllerRef = useRef<AbortController | null>(null);
  const downloadControllersRef = useRef<Map<string, AbortController>>(new Map());

  useEffect(() => {
    const downloadControllers = downloadControllersRef.current;

    return () => {
      uploadAbortControllerRef.current?.abort();
      downloadControllers.forEach((controller) => controller.abort());
      downloadControllers.clear();
    };
  }, []);

  useEffect(() => {
    if (!uploadProcessing) return;

    const intervalId = window.setInterval(() => {
      setUploadMessageIndex((current) => {
        const next = Math.floor(Math.random() * 5);
        return next === current ? (next + 1) % 5 : next;
      });
    }, 3500);

    return () => window.clearInterval(intervalId);
  }, [uploadProcessing]);

  const resourcesQuery = useQuery({
    queryKey: resourceKeys.page(page, pageSize),
    queryFn: ({ signal }) => getPageResources(page, pageSize, signal),
  });

  const stopUpload = () => {
    uploadAbortControllerRef.current?.abort();
    setUploadProcessing(false);
    setUploadStatus("Canceled");
  };

  const saveResourceMutation = useMutation({
    mutationFn: async (data: ResourceFormValues) => {
      if (editingResource) {
        await updateResource(editingResource.id, {
          name: data.name,
          description: data.description,
        });
        return { isEdit: true };
      } else {
        if (!data.file) {
          throw new Error("File is required for creation");
        }

        const abortController = new AbortController();
        uploadAbortControllerRef.current = abortController;
        setUploadProcessing(true);
        setUploadStatus("Started");
        setUploadMessageIndex(Math.floor(Math.random() * 5));

        try {
          await createResource(
            data.name,
            data.description,
            data.file,
            abortController.signal,
          );
        } finally {
          if (uploadAbortControllerRef.current === abortController) {
            uploadAbortControllerRef.current = null;
          }
        }

        return { isEdit: false };
      }
    },
    onSuccess: async (data) => {
      await queryClient.invalidateQueries({ queryKey: resourceKeys.pages() });
      if (!data.isEdit) {
        setUploadStatus("Completed");
      }

      notifications.show({
        title: t("notifications.success"),
        message: data.isEdit
          ? t("notifications.resource.updateSuccess")
          : t("notifications.resource.uploadSuccess"),
        color: "green",
      });

      if (!data.isEdit) {
        setTimeout(() => {
          setUploadProcessing(false);
          setFormOpened(false);
          setEditingResource(null);
        }, 1500);
      }
    },
    onError: (error) => {
      if (axios.isCancel(error)) {
        setUploadStatus("Canceled");
        return;
      }

      setUploadStatus("Failed");
      notifications.show({
        title: t("notifications.error"),
        message: editingResource
          ? t("notifications.resource.updateError")
          : t("notifications.resource.uploadError"),
        color: "red",
      });
    },
  });

  const deleteResourceMutation = useMutation({
    mutationFn: (id: string) => deleteResource(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: resourceKeys.pages() });
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.resource.deleteSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.resource.deleteError"),
        color: "red",
      });
    },
  });

  const openCreate = () => {
    setEditingResource(null);
    setFormOpened(true);
  };

  const openEdit = async (id: string) => {
    const resource = await queryClient.fetchQuery({
      queryKey: resourceKeys.detail(id),
      queryFn: ({ signal }) => getResourceById(id, signal),
    });
    setEditingResource(resource);
    setFormOpened(true);
  };

  const closeForm = () => {
    if (uploadProcessing) return;
    setFormOpened(false);
    setEditingResource(null);
  };

  const handleSubmit = async (data: ResourceFormValues) => {
    await saveResourceMutation.mutateAsync(data);
    if (editingResource) {
      closeForm();
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteResourceMutation.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  const handleDownload = async (id: string, name: string) => {
    if (downloadControllersRef.current.has(id)) {
      downloadControllersRef.current.get(id)?.abort();
    }
    const controller = new AbortController();
    downloadControllersRef.current.set(id, controller);

    try {
      setDownloadingId(id);
      const blob = await downloadResource(id, controller.signal);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;

      // Attempt to preserve the file extension from the name if possible, or build one.
      link.setAttribute("download", name);
      document.body.appendChild(link);
      link.click();
      link.parentNode?.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      if (axios.isCancel(error)) {
        return;
      }
      console.error("Failed to download resource", error);
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.resource.downloadError"),
        color: "red",
      });
    } finally {
      downloadControllersRef.current.delete(id);
      setDownloadingId((prev) => (prev === id ? null : prev));
    }
  };

  const resources = resourcesQuery.data?.items ?? [];
  const totalPages = resourcesQuery.data?.totalPages ?? 0;
  const submitting =
    saveResourceMutation.isPending || deleteResourceMutation.isPending;

  return {
    t,
    resources,
    totalPages,
    page,
    setPage,
    pageSize,
    loading: resourcesQuery.isLoading,
    formOpened,
    editingResource,
    deleteTarget,
    setDeleteTarget,
    downloadingId,
    submitting,
    uploadProcessing,
    uploadStatus,
    uploadMessageIndex,
    stopUpload,
    openCreate,
    openEdit,
    closeForm,
    handleSubmit,
    handleDelete,
    handleDownload,
  };
}
