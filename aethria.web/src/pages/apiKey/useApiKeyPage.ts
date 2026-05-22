import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import {
  getPageApiKeys,
  createApiKey,
  revokeApiKey,
} from "../../services";
import type {
  ApiKeyListItem,
  CreateApiKeyResponse,
} from "../../services";
import type { ApiKeyFormValues } from "./useApiKeyForm";

const apiKeyKeys = {
  all: ["apiKeys"] as const,
  pages: () => [...apiKeyKeys.all, "page"] as const,
  page: (page: number, pageSize: number) =>
    [...apiKeyKeys.pages(), page, pageSize] as const,
};

export function useApiKeyPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [formOpened, setFormOpened] = useState(false);
  const [createdApiKey, setCreatedApiKey] = useState<CreateApiKeyResponse | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ApiKeyListItem | null>(null);
  const pageSize = 10;

  const apiKeysQuery = useQuery({
    queryKey: apiKeyKeys.page(page, pageSize),
    queryFn: ({ signal }) => getPageApiKeys({ pageNumber: page, pageSize }, signal),
  });

  const createApiKeyMutation = useMutation({
    mutationFn: (data: ApiKeyFormValues) => createApiKey(data),
    onSuccess: async (data) => {
      await queryClient.invalidateQueries({ queryKey: apiKeyKeys.pages() });
      setCreatedApiKey(data);
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.apiKey.createSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.apiKey.createError"),
        color: "red",
      });
    },
  });

  const revokeApiKeyMutation = useMutation({
    mutationFn: (id: string) => revokeApiKey(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: apiKeyKeys.pages() });
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.apiKey.deleteSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.apiKey.deleteError"),
        color: "red",
      });
    },
  });

  const openCreate = () => {
    setFormOpened(true);
  };

  const closeForm = () => {
    setFormOpened(false);
  };

  const handleSubmit = async (data: ApiKeyFormValues) => {
    await createApiKeyMutation.mutateAsync(data);
    closeForm();
  };

  const handleRevoke = async () => {
    if (!deleteTarget) return;
    await revokeApiKeyMutation.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  const apiKeys = apiKeysQuery.data?.items ?? [];
  const totalPages = apiKeysQuery.data?.totalPages ?? 0;
  const submitting =
    createApiKeyMutation.isPending || revokeApiKeyMutation.isPending;

  return {
    t,
    apiKeys,
    totalPages,
    page,
    setPage,
    pageSize,
    loading: apiKeysQuery.isLoading,
    formOpened,
    createdApiKey,
    setCreatedApiKey,
    deleteTarget,
    setDeleteTarget,
    submitting,
    openCreate,
    closeForm,
    handleSubmit,
    handleRevoke,
  };
}
