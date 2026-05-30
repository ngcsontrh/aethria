import { HubConnectionBuilder } from "@microsoft/signalr";
import type { HubConnection } from "@microsoft/signalr";
import {
  getStoredSession,
  hasValidAccessToken,
  refreshAuthSession,
} from "./client";

export async function createSignalRConnection(
  hubPath: string,
): Promise<HubConnection> {
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

  return connection;
}
