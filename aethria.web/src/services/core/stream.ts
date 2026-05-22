import { HubConnectionBuilder } from "@microsoft/signalr";
import {
  getStoredSession,
  hasValidAccessToken,
  refreshAuthSession,
} from "./client";

export async function* streamSignalR<TEvent>(
  hubPath: string,
  methodName: string,
  args: unknown[],
  abortSignal?: AbortSignal,
): AsyncGenerator<TEvent> {
  const baseURL =
    (import.meta.env.VITE_API_URL as string) || "https://localhost:7011";
  const apiRoot = baseURL.replace(/\/api\/?$/, "");
  const fullUrl = `${apiRoot.replace(/\/$/, "")}/${hubPath.replace(/^\//, "")}`;

  const connection = new HubConnectionBuilder()
    .withUrl(fullUrl, {
      accessTokenFactory: async () => {
        let session = getStoredSession();
        if (session && session.accessToken && !hasValidAccessToken()) {
          try {
            session = await refreshAuthSession();
          } catch {
            throw new Error("Session expired. Authentication required.");
          }
        }
        return session?.accessToken || "";
      },
    })
    .withAutomaticReconnect()
    .build();

  await connection.start();

  const queue: TEvent[] = [];
  let error: unknown = null;
  let done = false;
  let resolveNext: (() => void) | null = null;

  const subscription = connection.stream(methodName, ...args).subscribe({
    next: (item) => {
      queue.push(item as TEvent);
      if (resolveNext) {
        resolveNext();
        resolveNext = null;
      }
    },
    error: (err) => {
      error = err;
      done = true;
      if (resolveNext) {
        resolveNext();
        resolveNext = null;
      }
    },
    complete: () => {
      done = true;
      if (resolveNext) {
        resolveNext();
        resolveNext = null;
      }
    },
  });

  const cleanup = async () => {
    subscription.dispose();
    if (connection.state !== "Disconnected") {
      try {
        await connection.stop();
      } catch (err) {
        console.error("Failed to stop SignalR connection:", err);
      }
    }
  };

  if (abortSignal) {
    abortSignal.addEventListener("abort", () => {
      done = true;
      if (resolveNext) {
        resolveNext();
        resolveNext = null;
      }
      cleanup();
    });
  }

  try {
    while (true) {
      if (abortSignal?.aborted) {
        break;
      }
      if (queue.length > 0) {
        const item = queue.shift()!;
        if (typeof item === "object" && item !== null) {
          const itemObj = item as Record<string, unknown>;
          if (
            "status" in itemObj &&
            String(itemObj.status).toLowerCase() === "failed"
          ) {
            throw new Error(String(itemObj.message || "Stream event failed."));
          }
        }
        yield item;
        continue;
      }
      if (done) {
        if (error) {
          throw error;
        }
        break;
      }
      await new Promise<void>((resolve) => {
        resolveNext = resolve;
      });
    }
  } finally {
    await cleanup();
  }
}
