import { useState, useEffect, useRef, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  sendGeneralChatMessageStream,
  sendMentorChatMessageStream,
  sendResourceChatMessageStream,
  getGeneralChatSessions,
  getGeneralChatSessionMessages,
  deleteGeneralChatSession,
  deleteAllGeneralChatSessions,
  getMentorChatSessions,
  getMentorChatSessionMessages,
  deleteMentorChatSession,
  deleteAllMentorChatSessions,
  getResourceChatSessions,
  getResourceChatSessionMessages,
  deleteResourceChatSession,
  deleteAllResourceChatSessions,
  getPageMentors,
  getPageResources,
  ChatRequestSchema,
  MentorChatRequestSchema,
  ResourceChatRequestSchema,
  getAvailableTools,
} from "../../services";
import type {
  ChatMessage,
  ChatSession,
  MentorListItemResponse,
  ResourcePageItemResponse,
  ChatStreamResponse,
  ChatSessionsPageResponse,
  ChatHistoryResponse,
  AgentTool,
} from "../../services";

export type ChatMode = "general" | "mentor" | "resource";

interface UseChatPageParams {
  mode: ChatMode;
}

const chatKeys = {
  all: ["chat"] as const,
  targets: (mode: Exclude<ChatMode, "general">) =>
    [...chatKeys.all, "targets", mode] as const,
  sessions: (mode: ChatMode, targetId: string | null) =>
    [...chatKeys.all, "sessions", mode, targetId ?? "none"] as const,
  messages: (mode: ChatMode, targetId: string | null, sessionId: string) =>
    [...chatKeys.all, "messages", mode, targetId ?? "none", sessionId] as const,
};

function canFetchSessions(mode: ChatMode, targetId: string | null) {
  return mode === "general" || !!targetId;
}

function mapSessions(
  response: ChatSessionsPageResponse,
  unnamedTitle: string,
): ChatSession[] {
  return response.items.map((session) => ({
    id: session.id,
    title: session.name || unnamedTitle,
    description: session.description ?? "",
    createdAt: session.createdAt,
    updatedAt: session.updatedAt,
  }));
}

function mapMessages(response: ChatHistoryResponse): ChatMessage[] {
  return response.messages.map((message) => ({
    id: message.id,
    role: message.role as "user" | "assistant",
    content: message.content,
    createdAt: message.createdAt,
  }));
}

async function fetchSessionsByMode(
  mode: ChatMode,
  targetId: string | null,
  signal?: AbortSignal,
) {
  if (mode === "general") {
    return getGeneralChatSessions(1, 20, signal);
  }

  if (mode === "mentor" && targetId) {
    return getMentorChatSessions(targetId, 1, 20, signal);
  }

  if (mode === "resource" && targetId) {
    return getResourceChatSessions(targetId, 1, 20, signal);
  }

  return { items: [], totalCount: 0, pageNumber: 1, pageSize: 20, totalPages: 0 };
}

async function fetchMessagesByMode(
  mode: ChatMode,
  targetId: string | null,
  sessionId: string,
  signal?: AbortSignal,
) {
  if (mode === "general") {
    return getGeneralChatSessionMessages(sessionId, signal);
  }

  if (mode === "mentor" && targetId) {
    return getMentorChatSessionMessages(targetId, sessionId, signal);
  }

  if (mode === "resource" && targetId) {
    return getResourceChatSessionMessages(targetId, sessionId, signal);
  }

  return { messages: [] };
}

export function useChatPage({ mode }: UseChatPageParams) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const [targetId, setTargetId] = useState<string | null>(null);
  const [targetName, setTargetName] = useState<string>("");

  const [activeSessionId, setActiveSessionId] = useState<string | null>(null);
  const [draftMessages, setDraftMessages] = useState<ChatMessage[]>([]);

  const [inputValue, setInputValue] = useState("");
  const [streaming, setStreaming] = useState(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  const [deleteTargetSessionId, setDeleteTargetSessionId] = useState<
    string | null
  >(null);
  const [showDeleteAllConfirm, setShowDeleteAllConfirm] = useState(false);

  const messagesEndRef = useRef<HTMLDivElement>(null);

  const [enabledTools, setEnabledTools] = useState<string[]>([]);

  const toolsQuery = useQuery({
    queryKey: ["chat", "availableTools"],
    queryFn: ({ signal }) => getAvailableTools(signal),
    enabled: mode === "general",
  });

  const toggleTool = useCallback((toolId: string) => {
    setEnabledTools((prev) =>
      prev.includes(toolId)
        ? prev.filter((id) => id !== toolId)
        : [...prev, toolId],
    );
  }, []);

  const targetListQuery = useQuery({
    queryKey:
      mode === "mentor" || mode === "resource"
        ? chatKeys.targets(mode)
        : chatKeys.targets("mentor"),
    queryFn: async ({ signal }) => {
      if (mode === "mentor") {
        const response = await getPageMentors(1, 50, signal);
        return { mentors: response.items, resources: [] };
      }

      if (mode === "resource") {
        const response = await getPageResources(1, 50, signal);
        return { mentors: [], resources: response.items };
      }

      return { mentors: [], resources: [] };
    },
    enabled: mode !== "general",
  });

  const sessionsQuery = useQuery({
    queryKey: chatKeys.sessions(mode, targetId),
    queryFn: ({ signal }) => fetchSessionsByMode(mode, targetId, signal),
    enabled: canFetchSessions(mode, targetId),
    select: (response) => mapSessions(response, t("chat.sessions.unnamed")),
  });

  const messagesQuery = useQuery({
    queryKey: activeSessionId
      ? chatKeys.messages(mode, targetId, activeSessionId)
      : chatKeys.messages(mode, targetId, "none"),
    queryFn: async ({ signal }) =>
      mapMessages(await fetchMessagesByMode(mode, targetId, activeSessionId!, signal)),
    enabled: !!activeSessionId && canFetchSessions(mode, targetId) && !streaming,
    staleTime: Infinity,
  });

  const sessionsQueryKey = chatKeys.sessions(mode, targetId);

  const invalidateSessions = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: sessionsQueryKey });
  }, [queryClient, sessionsQueryKey]);

  const setMessagesForSession = useCallback(
    (
      sessionId: string | null,
      updater: (messages: ChatMessage[]) => ChatMessage[],
    ) => {
      if (sessionId) {
        queryClient.setQueryData<ChatMessage[]>(
          chatKeys.messages(mode, targetId, sessionId),
          (previous) => updater(previous ?? []),
        );
      } else {
        setDraftMessages((previous) => updater(previous));
      }
    },
    [mode, queryClient, targetId],
  );

  const deleteSessionMutation = useMutation({
    mutationFn: async (sessionId: string) => {
      if (mode === "general") {
        await deleteGeneralChatSession(sessionId);
      } else if (mode === "mentor" && targetId) {
        await deleteMentorChatSession(targetId, sessionId);
      } else if (mode === "resource" && targetId) {
        await deleteResourceChatSession(targetId, sessionId);
      }
    },
    onSuccess: async (_, sessionId) => {
      if (activeSessionId === sessionId) {
        setActiveSessionId(null);
        setDraftMessages([]);
        setInputValue("");
        setStreaming(false);
      }
      await invalidateSessions();
    },
    onSettled: () => {
      setDeleteTargetSessionId(null);
    },
  });

  const deleteAllSessionsMutation = useMutation({
    mutationFn: async () => {
      if (mode === "general") {
        await deleteAllGeneralChatSessions();
      } else if (mode === "mentor" && targetId) {
        await deleteAllMentorChatSessions(targetId);
      } else if (mode === "resource" && targetId) {
        await deleteAllResourceChatSessions(targetId);
      }
    },
    onSuccess: async () => {
      setActiveSessionId(null);
      setDraftMessages([]);
      setInputValue("");
      setStreaming(false);
      await invalidateSessions();
    },
    onSettled: () => {
      setShowDeleteAllConfirm(false);
    },
  });

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [messagesQuery.data, draftMessages, scrollToBottom]);

  useEffect(() => {
    return () => {
      abortControllerRef.current?.abort();
    };
  }, []);

  const selectSession = useCallback((sessionId: string) => {
    setDraftMessages([]);
    setActiveSessionId(sessionId);
  }, []);

  const startNewChat = useCallback(() => {
    abortControllerRef.current?.abort();
    setActiveSessionId(null);
    setDraftMessages([]);
    setInputValue("");
    setStreaming(false);
  }, []);

  const selectTarget = useCallback((id: string, name: string) => {
    setTargetId(id);
    setTargetName(name);
    setActiveSessionId(null);
    setDraftMessages([]);
    setInputValue("");
  }, []);

  const clearTarget = useCallback(() => {
    setTargetId(null);
    setTargetName("");
    setActiveSessionId(null);
    setDraftMessages([]);
    setInputValue("");
  }, []);

  const sendMessage = useCallback(async () => {
    const text = inputValue.trim();
    if (!text || streaming) return;
    if ((mode === "mentor" || mode === "resource") && !targetId) return;

    const isNewSession = !activeSessionId;

    const tempUserId = `temp-user-${Date.now()}`;
    const userMsg: ChatMessage = {
      id: tempUserId,
      role: "user",
      content: text,
      createdAt: new Date().toISOString(),
    };

    const tempAssistantId = `temp-assistant-${Date.now()}`;
    const assistantMsg: ChatMessage = {
      id: tempAssistantId,
      role: "assistant",
      content: "",
      createdAt: new Date().toISOString(),
    };

    let currentSessionId = activeSessionId;
    const pendingMessages = [
      ...(currentSessionId ? (messagesQuery.data ?? []) : draftMessages),
      userMsg,
      assistantMsg,
    ];

    setMessagesForSession(currentSessionId, (previous) => [
      ...previous,
      userMsg,
      assistantMsg,
    ]);
    setInputValue("");

    const abortController = new AbortController();
    abortControllerRef.current = abortController;
    setStreaming(true);
    let assistantContent = "";

    try {
      let stream: AsyncGenerator<ChatStreamResponse>;
      const request = {
        message: text,
        sessionId: currentSessionId ?? undefined,
        tools: mode === "general" ? enabledTools : undefined,
      };

      if (mode === "general") {
        stream = sendGeneralChatMessageStream(
          ChatRequestSchema.parse(request),
          abortController.signal,
        );
      } else if (mode === "mentor" && targetId) {
        stream = sendMentorChatMessageStream(
          targetId,
          MentorChatRequestSchema.parse(request),
          abortController.signal,
        );
      } else if (mode === "resource" && targetId) {
        stream = sendResourceChatMessageStream(
          targetId,
          ResourceChatRequestSchema.parse(request),
          abortController.signal,
        );
      } else {
        return;
      }

      for await (const event of stream) {
        if (abortController.signal.aborted) break;

        if (
          event.status === "started" &&
          event.sessionId &&
          !currentSessionId
        ) {
          currentSessionId = event.sessionId;
          queryClient.setQueryData<ChatMessage[]>(
            chatKeys.messages(mode, targetId, event.sessionId),
            pendingMessages,
          );
          setDraftMessages([]);
          setActiveSessionId(event.sessionId);
        }

        if (event.status === "delta" && event.delta) {
          assistantContent += event.delta;
          setMessagesForSession(currentSessionId, (previous) =>
            previous.map((message) =>
              message.id === tempAssistantId
                ? { ...message, content: message.content + event.delta }
                : message,
            ),
          );
          scrollToBottom();
        }

        if (event.status === "completed" && event.answer) {
          const finalAnswer = event.answer ?? assistantContent;
          setMessagesForSession(currentSessionId, (previous) => {
            let replaced = false;
            const updated = previous.map((message) => {
              if (message.id !== tempAssistantId) return message;
              replaced = true;
              return { ...message, content: finalAnswer };
            });

            if (replaced) return updated;

            return [
              ...updated.filter(
                (message) =>
                  !(message.role === "assistant" && message.content === ""),
              ),
              {
                id: `completed-assistant-${Date.now()}`,
                role: "assistant",
                content: finalAnswer,
                createdAt: new Date().toISOString(),
              },
            ];
          });
        }
      }

      if (isNewSession) {
        await invalidateSessions();
      }
    } catch (err: unknown) {
      if (!abortController.signal.aborted) {
        const errorMsg =
          err instanceof Error ? err.message : "Something went wrong.";
        setMessagesForSession(currentSessionId, (previous) =>
          previous.map((message) =>
            message.id === tempAssistantId
              ? { ...message, content: errorMsg, isError: true }
              : message,
          ),
        );
      }
    } finally {
      setMessagesForSession(currentSessionId, (previous) =>
        previous.filter(
          (message) =>
            message.id !== tempAssistantId ||
            message.role !== "assistant" ||
            message.content !== "",
        ),
      );
      setStreaming(false);
      abortControllerRef.current = null;
      scrollToBottom();
    }
  }, [
    activeSessionId,
    draftMessages,
    enabledTools,
    inputValue,
    invalidateSessions,
    messagesQuery.data,
    mode,
    queryClient,
    scrollToBottom,
    setMessagesForSession,
    streaming,
    targetId,
  ]);

  const stopStreaming = useCallback(() => {
    abortControllerRef.current?.abort();
  }, []);

  const confirmDeleteSession = useCallback((sessionId: string) => {
    setDeleteTargetSessionId(sessionId);
  }, []);

  const deleteSession = useCallback(async () => {
    if (!deleteTargetSessionId) return;
    await deleteSessionMutation.mutateAsync(deleteTargetSessionId);
  }, [deleteSessionMutation, deleteTargetSessionId]);

  const cancelDeleteSession = useCallback(() => {
    setDeleteTargetSessionId(null);
  }, []);

  const deleteAllSessions = useCallback(async () => {
    await deleteAllSessionsMutation.mutateAsync();
  }, [deleteAllSessionsMutation]);

  return {
    t,
    mode,
    targetId,
    targetName,
    selectTarget,
    clearTarget,
    mentors: targetListQuery.data?.mentors ?? ([] as MentorListItemResponse[]),
    resources:
      targetListQuery.data?.resources ?? ([] as ResourcePageItemResponse[]),
    listLoading: targetListQuery.isLoading,
    sessions: sessionsQuery.data ?? [],
    sessionsLoading: sessionsQuery.isLoading,
    activeSessionId,
    selectSession,
    startNewChat,
    messages: activeSessionId ? (messagesQuery.data ?? []) : draftMessages,
    messagesLoading: !!activeSessionId && messagesQuery.isLoading,
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
    availableTools: toolsQuery.data ?? ([] as AgentTool[]),
    enabledTools,
    toggleTool,
  };
}
