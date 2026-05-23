import type { TFunction } from "i18next";
import {
  Bell,
  Brain,
  BookOpen,
  Map as MapIcon,
  type LucideIcon,
} from "lucide-react";
import type { NotificationPageItemResponse } from "../../services";

interface NotificationConfig {
  color: string;
  icon: LucideIcon;
}

const notificationConfigByType: Record<string, NotificationConfig> = {
  "quiz.generated": { color: "purple", icon: Brain },
  "roadmap.generated": { color: "teal", icon: MapIcon },
  "resource.created": { color: "blue", icon: BookOpen },
};

export function getNotificationConfig(type: string): NotificationConfig {
  return notificationConfigByType[type] ?? { color: "blue", icon: Bell };
}

export function getNotificationText(
  notification: NotificationPageItemResponse,
  t: TFunction,
) {
  const baseKey = `notification.items.${notification.type}`;
  const fallbackTitle = t("notification.items.fallback.title");
  const fallbackMessage = t("notification.items.fallback.message");

  return {
    title: t(`${baseKey}.title`, {
      ...notification.data,
      defaultValue: fallbackTitle,
    }),
    message: t(`${baseKey}.message`, {
      ...notification.data,
      defaultValue: fallbackMessage,
    }),
  };
}

export function formatNotificationRelativeTime(
  dateString: string,
  language: string,
) {
  const date = new Date(dateString);
  const seconds = Math.round((date.getTime() - Date.now()) / 1000);
  const absoluteSeconds = Math.abs(seconds);
  const formatter = new Intl.RelativeTimeFormat(language, {
    numeric: "auto",
    style: "short",
  });

  if (absoluteSeconds < 60) {
    return formatter.format(0, "second");
  }

  const minutes = Math.round(seconds / 60);
  if (Math.abs(minutes) < 60) {
    return formatter.format(minutes, "minute");
  }

  const hours = Math.round(minutes / 60);
  if (Math.abs(hours) < 24) {
    return formatter.format(hours, "hour");
  }

  const days = Math.round(hours / 24);
  if (Math.abs(days) < 7) {
    return formatter.format(days, "day");
  }

  return new Intl.DateTimeFormat(language).format(date);
}
