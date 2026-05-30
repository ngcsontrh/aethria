import { api } from "../core/client";
import type {
  GenerateAIRoadmapRequest,
  GetPageRoadmapsResponse,
  GetRoadmapByIdResponse,
} from "./types";

export async function generateAIRoadmap(
  request: GenerateAIRoadmapRequest,
  signal?: AbortSignal,
): Promise<{ id: string }> {
  const response = await api.post<{ id: string }>(
    "/api/roadmaps/ai",
    request,
    { signal },
  );
  return response.data;
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
