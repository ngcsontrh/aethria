import axios from "axios";

const STORAGE_KEY = "aethria.auth_session";

export interface StoredAuthSession {
  user: {
    userId: string;
    email: string;
  };
  accessToken: string;
  accessTokenExpiresAt: string;
}

export function getStoredSession(): StoredAuthSession | null {
  try {
    const val = localStorage.getItem(STORAGE_KEY);
    if (!val) return null;
    return JSON.parse(val) as StoredAuthSession;
  } catch {
    return null;
  }
}

export function writeStoredSession(session: StoredAuthSession): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  } catch {
    // Ignore storage errors in environments where localStorage might be blocked or full
  }
}

export function clearStoredSession(): void {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // Ignore storage errors
  }
}

export function hasValidAccessToken(): boolean {
  const session = getStoredSession();
  if (!session || !session.accessToken || !session.accessTokenExpiresAt) {
    return false;
  }
  const expiresAt = new Date(session.accessTokenExpiresAt).getTime();
  const skewMs = 30000;
  return expiresAt - skewMs > Date.now();
}

function isPublicAuthEndpoint(url?: string): boolean {
  if (!url) {
    return false;
  }

  return [
    "/api/auth/register",
    "/api/auth/login",
    "/api/auth/google",
    "/api/auth/refresh",
    "/api/auth/logout",
  ].some((endpoint) => url.endsWith(endpoint) || url === endpoint);
}

let refreshPromise: Promise<StoredAuthSession> | null = null;

async function requestAuthSessionRefresh(signal?: AbortSignal): Promise<StoredAuthSession> {
  const baseURL =
    (import.meta.env.VITE_API_URL as string) || "https://localhost:7011";

  const response = await axios.post(
    `${baseURL}/api/auth/refresh`,
    {},
    { withCredentials: true, signal },
  );
  const data = response.data;
  const session: StoredAuthSession = {
    user: {
      userId: data.userId,
      email: data.email,
    },
    accessToken: data.accessToken,
    accessTokenExpiresAt: data.accessTokenExpiresAt,
  };
  writeStoredSession(session);
  return session;
}

export async function refreshAuthSession(signal?: AbortSignal): Promise<StoredAuthSession> {
  refreshPromise ??= requestAuthSessionRefresh(signal).finally(() => {
    refreshPromise = null;
  });

  return refreshPromise;
}

export const api = axios.create({
  baseURL: (import.meta.env.VITE_API_URL as string) || "https://localhost:7011",
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else if (token) {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

api.interceptors.request.use(
  async (config) => {
    if (isPublicAuthEndpoint(config.url)) {
      return config;
    }

    const session = getStoredSession();
    if (session && session.accessToken) {
      if (!hasValidAccessToken()) {
        try {
          const newSession = await refreshAuthSession();
          config.headers.Authorization = `Bearer ${newSession.accessToken}`;
        } catch (err) {
          clearStoredSession();
          window.dispatchEvent(new Event("auth_logout"));
          return Promise.reject(err);
        }
      } else {
        config.headers.Authorization = `Bearer ${session.accessToken}`;
      }
    }

    return config;
  },
  (error) => Promise.reject(error),
);

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (
      error.response &&
      error.response.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      originalRequest.url &&
      !isPublicAuthEndpoint(originalRequest.url)
    ) {
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return api(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const newSession = await refreshAuthSession();
        isRefreshing = false;
        processQueue(null, newSession.accessToken);
        originalRequest.headers.Authorization = `Bearer ${newSession.accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        isRefreshing = false;
        processQueue(refreshError, null);
        clearStoredSession();
        window.dispatchEvent(new Event("auth_logout"));
        return Promise.reject(refreshError);
      }
    }

    // Log the raw error to console for developers
    if (!axios.isCancel(error)) {
      console.error("[API Error]:", {
        url: error.config?.url,
        method: error.config?.method,
        status: error.response?.status,
        data: error.response?.data,
        message: error.message,
      });
    }

    return Promise.reject(error);
  },
);
