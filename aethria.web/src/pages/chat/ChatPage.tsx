import { useState, useEffect, useRef } from "react";
import {
  Box,
  Group,
  Text,
  NavLink,
  ScrollArea,
  Textarea,
  ActionIcon,
  Loader,
  Stack,
  Paper,
  Badge,
  Button,
  Modal,
  Divider,
  Tooltip,
  Center,
  SimpleGrid,
  Card,
  Menu,
  Checkbox,
  Title,
  ThemeIcon,
} from "@mantine/core";
import { useMediaQuery } from "@mantine/hooks";
import {
  Send,
  Square,
  Plus,
  Trash2,
  Trash,
  MessageSquare,
  GraduationCap,
  BookOpen,
  ChevronLeft,
  Wrench,
  Search,
  FileText,
  PanelLeftClose,
  PanelLeftOpen,
} from "lucide-react";
import type { ChatMode } from "./useChatPage";
import { useChatPage } from "./useChatPage";

interface ChatPageProps {
  mode: ChatMode;
}

const getToolIcon = (toolId: string) => {
  switch (toolId) {
    case "web_search":
      return <Search size={14} />;
    case "web_extract":
      return <FileText size={14} />;
    default:
      return <Wrench size={14} />;
  }
};

export default function ChatPage({ mode }: ChatPageProps) {
  const {
    t,
    targetId,
    targetName,
    selectTarget,
    clearTarget,
    mentors,
    resources,
    listLoading,
    sessions,
    sessionsLoading,
    activeSessionId,
    selectSession,
    startNewChat,
    messages,
    messagesLoading,
    messagesEndRef,
    inputValue,
    setInputValue,
    sendMessage,
    streaming,
    stopStreaming,
    deleteTargetSessionId,
    confirmDeleteSession,
    deleteSession,
    cancelDeleteSession,
    showDeleteAllConfirm,
    setShowDeleteAllConfirm,
    deleteAllSessions,
    availableTools,
    enabledTools,
    toggleTool,
  } = useChatPage({ mode });

  const needsTarget = mode !== "general" && !targetId;

  const isMobile = useMediaQuery("(max-width: 48em)") ?? false;

  const inputRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    if (!needsTarget && !streaming && inputRef.current) {
      inputRef.current.focus();
    }
  }, [needsTarget, activeSessionId, targetId, streaming]);

  const [historyOpened, setHistoryOpened] = useState(() => {
    if (typeof window !== "undefined") {
      const saved = localStorage.getItem("chat_history_opened");
      if (saved !== null) return saved === "true";
    }
    return true;
  });

  /* eslint-disable react-hooks/set-state-in-effect */
  useEffect(() => {
    if (isMobile) {
      setHistoryOpened(false);
    } else {
      const saved = localStorage.getItem("chat_history_opened");
      setHistoryOpened(saved !== "false");
    }
  }, [isMobile]);
  /* eslint-enable react-hooks/set-state-in-effect */

  const toggleHistory = () => {
    setHistoryOpened((prev) => {
      const next = !prev;
      if (!isMobile) {
        localStorage.setItem("chat_history_opened", String(next));
      }
      return next;
    });
  };

  if (needsTarget) {
    const isMentor = mode === "mentor";
    const items = isMentor ? mentors : resources;
    const title = isMentor
      ? t("chat.selectors.selectMentor")
      : t("chat.selectors.selectResource");
    const subtitle = isMentor
      ? t("chat.selectors.mentorSubtitle")
      : t("chat.selectors.resourceSubtitle");
    const emptyMsg = isMentor
      ? t("chat.selectors.noMentors")
      : t("chat.selectors.noResources");
    const ModeIcon = isMentor ? GraduationCap : BookOpen;

    return (
      <Box p="xl" style={{ height: "100%", overflowY: "auto" }}>
        <Group mb="md" gap="md">
          <ThemeIcon size={44} variant="light" radius="md">
            <ModeIcon size={24} />
          </ThemeIcon>
          <Box>
            <Title order={2} style={{ fontSize: "1.5rem", fontWeight: 700 }}>
              {title}
            </Title>
            <Text c="dimmed" size="sm" mt={2}>
              {subtitle}
            </Text>
          </Box>
        </Group>

        <Divider mb="xl" />

        {listLoading ? (
          <Center h={200}>
            <Loader size="sm" />
          </Center>
        ) : items.length === 0 ? (
          <Center h={200}>
            <Text c="dimmed">{emptyMsg}</Text>
          </Center>
        ) : (
          <SimpleGrid
            cols={{ base: 1, sm: 2, md: 3 }}
            spacing="lg"
            verticalSpacing="lg"
          >
            {(isMentor ? mentors : resources).map((item) => {
              const name = item.name;
              const description =
                "description" in item && item.description
                  ? item.description
                  : "";

              const fileType =
                "fileType" in item
                  ? (item as { fileType: string }).fileType
                  : null;
              const fileSize =
                "fileSize" in item
                  ? (item as { fileSize: number }).fileSize
                  : null;

              return (
                <Card
                  key={item.id}
                  withBorder
                  padding="lg"
                  radius="lg"
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    justifyContent: "space-between",
                    height: "100%",
                    boxShadow: "var(--mantine-shadow-xs)",
                  }}
                  className="selector-card"
                >
                  <Box
                    style={{
                      flex: 1,
                      display: "flex",
                      flexDirection: "column",
                    }}
                  >
                    <Group
                      mb="sm"
                      justify="space-between"
                      align="flex-start"
                      wrap="nowrap"
                    >
                      <Text
                        fw={700}
                        size="md"
                        lineClamp={2}
                        style={{ flex: 1, lineHeight: 1.3 }}
                      >
                        {name}
                      </Text>
                      {fileType && (
                        <Badge size="sm" variant="light" color="blue">
                          {fileType.toUpperCase()}
                        </Badge>
                      )}
                    </Group>
                    {description && (
                      <Text c="dimmed" size="sm" lineClamp={3} mb="md">
                        {description}
                      </Text>
                    )}
                    {fileSize !== null && (
                      <Text
                        c="dimmed"
                        size="xs"
                        mb="md"
                        style={{ marginTop: "auto" }}
                      >
                        {t("chat.selectors.fileSize", {
                          size: Math.round(fileSize / 1024),
                        })}
                      </Text>
                    )}
                  </Box>
                  <Button
                    size="sm"
                    variant="light"
                    fullWidth
                    leftSection={<MessageSquare size={15} />}
                    onClick={() => selectTarget(item.id, name)}
                    mt={fileSize === null ? "auto" : 0}
                    style={{ fontWeight: 600 }}
                  >
                    {t("chat.selectors.startChat")}
                  </Button>
                </Card>
              );
            })}
          </SimpleGrid>
        )}
      </Box>
    );
  }

  return (
    <Box
      style={{
        display: "flex",
        height: "100%",
        overflow: "hidden",
        position: "relative",
      }}
    >
      {/* Mobile Drawer Overlay Backdrop */}
      {isMobile && historyOpened && (
        <Box
          style={{
            position: "absolute",
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: "rgba(0, 0, 0, 0.4)",
            zIndex: 99,
          }}
          onClick={toggleHistory}
        />
      )}

      {/* Sidebar (History) */}
      <Box
        style={{
          position: isMobile ? "absolute" : "relative",
          zIndex: isMobile ? 100 : 1,
          left: isMobile && !historyOpened ? -240 : 0,
          top: 0,
          bottom: 0,
          backgroundColor: "var(--mantine-color-body)",
          width: isMobile ? 240 : historyOpened ? 240 : 0,
          minWidth: isMobile ? undefined : historyOpened ? 240 : 0,
          maxWidth: 240,
          opacity: isMobile ? 1 : historyOpened ? 1 : 0,
          borderRight:
            (isMobile && !historyOpened) || (!isMobile && !historyOpened)
              ? "none"
              : "1px solid var(--mantine-color-default-border)",
          display: "flex",
          flexDirection: "column",
          height: "100%",
          transition:
            "left 200ms cubic-bezier(0.4, 0, 0.2, 1), width 200ms cubic-bezier(0.4, 0, 0.2, 1), min-width 200ms cubic-bezier(0.4, 0, 0.2, 1), opacity 200ms cubic-bezier(0.4, 0, 0.2, 1), border-right 200ms cubic-bezier(0.4, 0, 0.2, 1)",
          overflow: "hidden",
          boxShadow:
            isMobile && historyOpened ? "var(--mantine-shadow-md)" : "none",
        }}
      >
        <Group
          px="md"
          py="xs"
          justify="space-between"
          align="center"
          style={{
            borderBottom: "1px solid var(--mantine-color-default-border)",
            height: 52,
            minHeight: 52,
          }}
        >
          <Text size="xs" fw={600} c="dimmed" tt="uppercase">
            {t("chat.sessions.title")}
          </Text>
          <Group gap={4}>
            {sessions.length > 0 && (
              <Tooltip
                label={t("chat.sessions.deleteConfirmTitle")}
                position="bottom"
                withArrow
              >
                <ActionIcon
                  size="sm"
                  variant="subtle"
                  color="red"
                  onClick={() => setShowDeleteAllConfirm(true)}
                >
                  <Trash size={13} />
                </ActionIcon>
              </Tooltip>
            )}
            <Tooltip label={t("chat.sessions.new")} position="bottom" withArrow>
              <ActionIcon size="sm" variant="subtle" onClick={startNewChat}>
                <Plus size={14} />
              </ActionIcon>
            </Tooltip>
          </Group>
        </Group>

        <ScrollArea style={{ flex: 1 }} scrollbarSize={4}>
          {sessionsLoading ? (
            <Center h={80}>
              <Loader size="xs" />
            </Center>
          ) : sessions.length === 0 ? (
            <Center h={80}>
              <Text size="xs" c="dimmed">
                {t("chat.sessions.empty")}
              </Text>
            </Center>
          ) : (
            <Stack gap={0} p={4}>
              {sessions.map((session) => (
                <Box
                  key={session.id}
                  style={{ position: "relative" }}
                  className="session-item"
                >
                  <NavLink
                    label={
                      <Text size="xs" lineClamp={1}>
                        {session.title}
                      </Text>
                    }
                    active={activeSessionId === session.id}
                    onClick={() => {
                      selectSession(session.id);
                      if (isMobile) {
                        setHistoryOpened(false);
                      }
                    }}
                    style={{
                      borderRadius: "var(--mantine-radius-sm)",
                      paddingRight: 28,
                    }}
                    rightSection={
                      <ActionIcon
                        size="xs"
                        variant="subtle"
                        color="red"
                        onClick={(e) => {
                          e.stopPropagation();
                          confirmDeleteSession(session.id);
                        }}
                      >
                        <Trash2 size={11} />
                      </ActionIcon>
                    }
                  />
                </Box>
              ))}
            </Stack>
          )}
        </ScrollArea>
      </Box>

      {/* Main Chat Panel */}
      <Box
        style={{
          flex: 1,
          display: "flex",
          flexDirection: "column",
          height: "100%",
          overflow: "hidden",
        }}
      >
        {/* Chat Panel Header */}
        <Group
          px="md"
          py="xs"
          justify="space-between"
          align="center"
          style={{
            borderBottom: "1px solid var(--mantine-color-default-border)",
            height: 52,
            minHeight: 52,
          }}
        >
          <Group gap="sm">
            <Tooltip
              label={
                historyOpened
                  ? t("chat.sessions.toggleCollapse")
                  : t("chat.sessions.toggleExpand")
              }
              position="bottom"
              withArrow
            >
              <ActionIcon
                variant="subtle"
                size="md"
                onClick={toggleHistory}
                color="gray"
              >
                {historyOpened ? (
                  <PanelLeftClose size={18} />
                ) : (
                  <PanelLeftOpen size={18} />
                )}
              </ActionIcon>
            </Tooltip>

            <Text fw={600} size="sm" lineClamp={1}>
              {activeSessionId
                ? sessions.find((s) => s.id === activeSessionId)?.title ||
                  t("chat.sessions.unnamed")
                : t("chat.sessions.new")}
            </Text>
          </Group>

          {mode !== "general" && (
            <Button
              variant="subtle"
              size="xs"
              leftSection={<ChevronLeft size={14} />}
              onClick={clearTarget}
              px={8}
              style={{ fontWeight: 600 }}
            >
              {targetName}
            </Button>
          )}
        </Group>

        <ScrollArea style={{ flex: 1 }} p="md" scrollbarSize={6}>
          {messagesLoading ? (
            <Center h={200}>
              <Loader size="sm" />
            </Center>
          ) : messages.length === 0 ? (
            <Center h={200}>
              <Stack align="center" gap="xs">
                <ThemeIcon size="xl" variant="light" radius="xl" color="gray">
                  <MessageSquare size={22} />
                </ThemeIcon>
                <Text c="dimmed" size="sm" ta="center">
                  {t("chat.messages.empty")}
                </Text>
              </Stack>
            </Center>
          ) : (
            <Stack gap="md" p="xs">
              {messages.map((msg) => (
                <Box
                  key={msg.id}
                  style={{
                    display: "flex",
                    justifyContent:
                      msg.role === "user" ? "flex-end" : "flex-start",
                  }}
                >
                  <Paper
                    p="sm"
                    radius="md"
                    withBorder={msg.role === "assistant"}
                    style={{
                      maxWidth: "75%",
                      backgroundColor:
                        msg.role === "user"
                          ? "var(--mantine-color-blue-filled)"
                          : undefined,
                      color:
                        msg.role === "user"
                          ? "var(--mantine-color-white)"
                          : undefined,
                    }}
                  >
                    {msg.content === "" && msg.role === "assistant" ? (
                      <Group gap="xs">
                        <Loader size="xs" />
                        <Text size="sm" c="dimmed">
                          {t("chat.messages.loading")}
                        </Text>
                      </Group>
                    ) : (
                      <Text
                        size="sm"
                        c={msg.isError ? "red" : undefined}
                        style={{
                          whiteSpace: "pre-wrap",
                          wordBreak: "break-word",
                        }}
                      >
                        {msg.content}
                      </Text>
                    )}
                  </Paper>
                </Box>
              ))}
              <div ref={messagesEndRef} />
            </Stack>
          )}
        </ScrollArea>

        <Box
          p="md"
          style={{ borderTop: "1px solid var(--mantine-color-default-border)" }}
        >
          <Group align="flex-end" gap="sm">
            {mode === "general" && availableTools.length > 0 && (
              <Menu
                shadow="md"
                width={220}
                position="top-start"
                closeOnItemClick={false}
              >
                <Menu.Target>
                  <Tooltip label={t("chat.tools.title")} withArrow>
                    <ActionIcon
                      size={36}
                      variant="light"
                      color={enabledTools.length > 0 ? "blue" : "gray"}
                      disabled={streaming}
                    >
                      <Wrench size={16} />
                    </ActionIcon>
                  </Tooltip>
                </Menu.Target>
                <Menu.Dropdown>
                  <Menu.Label>{t("chat.tools.title")}</Menu.Label>
                  {availableTools.map((tool) => {
                    const isEnabled = enabledTools.includes(tool.id);
                    const displayName = t(`chat.tools.${tool.id}`, tool.name);
                    return (
                      <Menu.Item
                        key={tool.id}
                        onClick={() => toggleTool(tool.id)}
                        closeMenuOnClick={false}
                        leftSection={getToolIcon(tool.id)}
                      >
                        <Checkbox
                          checked={isEnabled}
                          readOnly
                          label={displayName}
                          style={{ pointerEvents: "none" }}
                        />
                      </Menu.Item>
                    );
                  })}
                </Menu.Dropdown>
              </Menu>
            )}
            <Textarea
              ref={inputRef}
              style={{ flex: 1 }}
              placeholder={t("chat.input.placeholder")}
              value={inputValue}
              onChange={(e) => setInputValue(e.currentTarget.value)}
              autosize
              minRows={1}
              maxRows={6}
              onKeyDown={(e) => {
                if (e.key === "Enter" && !e.shiftKey) {
                  e.preventDefault();
                  sendMessage();
                }
              }}
              disabled={streaming}
            />
            {streaming ? (
              <Tooltip label={t("chat.input.stop")} withArrow>
                <ActionIcon
                  size={36}
                  variant="filled"
                  color="red"
                  onClick={stopStreaming}
                >
                  <Square size={16} />
                </ActionIcon>
              </Tooltip>
            ) : (
              <Tooltip label={t("chat.input.send")} withArrow>
                <ActionIcon
                  size={36}
                  variant="filled"
                  onClick={sendMessage}
                  disabled={!inputValue.trim()}
                >
                  <Send size={16} />
                </ActionIcon>
              </Tooltip>
            )}
          </Group>
        </Box>
      </Box>

      <Modal
        opened={!!deleteTargetSessionId}
        onClose={cancelDeleteSession}
        title={t("chat.sessions.deleteConfirmTitle")}
        size="sm"
        centered
      >
        <Text size="sm">{t("chat.sessions.deleteConfirmMessage")}</Text>
        <Group mt="lg" justify="flex-end">
          <Button variant="default" size="sm" onClick={cancelDeleteSession}>
            {t("chat.sessions.cancel")}
          </Button>
          <Button color="red" size="sm" onClick={deleteSession}>
            {t("chat.sessions.delete")}
          </Button>
        </Group>
      </Modal>

      <Modal
        opened={showDeleteAllConfirm}
        onClose={() => setShowDeleteAllConfirm(false)}
        title={t("chat.sessions.deleteAllConfirmTitle")}
        size="sm"
        centered
      >
        <Text size="sm">{t("chat.sessions.deleteAllConfirmMessage")}</Text>
        <Group mt="lg" justify="flex-end">
          <Button
            variant="default"
            size="sm"
            onClick={() => setShowDeleteAllConfirm(false)}
          >
            {t("chat.sessions.cancel")}
          </Button>
          <Button color="red" size="sm" onClick={deleteAllSessions}>
            {t("chat.sessions.delete")}
          </Button>
        </Group>
      </Modal>
    </Box>
  );
}
