import {
  api,
  getStoredSession,
  writeStoredSession,
  clearStoredSession,
  hasValidAccessToken,
  refreshAuthSession,
} from "../core/client";
import type {
  LoginRequest,
  RegisterRequest,
  GoogleRequest,
  ChangePasswordRequest,
  AuthResponse,
  AuthUser,
} from "./types";

function handleAuthSuccess(response: AuthResponse): AuthUser {
  const session = {
    user: {
      userId: response.userId,
      email: response.email,
    },
    accessToken: response.accessToken,
    accessTokenExpiresAt: response.accessTokenExpiresAt,
  };
  writeStoredSession(session);
  return session.user;
}

export async function loginWithEmail(
  request: LoginRequest,
  signal?: AbortSignal,
): Promise<AuthUser> {
  const response = await api.post<AuthResponse>(
    "/api/auth/login",
    request,
    { signal },
  );
  return handleAuthSuccess(response.data);
}

export async function registerUser(
  request: RegisterRequest,
  signal?: AbortSignal,
): Promise<AuthUser> {
  const response = await api.post<AuthResponse>(
    "/api/auth/register",
    request,
    { signal },
  );
  return handleAuthSuccess(response.data);
}

export async function loginWithGoogle(
  idToken: string,
  signal?: AbortSignal,
): Promise<AuthUser> {
  const request: GoogleRequest = { idToken };
  const response = await api.post<AuthResponse>(
    "/api/auth/google",
    request,
    { signal },
  );
  return handleAuthSuccess(response.data);
}

export async function changePassword(
  request: ChangePasswordRequest,
  signal?: AbortSignal,
): Promise<void> {
  await api.post("/api/auth/change-password", request, { signal });
}

export async function refreshSession(signal?: AbortSignal): Promise<boolean> {
  if (hasValidAccessToken()) {
    return true;
  }

  try {
    await refreshAuthSession(signal);
    return true;
  } catch {
    clearStoredSession();
    return false;
  }
}

export async function logoutUser(signal?: AbortSignal): Promise<void> {
  try {
    await api.post("/api/auth/logout", {}, { signal });
  } finally {
    clearStoredSession();
  }
}

export function getAccessToken(): string | null {
  if (!hasValidAccessToken()) {
    return null;
  }
  const session = getStoredSession();
  return session ? session.accessToken : null;
}

export function isAuthenticated(): boolean {
  const session = getStoredSession();
  return session !== null && hasValidAccessToken();
}

export function getUser(): AuthUser | null {
  const session = getStoredSession();
  return session ? session.user : null;
}
