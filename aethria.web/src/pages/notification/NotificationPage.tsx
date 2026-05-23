import { useState } from "react";
import {
  Box,
  Title,
  Group,
  Pagination,
  Text,
  LoadingOverlay,
  Skeleton,
  Stack,
  Card,
  Avatar,
  ActionIcon,
  Tooltip,
  Badge,
  Button,
  SegmentedControl,
} from "@mantine/core";
import {
  Bell,
  Check,
  CheckCheck,
  Inbox,
} from "lucide-react";
import { useNotificationPage } from "./useNotificationPage";
import {
  formatNotificationRelativeTime,
  getNotificationConfig,
  getNotificationText,
} from "./notificationDisplay";

export default function NotificationPage() {
  const {
    t,
    language,
    notifications,
    totalPages,
    page,
    setPage,
    filter,
    setFilter,
    loading,
    markingManyAsRead,
    handleMarkAsRead,
    handleMarkManyAsRead,
  } = useNotificationPage();

  const [hoveredId, setHoveredId] = useState<string | null>(null);

  // Notifications are filtered server-side
  const filteredNotifications = notifications;

  const unreadOnPage = notifications.filter((n) => !n.isRead);
  const hasUnreadOnPage = unreadOnPage.length > 0;

  const handleMarkPageAsRead = async () => {
    if (!hasUnreadOnPage) return;
    await handleMarkManyAsRead(unreadOnPage.map((n) => n.id));
  };

  return (
    <Box p="md" style={{ overflow: "auto", flex: 1 }}>
      {/* Title & Page Action Row */}
      <Group justify="space-between" mb="md" wrap="wrap" gap="sm">
        <Group gap="xs">
          <Bell size={22} style={{ color: "var(--mantine-color-blue-filled)" }} />
          <Title order={3}>{t("notification.title")}</Title>
        </Group>

        {hasUnreadOnPage && (
          <Button
            leftSection={<CheckCheck size={16} />}
            variant="light"
            color="blue"
            size="xs"
            onClick={handleMarkPageAsRead}
            loading={markingManyAsRead}
          >
            {t("notification.markPageRead")}
          </Button>
        )}
      </Group>

      {/* Filter Tabs */}
      <SegmentedControl
        value={filter}
        onChange={setFilter}
        mb="md"
        size="xs"
        data={[
          { label: t("notification.filter.all"), value: "all" },
          {
            label: t("notification.filter.unread"),
            value: "unread",
          },
          {
            label: t("notification.filter.read"),
            value: "read",
          },
        ]}
      />

      <Box pos="relative" mih={200}>
        <LoadingOverlay
          visible={loading && notifications.length > 0}
          overlayProps={{ radius: "sm", blur: 1.5 }}
        />

        {loading && notifications.length === 0 ? (
          <Stack gap="sm">
            {Array(5)
              .fill(0)
              .map((_, index) => (
                <Box
                  key={`skeleton-${index}`}
                  p="md"
                  style={{
                    border: "1px solid var(--mantine-color-default-border)",
                    borderRadius: "var(--mantine-radius-md)",
                    display: "flex",
                    gap: "16px",
                  }}
                >
                  <Skeleton height={40} circle />
                  <div style={{ flex: 1 }}>
                    <Skeleton height={16} radius="sm" width="30%" mb="sm" />
                    <Skeleton height={14} radius="sm" width="80%" />
                  </div>
                </Box>
              ))}
          </Stack>
        ) : filteredNotifications.length === 0 ? (
          <Card
            withBorder
            radius="md"
            p="xl"
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              justifyContent: "center",
              minHeight: 250,
              backgroundColor: "transparent",
              borderStyle: "dashed",
            }}
          >
            <Avatar size="xl" radius="xl" color="gray" variant="light" mb="md">
              <Inbox size={32} />
            </Avatar>
            <Text ta="center" c="dimmed" fw={500}>
              {t("notification.empty")}
            </Text>
          </Card>
        ) : (
          <Stack gap="sm">
            {filteredNotifications.map((n) => {
              const config = getNotificationConfig(n.type);
              const text = getNotificationText(n, t);
              const IconComponent = config.icon;

              return (
                <Card
                  key={n.id}
                  withBorder
                  radius="md"
                  p="md"
                  style={{
                    position: "relative",
                    cursor: n.isRead ? "default" : "pointer",
                    transition: "all 0.2s cubic-bezier(0.16, 1, 0.3, 1)",
                    backgroundColor: n.isRead
                      ? "var(--mantine-color-default)"
                      : "var(--mantine-color-blue-light)",
                    borderLeft: n.isRead
                      ? "1px solid var(--mantine-color-default-border)"
                      : `4px solid var(--mantine-color-${config.color}-filled)`,
                    transform:
                      hoveredId === n.id && !n.isRead ? "translateY(-2px)" : "none",
                    boxShadow:
                      hoveredId === n.id && !n.isRead
                        ? "var(--mantine-shadow-md)"
                        : "var(--mantine-shadow-xs)",
                  }}
                  onMouseEnter={() => setHoveredId(n.id)}
                  onMouseLeave={() => setHoveredId(null)}
                  onClick={() => {
                    if (!n.isRead) {
                      handleMarkAsRead(n.id);
                    }
                  }}
                >
                  <Group gap="md" wrap="nowrap" align="flex-start">
                    <Avatar
                      color={config.color}
                      radius="xl"
                      size="md"
                      variant={n.isRead ? "light" : "filled"}
                    >
                      <IconComponent size={18} />
                    </Avatar>

                    <div style={{ flex: 1, minWidth: 0 }}>
                        <Group justify="space-between" mb={4} wrap="wrap" gap="xs">
                        <Group gap="xs">
                          <Text
                            fw={700}
                            size="sm"
                            c={
                              n.isRead
                                ? "dimmed"
                                : "var(--mantine-color-text)"
                            }
                          >
                            {text.title}
                          </Text>
                          {!n.isRead && (
                            <Badge size="xs" color={config.color} variant="filled">
                              {t("notification.unreadStatus")}
                            </Badge>
                          )}
                        </Group>
                        <Text size="xs" c="dimmed">
                          {formatNotificationRelativeTime(n.createdAt, language)}
                        </Text>
                      </Group>

                      <Text
                        size="sm"
                        c={
                          n.isRead
                            ? "dimmed"
                            : "var(--mantine-color-text)"
                        }
                        style={{ lineHeight: 1.5 }}
                      >
                        {text.message}
                      </Text>
                    </div>

                    {!n.isRead && (
                      <Tooltip label={t("notification.markAsRead")}>
                        <ActionIcon
                          variant="subtle"
                          color="gray"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleMarkAsRead(n.id);
                          }}
                          aria-label={t("notification.markAsRead")}
                        >
                          <Check size={16} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                  </Group>
                </Card>
              );
            })}
          </Stack>
        )}
      </Box>

      {totalPages > 1 && (
        <Group justify="center" mt="xl">
          <Pagination total={totalPages} value={page} onChange={setPage} />
        </Group>
      )}
    </Box>
  );
}
