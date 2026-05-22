import { api } from "../core/client";
import { streamSignalR } from "../core/stream";
import type {
  CreateBlankQuizRequest,
  GetPageQuizzesResponse,
  GetQuizByIdResponse,
  GetQuizQuestionsResponse,
  GetQuizQuestionsForEditResponse,
  SubmitQuizRequest,
  SubmitQuizAnswersResponse,
  UpdateQuizRequest,
  GetQuizSubmissionHistoryResponse,
  GetSubmissionByIdResponse,
  CreateAIQuizRequest,
  CreateAIQuizStreamEvent,
} from "./types";

export async function createBlankQuiz(
  request: CreateBlankQuizRequest,
  signal?: AbortSignal,
): Promise<{ id: string }> {
  const response = await api.post<{ id: string }>(
    "/api/quizzes/blank",
    request,
    { signal },
  );
  return response.data;
}

export async function getPageQuizzes(
  pageNumber = 1,
  pageSize = 10,
  signal?: AbortSignal,
): Promise<GetPageQuizzesResponse> {
  const response = await api.get<GetPageQuizzesResponse>("/api/quizzes", {
    params: { pageNumber, pageSize },
    signal,
  });
  return response.data;
}

export async function getQuizById(
  id: string,
  signal?: AbortSignal,
): Promise<GetQuizByIdResponse> {
  const response = await api.get<GetQuizByIdResponse>(`/api/quizzes/${id}`, {
    signal,
  });
  return response.data;
}

export async function getQuizQuestions(
  id: string,
  signal?: AbortSignal,
): Promise<GetQuizQuestionsResponse> {
  const response = await api.get<GetQuizQuestionsResponse>(
    `/api/quizzes/${id}/questions`,
    { signal },
  );
  return response.data;
}

export async function getQuizQuestionsForEdit(
  id: string,
  signal?: AbortSignal,
): Promise<GetQuizQuestionsForEditResponse> {
  const response = await api.get<GetQuizQuestionsForEditResponse>(
    `/api/quizzes/${id}/questions/edit`,
    { signal },
  );
  return response.data;
}

export async function submitQuizAnswers(
  id: string,
  request: SubmitQuizRequest,
  signal?: AbortSignal,
): Promise<SubmitQuizAnswersResponse> {
  const response = await api.post<SubmitQuizAnswersResponse>(
    `/api/quizzes/${id}/submissions`,
    request,
    { signal },
  );
  return response.data;
}

export async function updateQuiz(
  id: string,
  request: UpdateQuizRequest,
  signal?: AbortSignal,
): Promise<unknown> {
  const response = await api.put<unknown>(`/api/quizzes/${id}`, request, {
    signal,
  });
  return response.data;
}

export async function deleteQuiz(
  id: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/quizzes/${id}`, { signal });
}

export async function getQuizSubmissionHistory(
  id: string,
  pageNumber = 1,
  pageSize = 10,
  signal?: AbortSignal,
): Promise<GetQuizSubmissionHistoryResponse> {
  const response = await api.get<GetQuizSubmissionHistoryResponse>(
    `/api/quizzes/${id}/submissions`,
    {
      params: { pageNumber, pageSize },
      signal,
    },
  );
  return response.data;
}

export async function getQuizSubmissionById(
  id: string,
  submissionId: string,
  signal?: AbortSignal,
): Promise<GetSubmissionByIdResponse> {
  const response = await api.get<GetSubmissionByIdResponse>(
    `/api/quizzes/${id}/submissions/${submissionId}`,
    { signal },
  );
  return response.data;
}

export function createAIQuizStream(
  request: CreateAIQuizRequest,
  abortSignal?: AbortSignal,
): AsyncGenerator<CreateAIQuizStreamEvent> {
  return streamSignalR<CreateAIQuizStreamEvent>(
    "/hubs/quizzes",
    "CreateAIQuizStream",
    [request],
    abortSignal,
  );
}
