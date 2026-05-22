import { api } from "../core/client";
import { streamSignalR } from "../core/stream";
import type {
  AgentTool,
  ChatRequest,
  MentorChatRequest,
  ResourceChatRequest,
  ChatStreamResponse,
} from "./types";

export async function getAvailableTools(
  signal?: AbortSignal,
): Promise<AgentTool[]> {
  const response = await api.get<AgentTool[]>("/api/chat/tools", { signal });
  return response.data;
}

export function sendGeneralChatMessageStream(
  request: ChatRequest,
  abortSignal?: AbortSignal,
): AsyncGenerator<ChatStreamResponse> {
  return streamSignalR<ChatStreamResponse>(
    "/hubs/chat",
    "GeneralChatStream",
    [request],
    abortSignal,
  );
}

export function sendMentorChatMessageStream(
  mentorId: string,
  request: MentorChatRequest,
  abortSignal?: AbortSignal,
): AsyncGenerator<ChatStreamResponse> {
  return streamSignalR<ChatStreamResponse>(
    "/hubs/chat",
    "MentorChatStream",
    [mentorId, request],
    abortSignal,
  );
}

export function sendResourceChatMessageStream(
  resourceId: string,
  request: ResourceChatRequest,
  abortSignal?: AbortSignal,
): AsyncGenerator<ChatStreamResponse> {
  return streamSignalR<ChatStreamResponse>(
    "/hubs/chat",
    "ResourceChatStream",
    [resourceId, request],
    abortSignal,
  );
}
