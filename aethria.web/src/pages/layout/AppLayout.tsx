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
} from "@mantine/core";
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
} from "lucide-react";
import { useTranslation as useI18nTranslation } from "react-i18next";
import { useAppLayout } from "./useAppLayout";
import { ChangePasswordModal } from "./ChangePasswordModal";
import logoIcon from "../../assets/logo-icon.png";

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
            {}
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
