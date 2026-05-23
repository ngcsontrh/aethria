import { api } from "../core/client";
import type { GetPageNotificationsResponse } from "./types";

export async function getPageNotifications(
  pageNumber = 1,
  pageSize = 10,
  isRead?: boolean,
  signal?: AbortSignal,
): Promise<GetPageNotificationsResponse> {
  const response = await api.get<GetPageNotificationsResponse>("/api/notifications", {
    params: { pageNumber, pageSize, isRead },
    signal,
  });
  return response.data;
}

export async function markNotificationAsRead(
  id: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.patch(`/api/notifications/${id}/read`, null, { signal });
}

export async function markNotificationsAsRead(
  notificationIds: string[],
  signal?: AbortSignal,
): Promise<void> {
  await api.patch(
    "/api/notifications/read",
    { notificationIds },
    { signal },
  );
}
