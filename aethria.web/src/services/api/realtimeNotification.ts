import { createSignalRConnection } from "../core/signalr";
import type { HubConnection } from "@microsoft/signalr";
import type { NotificationPageItemResponse } from "./types";

export function connectNotificationEvents(): Promise<HubConnection> {
  return createSignalRConnection("/hubs/notifications");
}

export type NotificationCreatedHandler = (
  notification: NotificationPageItemResponse,
) => void;
