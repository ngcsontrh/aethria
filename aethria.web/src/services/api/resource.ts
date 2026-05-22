import { api } from "../core/client";
import type {
  GetPageResourcesResponse,
  GetResourceByIdResponse,
  GetResourceSelectorResponse,
  UpdateResourceRequest,
} from "./types";

export async function createResource(
  name: string,
  description: string,
  file: File,
  signal?: AbortSignal,
): Promise<{ id: string }> {
  const formData = new FormData();
  formData.append("name", name);
  formData.append("description", description);
  formData.append("file", file);

  const response = await api.post<{ id: string }>("/api/resources", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
    signal,
  });
  return response.data;
}

export async function updateResource(
  id: string,
  request: UpdateResourceRequest,
  signal?: AbortSignal,
): Promise<void> {
  await api.put(`/api/resources/${id}`, request, { signal });
}

export async function getResourceById(
  id: string,
  signal?: AbortSignal,
): Promise<GetResourceByIdResponse> {
  const response = await api.get<GetResourceByIdResponse>(
    `/api/resources/${id}`,
    { signal },
  );
  return response.data;
}

export async function downloadResource(
  id: string,
  signal?: AbortSignal,
): Promise<Blob> {
  const response = await api.get<Blob>(`/api/resources/${id}/download`, {
    responseType: "blob",
    signal,
  });
  return response.data;
}

export async function getPageResources(
  pageNumber = 1,
  pageSize = 10,
  signal?: AbortSignal,
): Promise<GetPageResourcesResponse> {
  const response = await api.get<GetPageResourcesResponse>("/api/resources", {
    params: { pageNumber, pageSize },
    signal,
  });
  return response.data;
}

export async function getResourceSelector(
  signal?: AbortSignal,
): Promise<GetResourceSelectorResponse> {
  const response = await api.get<GetResourceSelectorResponse>(
    "/api/resources/selector",
    { signal },
  );
  return response.data;
}

export async function deleteResource(
  id: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/resources/${id}`, { signal });
}
