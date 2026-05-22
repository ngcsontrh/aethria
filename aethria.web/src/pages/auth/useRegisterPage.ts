import { useState, useRef, useEffect } from "react";
import { useNavigate } from "@tanstack/react-router";
import { schemaResolver, useForm } from "@mantine/form";
import { z } from "zod/v4";
import { useTranslation } from "react-i18next";
import { notifications } from "@mantine/notifications";
import { registerUser, loginWithGoogle } from "../../services/api/auth";
import { RegisterRequestSchema } from "../../services";

interface GoogleAccountsId {
  initialize: (config: {
    client_id: string;
    callback: (response: { credential?: string }) => void;
    ux_mode: string;
  }) => void;
  renderButton: (
    element: HTMLDivElement,
    config: {
      type: string;
      theme: string;
      size: string;
      text: string;
      shape: string;
      logo_alignment: string;
      width: string;
    },
  ) => void;
}

interface GoogleClient {
  accounts?: {
    id?: GoogleAccountsId;
  };
}

declare global {
  interface Window {
    google?: GoogleClient;
  }
}

export type PasswordRequirementKey =
  | "auth.passwordRequirements.number"
  | "auth.passwordRequirements.lowercase"
  | "auth.passwordRequirements.uppercase"
  | "auth.passwordRequirements.symbol";

export interface Requirement {
  re: RegExp;
  key: PasswordRequirementKey;
}

export const requirements: Requirement[] = [
  { re: /[0-9]/, key: "auth.passwordRequirements.number" },
  { re: /[a-z]/, key: "auth.passwordRequirements.lowercase" },
  { re: /[A-Z]/, key: "auth.passwordRequirements.uppercase" },
  { re: /[$&+,:;=?@#|'<>.^*()%!-]/, key: "auth.passwordRequirements.symbol" },
];

export function getStrength(password: string) {
  let multiplier = password.length >= 8 ? 1 : 0;

  requirements.forEach((requirement) => {
    if (requirement.re.test(password)) {
      multiplier += 1;
    }
  });

  return Math.min(
    100,
    Math.round((multiplier / (requirements.length + 1)) * 100),
  );
}

export function useRegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const googleButtonRef = useRef<HTMLDivElement>(null);

  const [loading, setLoading] = useState(false);

  const form = useForm({
    mode: "controlled",
    initialValues: {
      email: "",
      password: "",
      confirmPassword: "",
    },
    validate: schemaResolver(
      RegisterRequestSchema.extend({
        confirmPassword: z.string(),
      }).refine((data) => data.password === data.confirmPassword, {
        path: ["confirmPassword"],
      }),
      { sync: true },
    ),
  });

  const strength = getStrength(form.values.password);
  const hasEightChars = form.values.password.length >= 8;

  useEffect(() => {
    const scriptId = "google-identity-services-client";
    let script = document.getElementById(scriptId) as HTMLScriptElement;

    const initializeGoogle = () => {
      const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
      if (!clientId) {
        console.error(
          "Google Client ID is not configured in environment variables.",
        );
        return;
      }

      if (window.google?.accounts?.id) {
        window.google.accounts.id.initialize({
          client_id: clientId,
          callback: async (response) => {
            if (response.credential) {
              setLoading(true);
              try {
                const user = await loginWithGoogle(response.credential);
                notifications.show({
                  title: t("auth.common.success"),
                  message: t("auth.login.googleSuccess", { email: user.email }),
                  color: "green",
                });
                setTimeout(() => {
                  navigate({ to: "/" });
                }, 1500);
              } catch (err) {
                console.error(err);
                notifications.show({
                  title: t("auth.common.error"),
                  message: t("auth.common.googleFail"),
                  color: "red",
                });
              } finally {
                setLoading(false);
              }
            }
          },
          ux_mode: "popup",
        });

        if (googleButtonRef.current) {
          window.google.accounts.id.renderButton(googleButtonRef.current, {
            type: "standard",
            theme: "outline",
            size: "large",
            text: "signup_with",
            shape: "rectangular",
            logo_alignment: "left",
            width: "360",
          });
        }
      }
    };

    if (!script) {
      script = document.createElement("script");
      script.id = scriptId;
      script.src = "https://accounts.google.com/gsi/client";
      script.async = true;
      script.defer = true;
      script.addEventListener("load", initializeGoogle);
      document.head.appendChild(script);
    } else {
      if (window.google?.accounts?.id) {
        initializeGoogle();
      } else {
        script.addEventListener("load", initializeGoogle);
      }
    }

    return () => {
      if (script) {
        script.removeEventListener("load", initializeGoogle);
      }
    };
  }, [navigate, t]);

  const handleSubmit = async (values: typeof form.values) => {
    setLoading(true);

    try {
      const user = await registerUser({
        email: values.email,
        password: values.password,
      });
      notifications.show({
        title: t("auth.common.success"),
        message: t("auth.register.registerSuccess", { email: user.email }),
        color: "green",
      });
      setTimeout(() => {
        navigate({ to: "/" });
      }, 1500);
    } catch (err) {
      console.error(err);
      notifications.show({
        title: t("auth.common.error"),
        message: t("auth.common.genericError"),
        color: "red",
      });
    } finally {
      setLoading(false);
    }
  };

  return {
    form,
    loading,
    googleButtonRef,
    handleSubmit,
    strength,
    hasEightChars,
    requirements,
    t,
  };
}
