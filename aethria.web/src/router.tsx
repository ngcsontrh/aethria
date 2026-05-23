import {
  createRootRoute,
  createRoute,
  createRouter,
  Outlet,
  redirect,
  HeadContent,
} from "@tanstack/react-router";
import i18n from "./i18n/i18n";
import RegisterPage from "./pages/auth/RegisterPage";
import LoginPage from "./pages/auth/LoginPage";
import AppLayout from "./pages/layout/AppLayout";
import ChatPage from "./pages/chat/ChatPage";
import MentorPage from "./pages/mentor/MentorPage";
import ResourcePage from "./pages/resource/ResourcePage";
import QuizPage from "./pages/quiz/QuizPage";
import RoadmapPage from "./pages/roadmap/RoadmapPage";
import ApiKeyPage from "./pages/apiKey/ApiKeyPage";
import NotificationPage from "./pages/notification/NotificationPage";
import { isAuthenticated } from "./services";

const rootRoute = createRootRoute({
  component: () => (
    <>
      <HeadContent />
      <Outlet />
    </>
  ),
});

const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/",
  beforeLoad: () => {
    if (isAuthenticated()) {
      throw redirect({ to: "/chat/general", replace: true });
    }
    throw redirect({ to: "/auth/login", replace: true });
  },
});

const authRegisterRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/auth/register",
  component: () => <RegisterPage />,
  head: () => ({
    meta: [{ title: `${i18n.t("auth.register.title")} | Aethria` }],
  }),
});

const authLoginRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/auth/login",
  component: () => <LoginPage />,
  head: () => ({
    meta: [{ title: `${i18n.t("auth.login.title")} | Aethria` }],
  }),
});

const authenticatedRoute = createRoute({
  getParentRoute: () => rootRoute,
  id: "authenticated",
  component: () => <AppLayout />,
  beforeLoad: () => {
    if (!isAuthenticated()) {
      throw redirect({ to: "/auth/login", replace: true });
    }
  },
});

const chatIndexRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/chat",
  beforeLoad: () => {
    throw redirect({ to: "/chat/general", replace: true });
  },
});

const generalChatRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/chat/general",
  component: () => <ChatPage key="general" mode="general" />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.generalChat")} | Aethria` }],
  }),
});

const mentorChatRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/chat/mentor",
  component: () => <ChatPage key="mentor" mode="mentor" />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.mentorChat")} | Aethria` }],
  }),
});

const resourceChatRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/chat/resource",
  component: () => <ChatPage key="resource" mode="resource" />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.resourceChat")} | Aethria` }],
  }),
});

const mentorRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/mentors",
  component: () => <MentorPage />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.mentors")} | Aethria` }],
  }),
});

const resourceRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/resources",
  component: () => <ResourcePage />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.resources")} | Aethria` }],
  }),
});

const quizRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/quizzes",
  component: () => <QuizPage />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.quizzes")} | Aethria` }],
  }),
});

const roadmapRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/roadmaps",
  component: () => <RoadmapPage />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.roadmaps")} | Aethria` }],
  }),
});

const apiKeyRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/api-keys",
  component: () => <ApiKeyPage />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.apiKeys")} | Aethria` }],
  }),
});

const notificationRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: "/notifications",
  component: () => <NotificationPage />,
  head: () => ({
    meta: [{ title: `${i18n.t("layout.nav.notifications")} | Aethria` }],
  }),
});

const routeTree = rootRoute.addChildren([
  indexRoute,
  authRegisterRoute,
  authLoginRoute,
  authenticatedRoute.addChildren([
    chatIndexRoute,
    generalChatRoute,
    mentorChatRoute,
    resourceChatRoute,
    mentorRoute,
    resourceRoute,
    quizRoute,
    roadmapRoute,
    apiKeyRoute,
    notificationRoute,
  ]),
]);

export const router = createRouter({ routeTree });

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}
