import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import {
  getPageNotifications,
  markNotificationAsRead,
  markNotificationsAsRead,
} from "../../services";

const notificationKeys = {
  all: ["notifications"] as const,
  pages: () => [...notificationKeys.all, "page"] as const,
  page: (page: number, pageSize: number, isRead?: boolean) =>
    [...notificationKeys.pages(), page, pageSize, isRead] as const,
};

export function useNotificationPage() {
  const { t, i18n } = useTranslation();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState<string>("all");
  const pageSize = 10;

  const isRead = filter === "all" ? undefined : filter === "read";

  const notificationsQuery = useQuery({
    queryKey: notificationKeys.page(page, pageSize, isRead),
    queryFn: ({ signal }) => getPageNotifications(page, pageSize, isRead, signal),
  });

  const markAsReadMutation = useMutation({
    mutationFn: (id: string) => markNotificationAsRead(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: notificationKeys.all });
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.notification.markReadSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.notification.markReadError"),
        color: "red",
      });
    },
  });

  const markManyAsReadMutation = useMutation({
    mutationFn: (ids: string[]) => markNotificationsAsRead(ids),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: notificationKeys.all });
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.notification.markReadSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.notification.markReadError"),
        color: "red",
      });
    },
  });

  const handleMarkAsRead = async (id: string) => {
    await markAsReadMutation.mutateAsync(id);
  };

  const handleMarkManyAsRead = async (ids: string[]) => {
    await markManyAsReadMutation.mutateAsync(ids);
  };

  const notificationItems = notificationsQuery.data?.items ?? [];
  const totalPages = notificationsQuery.data?.totalPages ?? 0;
  const loading = notificationsQuery.isLoading;

  return {
    t,
    language: i18n.resolvedLanguage || i18n.language || "en",
    notifications: notificationItems,
    totalPages,
    page,
    setPage,
    filter,
    setFilter,
    loading,
    submitting: markAsReadMutation.isPending,
    markingManyAsRead: markManyAsReadMutation.isPending,
    handleMarkAsRead,
    handleMarkManyAsRead,
  };
}
