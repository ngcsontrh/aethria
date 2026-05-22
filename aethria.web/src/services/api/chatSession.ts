import { api } from "../core/client";
import type { ChatSessionsPageResponse, ChatHistoryResponse } from "./types";

export async function getGeneralChatSessions(
  pageNumber = 1,
  pageSize = 20,
  signal?: AbortSignal,
): Promise<ChatSessionsPageResponse> {
  const response = await api.get<ChatSessionsPageResponse>(
    "/api/chat-sessions/general",
    {
      params: { pageNumber, pageSize },
      signal,
    },
  );
  return response.data;
}

export async function getGeneralChatSessionMessages(
  sessionId: string,
  signal?: AbortSignal,
): Promise<ChatHistoryResponse> {
  const response = await api.get<ChatHistoryResponse>(
    `/api/chat-sessions/general/${sessionId}/messages`,
    { signal },
  );
  return response.data;
}

export async function deleteGeneralChatSession(
  sessionId: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/chat-sessions/general/${sessionId}`, { signal });
}

export async function deleteAllGeneralChatSessions(
  signal?: AbortSignal,
): Promise<void> {
  await api.delete("/api/chat-sessions/general", { signal });
}

export async function getMentorChatSessions(
  mentorId: string,
  pageNumber = 1,
  pageSize = 20,
  signal?: AbortSignal,
): Promise<ChatSessionsPageResponse> {
  const response = await api.get<ChatSessionsPageResponse>(
    `/api/chat-sessions/mentors/${mentorId}`,
    {
      params: { pageNumber, pageSize },
      signal,
    },
  );
  return response.data;
}

export async function getMentorChatSessionMessages(
  mentorId: string,
  sessionId: string,
  signal?: AbortSignal,
): Promise<ChatHistoryResponse> {
  const response = await api.get<ChatHistoryResponse>(
    `/api/chat-sessions/mentors/${mentorId}/${sessionId}/messages`,
    { signal },
  );
  return response.data;
}

export async function deleteMentorChatSession(
  mentorId: string,
  sessionId: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/chat-sessions/mentors/${mentorId}/${sessionId}`, {
    signal,
  });
}

export async function deleteAllMentorChatSessions(
  mentorId: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/chat-sessions/mentors/${mentorId}`, { signal });
}

export async function getResourceChatSessions(
  resourceId: string,
  pageNumber = 1,
  pageSize = 20,
  signal?: AbortSignal,
): Promise<ChatSessionsPageResponse> {
  const response = await api.get<ChatSessionsPageResponse>(
    `/api/chat-sessions/resources/${resourceId}`,
    {
      params: { pageNumber, pageSize },
      signal,
    },
  );
  return response.data;
}

export async function getResourceChatSessionMessages(
  resourceId: string,
  sessionId: string,
  signal?: AbortSignal,
): Promise<ChatHistoryResponse> {
  const response = await api.get<ChatHistoryResponse>(
    `/api/chat-sessions/resources/${resourceId}/${sessionId}/messages`,
    { signal },
  );
  return response.data;
}

export async function deleteResourceChatSession(
  resourceId: string,
  sessionId: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/chat-sessions/resources/${resourceId}/${sessionId}`, {
    signal,
  });
}

export async function deleteAllResourceChatSessions(
  resourceId: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/chat-sessions/resources/${resourceId}`, { signal });
}

export async function deleteAllUserChatSessions(
  signal?: AbortSignal,
): Promise<void> {
  await api.delete("/api/chat-sessions", { signal });
}
