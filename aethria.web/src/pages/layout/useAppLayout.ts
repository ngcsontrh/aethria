import { useState, useEffect } from "react";
import { useNavigate } from "@tanstack/react-router";
import { useDisclosure } from "@mantine/hooks";
import { useTranslation } from "react-i18next";
import { getUser, logoutUser } from "../../services";

export function useAppLayout() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [mobileOpened, { toggle: toggleMobile, close: closeMobile }] =
    useDisclosure();
  const [desktopOpened, { toggle: toggleDesktop }] = useDisclosure(true);
  const [loggingOut, setLoggingOut] = useState(false);
  const [
    changePasswordOpened,
    { open: openChangePassword, close: closeChangePassword },
  ] = useDisclosure(false);

  const user = getUser();

  useEffect(() => {
    const handleAuthLogout = () => {
      navigate({ to: "/auth/login", replace: true });
    };
    window.addEventListener("auth_logout", handleAuthLogout);
    return () => window.removeEventListener("auth_logout", handleAuthLogout);
  }, [navigate]);

  const handleLogout = async () => {
    setLoggingOut(true);
    try {
      await logoutUser();
    } finally {
      setLoggingOut(false);
      navigate({ to: "/auth/login", replace: true });
    }
  };

  return {
    t,
    user,
    mobileOpened,
    desktopOpened,
    toggleMobile,
    toggleDesktop,
    closeMobile,
    loggingOut,
    handleLogout,
    changePasswordOpened,
    openChangePassword,
    closeChangePassword,
  };
}
