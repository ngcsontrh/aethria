import { api } from "../core/client";
import type {
  CreateMentorRequest,
  UpdateMentorRequest,
  GetPageMentorsResponse,
  GetMentorByIdResponse,
} from "./types";

export async function getPageMentors(
  pageNumber = 1,
  pageSize = 10,
  signal?: AbortSignal,
): Promise<GetPageMentorsResponse> {
  const response = await api.get<GetPageMentorsResponse>("/api/mentors", {
    params: { pageNumber, pageSize },
    signal,
  });
  return response.data;
}

export async function getMentorById(
  id: string,
  signal?: AbortSignal,
): Promise<GetMentorByIdResponse> {
  const response = await api.get<GetMentorByIdResponse>(`/api/mentors/${id}`, {
    signal,
  });
  return response.data;
}

export async function createMentor(
  request: CreateMentorRequest,
  signal?: AbortSignal,
): Promise<{ id: string }> {
  const response = await api.post<{ id: string }>("/api/mentors", request, {
    signal,
  });
  return response.data;
}

export async function updateMentor(
  id: string,
  request: UpdateMentorRequest,
  signal?: AbortSignal,
): Promise<void> {
  await api.put(`/api/mentors/${id}`, request, { signal });
}

export async function deleteMentor(
  id: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/mentors/${id}`, { signal });
}
