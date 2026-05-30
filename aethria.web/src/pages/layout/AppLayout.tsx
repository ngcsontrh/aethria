import {
  AppShell,
  Burger,
  Group,
  Text,
  NavLink,
  ScrollArea,
  Avatar,
  Menu,
  Divider,
  Box,
  UnstyledButton,
  SegmentedControl,
  useComputedColorScheme,
  useMantineColorScheme,
  Popover,
  Indicator,
  Stack,
  Skeleton,
  ActionIcon,
  Tooltip,
} from "@mantine/core";
import { useEffect, useState } from "react";
import { Outlet, useLocation, useNavigate } from "@tanstack/react-router";
import {
  MessageSquare,
  GraduationCap,
  BookOpen,
  LogOut,
  Sun,
  Moon,
  Languages,
  Users,
  FileQuestion,
  Map,
  KeyRound,
  Bell,
  Check,
} from "lucide-react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  connectNotificationEvents,
  getPageNotifications,
  markNotificationAsRead,
} from "../../services";
import type {
  GetPageNotificationsResponse,
  NotificationPageItemResponse,
} from "../../services";
import { useTranslation as useI18nTranslation } from "react-i18next";
import { useAppLayout } from "./useAppLayout";
import { ChangePasswordModal } from "./ChangePasswordModal";
import logoIcon from "../../assets/logo-icon.png";
import {
  formatNotificationRelativeTime,
  getNotificationText,
} from "../notification/notificationDisplay";

export default function AppLayout() {
  const {
    t,
    user,
    mobileOpened,
    desktopOpened,
    toggleMobile,
    toggleDesktop,
    closeMobile,
    loggingOut,
    handleLogout,
    changePasswordOpened,
    openChangePassword,
    closeChangePassword,
  } = useAppLayout();

  const { setColorScheme } = useMantineColorScheme();
  const computedColorScheme = useComputedColorScheme("light", {
    getInitialValueInEffect: true,
  });
  const isDark = computedColorScheme === "dark";

  const { i18n } = useI18nTranslation();
  const currentLanguage = i18n.resolvedLanguage || i18n.language || "en";

  const location = useLocation();
  const navigate = useNavigate();

  const queryClient = useQueryClient();
  const [popoverOpened, setPopoverOpened] = useState(false);
  const [markingReadId, setMarkingReadId] = useState<string | null>(null);
  const userId = user?.userId;

  const headerNotificationsQuery = useQuery({
    queryKey: ["notifications", "header"],
    queryFn: ({ signal }) => getPageNotifications(1, 5, undefined, signal),
    enabled: !!user,
  });

  const headerNotifications = headerNotificationsQuery.data?.items ?? [];
  const headerNotificationsLoading = headerNotificationsQuery.isLoading;
  const unreadCount = headerNotificationsQuery.data?.items.filter((n) => !n.isRead).length ?? 0;

  const markAsReadMutation = useMutation({
    mutationFn: (id: string) => markNotificationAsRead(id),
    onMutate: (id) => {
      setMarkingReadId(id);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
    onSettled: () => {
      setMarkingReadId(null);
    },
  });

  const handleHeaderMarkAsRead = (id: string) => {
    markAsReadMutation.mutate(id);
  };

  useEffect(() => {
    if (!userId) return;

    let disposed = false;
    let stopConnection: (() => void) | null = null;

    connectNotificationEvents()
      .then((connection) => {
        if (disposed) {
          void connection.stop();
          return;
        }

        stopConnection = () => {
          void connection.stop();
        };

        connection.on(
          "NotificationCreated",
          (notification: NotificationPageItemResponse) => {
            queryClient.setQueryData(
              ["notifications", "header"],
              (current: GetPageNotificationsResponse | undefined) => {
                if (!current) return current;

                return {
                  ...current,
                  items: [notification, ...current.items].slice(0, 5),
                  totalCount: current.totalCount + 1,
                };
              },
            );
            void queryClient.invalidateQueries({ queryKey: ["notifications"] });
          },
        );
      })
      .catch((error) => {
        console.error("Failed to connect notification SignalR hub:", error);
      });

    return () => {
      disposed = true;
      stopConnection?.();
    };
  }, [queryClient, userId]);

  const chatSubItems = [
    {
      label: t("layout.nav.generalChat"),
      path: "/chat/general",
      icon: <MessageSquare size={16} />,
    },
    {
      label: t("layout.nav.mentorChat"),
      path: "/chat/mentor",
      icon: <GraduationCap size={16} />,
    },
    {
      label: t("layout.nav.resourceChat"),
      path: "/chat/resource",
      icon: <BookOpen size={16} />,
    },
  ];

  const isChatActive = location.pathname.startsWith("/chat");
  const isMentorActive = location.pathname.startsWith("/mentors");
  const isResourceActive = location.pathname.startsWith("/resources");
  const isQuizActive = location.pathname.startsWith("/quizzes");
  const isRoadmapActive = location.pathname.startsWith("/roadmaps");
  const isApiKeyActive = location.pathname.startsWith("/api-keys");

  const userInitials = user?.email ? user.email.slice(0, 2).toUpperCase() : "U";

  return (
    <AppShell
      layout="alt"
      header={{ height: 52 }}
      navbar={{
        width: 240,
        breakpoint: "sm",
        collapsed: { mobile: !mobileOpened, desktop: !desktopOpened },
      }}
      padding={0}
    >
      {}
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          {}
          <Group>
            <Burger
              opened={mobileOpened}
              onClick={toggleMobile}
              hiddenFrom="sm"
              size="sm"
            />
            {!desktopOpened && (
              <Burger
                opened={desktopOpened}
                onClick={toggleDesktop}
                visibleFrom="sm"
                size="sm"
              />
            )}
            {!desktopOpened ? (
              <Group gap="xs">
                <img
                  src={logoIcon}
                  alt="Aethria Logo"
                  style={{ height: 24, width: 24, objectFit: "contain" }}
                />
                <Text fw={700} size="lg" style={{ letterSpacing: "-0.02em" }}>
                  {t("layout.title")}
                </Text>
              </Group>
            ) : (
              <Group gap="xs" hiddenFrom="sm">
                <img
                  src={logoIcon}
                  alt="Aethria Logo"
                  style={{ height: 24, width: 24, objectFit: "contain" }}
                />
                <Text fw={700} size="lg" style={{ letterSpacing: "-0.02em" }}>
                  {t("layout.title")}
                </Text>
              </Group>
            )}
          </Group>

          {}
          <Group gap={8}>
            {/* Notification Bell Menu/Popover */}
            <Popover
              width={320}
              position="bottom-end"
              withArrow
              shadow="md"
              opened={popoverOpened}
              onChange={setPopoverOpened}
            >
              <Popover.Target>
                <Indicator
                  disabled={unreadCount === 0}
                  color="red"
                  size={16}
                  withBorder
                  label={unreadCount > 9 ? "9+" : unreadCount}
                >
                  <ActionIcon
                    variant="subtle"
                    color="gray"
                    radius="xl"
                    size="lg"
                    onClick={() => setPopoverOpened((o) => !o)}
                  >
                    <Bell size={20} />
                  </ActionIcon>
                </Indicator>
              </Popover.Target>
              <Popover.Dropdown p="xs">
                <Box
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    marginBottom: "8px",
                  }}
                >
                  <Text fw={700} size="sm">
                    {t("notification.title")}
                  </Text>
                </Box>
                <Divider mb="xs" />
                <ScrollArea style={{ maxHeight: 300 }} type="hover">
                  {headerNotificationsLoading && headerNotifications.length === 0 ? (
                    <Stack gap="xs">
                      <Skeleton height={40} radius="sm" />
                      <Skeleton height={40} radius="sm" />
                      <Skeleton height={40} radius="sm" />
                    </Stack>
                  ) : headerNotifications.length === 0 ? (
                    <Text size="xs" c="dimmed" ta="center" py="md">
                      {t("notification.empty")}
                    </Text>
                  ) : (
                    <Stack gap="xs">
                      {headerNotifications.map((n) => (
                        (() => {
                          const text = getNotificationText(n, t);

                          return (
                            <Box
                              key={n.id}
                              p="xs"
                              style={{
                                backgroundColor: n.isRead
                                  ? "transparent"
                                  : "var(--mantine-color-blue-light)",
                                borderRadius: "var(--mantine-radius-sm)",
                                display: "flex",
                                justifyContent: "space-between",
                                alignItems: "center",
                                gap: "8px",
                                cursor: "pointer",
                              }}
                              onClick={() => {
                                if (!n.isRead) {
                                  handleHeaderMarkAsRead(n.id);
                                }
                              }}
                            >
                              <div style={{ flex: 1, minWidth: 0 }}>
                                <Text size="xs" fw={700} c={n.isRead ? "dimmed" : "blue"}>
                                  {text.title}
                                </Text>
                                <Text size="xs" style={{ wordBreak: "break-word" }} lineClamp={2}>
                                  {text.message}
                                </Text>
                                <Text size="10px" c="dimmed">
                                  {formatNotificationRelativeTime(n.createdAt, currentLanguage)}
                                </Text>
                              </div>
                              {!n.isRead && (
                                <Tooltip label={t("notification.markAsRead")}>
                                  <ActionIcon
                                    size="xs"
                                    variant="subtle"
                                    color="blue"
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      handleHeaderMarkAsRead(n.id);
                                    }}
                                    loading={markingReadId === n.id}
                                  >
                                    <Check size={12} />
                                  </ActionIcon>
                                </Tooltip>
                              )}
                            </Box>
                          );
                        })()
                      ))}
                    </Stack>
                  )}
                </ScrollArea>
                <Divider my="xs" />
                <UnstyledButton
                  style={{
                    display: "block",
                    width: "100%",
                    textAlign: "center",
                    padding: "4px 0",
                  }}
                  onClick={() => {
                    setPopoverOpened(false);
                    navigate({ to: "/notifications" });
                  }}
                >
                  <Text size="xs" color="blue" fw={500}>
                    {t("notification.viewAll")}
                  </Text>
                </UnstyledButton>
              </Popover.Dropdown>
            </Popover>

            <Menu
              shadow="md"
              width={240}
              position="bottom-end"
              closeOnItemClick={false}
            >
              <Menu.Target>
                <UnstyledButton>
                  <Avatar
                    size={30}
                    radius="xl"
                    color="blue"
                    style={{ cursor: "pointer" }}
                  >
                    {userInitials}
                  </Avatar>
                </UnstyledButton>
              </Menu.Target>
              <Menu.Dropdown>
                {}
                <Menu.Label>
                  <Text size="xs" fw={500}>
                    {t("layout.user.account")}
                  </Text>
                  <Text size="xs" c="dimmed" style={{ wordBreak: "break-all" }}>
                    {user?.email}
                  </Text>
                </Menu.Label>

                <Menu.Item
                  leftSection={<KeyRound size={14} />}
                  onClick={openChangePassword}
                  closeMenuOnClick
                  id="menu-change-password"
                >
                  {t("changePassword.menuItem")}
                </Menu.Item>

                <Divider my={4} />

                {}
                <Menu.Label>
                  <Group gap={6}>
                    {isDark ? <Moon size={12} /> : <Sun size={12} />}
                    <Text size="xs">{t("layout.user.appearance")}</Text>
                  </Group>
                </Menu.Label>
                <Box px={10} pb={6}>
                  <SegmentedControl
                    fullWidth
                    size="xs"
                    value={computedColorScheme}
                    onChange={(val) => setColorScheme(val as "light" | "dark")}
                    data={[
                      {
                        value: "light",
                        label: (
                          <Group gap={4} justify="center">
                            <Sun size={12} />
                            <span>{t("auth.common.appearance.light")}</span>
                          </Group>
                        ),
                      },
                      {
                        value: "dark",
                        label: (
                          <Group gap={4} justify="center">
                            <Moon size={12} />
                            <span>{t("auth.common.appearance.dark")}</span>
                          </Group>
                        ),
                      },
                    ]}
                  />
                </Box>

                <Divider my={4} />

                {}
                <Menu.Label>
                  <Group gap={6}>
                    <Languages size={12} />
                    <Text size="xs">{t("layout.user.language")}</Text>
                  </Group>
                </Menu.Label>
                <Box px={10} pb={6}>
                  <SegmentedControl
                    fullWidth
                    size="xs"
                    value={currentLanguage}
                    onChange={(val) => i18n.changeLanguage(val)}
                    data={[
                      { value: "vi", label: "VI" },
                      { value: "en", label: "EN" },
                    ]}
                  />
                </Box>

                <Divider my={4} />

                {}
                <Menu.Item
                  color="red"
                  leftSection={<LogOut size={14} />}
                  onClick={handleLogout}
                  disabled={loggingOut}
                  closeMenuOnClick
                >
                  {t("layout.user.logout")}
                </Menu.Item>
              </Menu.Dropdown>
            </Menu>

            <ChangePasswordModal
              opened={changePasswordOpened}
              onClose={closeChangePassword}
            />
          </Group>
        </Group>
      </AppShell.Header>

      {}
      <AppShell.Navbar
        style={{ overflow: "hidden", display: "flex", flexDirection: "column" }}
      >
        <Group
          h={52}
          px="md"
          justify="space-between"
          style={{
            borderBottom: "1px solid var(--mantine-color-default-border)",
          }}
        >
          <Group gap="xs">
            <img
              src={logoIcon}
              alt="Aethria Logo"
              style={{ height: 24, width: 24, objectFit: "contain" }}
            />
            <Text fw={700} size="lg" style={{ letterSpacing: "-0.02em" }}>
              {t("layout.title")}
            </Text>
          </Group>
          <Burger
            opened={desktopOpened}
            onClick={toggleDesktop}
            visibleFrom="sm"
            size="sm"
          />
          <Burger
            opened={mobileOpened}
            onClick={toggleMobile}
            hiddenFrom="sm"
            size="sm"
          />
        </Group>

        <ScrollArea style={{ flex: 1 }} scrollbarSize={6}>
          <Box p="xs" pt="xs">
            <NavLink
              label={t("layout.nav.chat")}
              leftSection={<MessageSquare size={18} />}
              defaultOpened={isChatActive}
              className="sidebar-nav-link"
              childrenOffset={16}
            >
              {chatSubItems.map((item) => {
                const isActive = location.pathname.startsWith(item.path);
                return (
                  <NavLink
                    key={item.path}
                    label={item.label}
                    leftSection={item.icon}
                    active={isActive}
                    onClick={() => {
                      navigate({ to: item.path });
                      closeMobile();
                    }}
                    className="sidebar-nav-link"
                  />
                );
              })}
            </NavLink>
            <NavLink
              label={t("layout.nav.mentors")}
              leftSection={<Users size={18} />}
              active={isMentorActive}
              onClick={() => {
                navigate({ to: "/mentors" });
                closeMobile();
              }}
              className="sidebar-nav-link"
            />
            <NavLink
              label={t("layout.nav.resources")}
              leftSection={<BookOpen size={18} />}
              active={isResourceActive}
              onClick={() => {
                navigate({ to: "/resources" });
                closeMobile();
              }}
              className="sidebar-nav-link"
            />
            <NavLink
              label={t("layout.nav.quizzes")}
              leftSection={<FileQuestion size={18} />}
              active={isQuizActive}
              onClick={() => {
                navigate({ to: "/quizzes" });
                closeMobile();
              }}
              className="sidebar-nav-link"
            />
            <NavLink
              label={t("layout.nav.roadmaps")}
              leftSection={<Map size={18} />}
              active={isRoadmapActive}
              onClick={() => {
                navigate({ to: "/roadmaps" });
                closeMobile();
              }}
              className="sidebar-nav-link"
            />
            <NavLink
              label={t("layout.nav.apiKeys")}
              leftSection={<KeyRound size={18} />}
              active={isApiKeyActive}
              onClick={() => {
                navigate({ to: "/api-keys" });
                closeMobile();
              }}
              className="sidebar-nav-link"
            />
          </Box>
        </ScrollArea>
      </AppShell.Navbar>

      <AppShell.Main
        style={{
          display: "flex",
          flexDirection: "column",
          height: "calc(100vh - 52px)",
          overflow: "hidden",
        }}
      >
        <Outlet />
      </AppShell.Main>
    </AppShell>
  );
}
