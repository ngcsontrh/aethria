import { api } from "../core/client";
import type {
  CreateApiKeyRequest,
  CreateApiKeyResponse,
  GetPageApiKeysRequest,
  GetPageApiKeysResponse,
} from "./types";

export async function createApiKey(
  request: CreateApiKeyRequest,
  signal?: AbortSignal,
): Promise<CreateApiKeyResponse> {
  const response = await api.post<CreateApiKeyResponse>(
    "/api/api-keys",
    request,
    { signal },
  );
  return response.data;
}

export async function getPageApiKeys(
  params?: GetPageApiKeysRequest,
  signal?: AbortSignal,
): Promise<GetPageApiKeysResponse> {
  const response = await api.get<GetPageApiKeysResponse>("/api/api-keys", {
    params,
    signal,
  });
  return response.data;
}

export async function revokeApiKey(
  id: string,
  signal?: AbortSignal,
): Promise<void> {
  await api.delete(`/api/api-keys/${id}`, { signal });
}
