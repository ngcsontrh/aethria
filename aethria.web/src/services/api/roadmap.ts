import { api } from "../core/client";
import { streamSignalR } from "../core/stream";
import type {
  GenerateAIRoadmapRequest,
  GenerateAIRoadmapStreamEvent,
  GetPageRoadmapsResponse,
  GetRoadmapByIdResponse,
} from "./types";

export function generateAIRoadmapStream(
  request: GenerateAIRoadmapRequest,
  abortSignal?: AbortSignal,
): AsyncGenerator<GenerateAIRoadmapStreamEvent> {
  return streamSignalR<GenerateAIRoadmapStreamEvent>(
    "/hubs/roadmaps",
    "GenerateAIRoadmapStream",
    [request],
    abortSignal,
  );
}

export async function getPageRoadmaps(
  pageNumber = 1,
  pageSize = 10,
  signal?: AbortSignal,
): Promise<GetPageRoadmapsResponse> {
  const response = await api.get<GetPageRoadmapsResponse>("/api/roadmaps", {
    params: { pageNumber, pageSize },
    signal,
  });
  return response.data;
}

export async function getRoadmapById(
  id: string,
  signal?: AbortSignal,
): Promise<GetRoadmapByIdResponse> {
  const response = await api.get<GetRoadmapByIdResponse>(`/api/roadmaps/${id}`, {
    signal,
  });
  return response.data;
}

export async function deleteRoadmap(
  id: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/roadmaps/${id}`, { signal });
}
