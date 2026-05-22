import {
  ActionIcon,
  Tooltip,
  useComputedColorScheme,
  useMantineColorScheme,
  Group,
  Box,
  Menu,
  Button,
} from "@mantine/core";
import { Sun, Moon, Languages } from "lucide-react";
import { useTranslation } from "react-i18next";

function ColorSchemeToggle() {
  const { setColorScheme } = useMantineColorScheme();
  const computedColorScheme = useComputedColorScheme("light", {
    getInitialValueInEffect: true,
  });
  const { t } = useTranslation();

  const isDark = computedColorScheme === "dark";

  return (
    <Tooltip
      label={
        isDark
          ? t("auth.common.appearance.light")
          : t("auth.common.appearance.dark")
      }
      position="bottom"
      withArrow
    >
      <ActionIcon
        onClick={() => setColorScheme(isDark ? "light" : "dark")}
        variant="subtle"
        color="gray"
        size={30}
        aria-label="Toggle color scheme"
        style={{
          borderRadius: "var(--mantine-radius-xl)",
        }}
      >
        {isDark ? (
          <Sun size={16} strokeWidth={1.5} />
        ) : (
          <Moon size={16} strokeWidth={1.5} />
        )}
      </ActionIcon>
    </Tooltip>
  );
}

function LanguageSwitcher() {
  const { i18n } = useTranslation();

  const changeLanguage = (lng: string) => {
    i18n.changeLanguage(lng);
  };

  const currentLanguage = i18n.resolvedLanguage || i18n.language || "en";

  return (
    <Menu shadow="md" width={130} position="bottom-end">
      <Menu.Target>
        <Button
          variant="subtle"
          color="gray"
          leftSection={<Languages size={15} />}
          h={30}
          px={10}
          style={{
            borderRadius: "var(--mantine-radius-xl)",
            fontSize: "var(--mantine-font-size-xs)",
            fontWeight: 600,
          }}
        >
          {currentLanguage === "en" ? "EN" : "VI"}
        </Button>
      </Menu.Target>

      <Menu.Dropdown>
        <Menu.Item
          onClick={() => changeLanguage("en")}
          style={{
            fontWeight: currentLanguage === "en" ? 600 : 400,
          }}
        >
          🇺🇸 English
        </Menu.Item>
        <Menu.Item
          onClick={() => changeLanguage("vi")}
          style={{
            fontWeight: currentLanguage === "vi" ? 600 : 400,
          }}
        >
          🇻🇳 Tiếng Việt
        </Menu.Item>
      </Menu.Dropdown>
    </Menu>
  );
}

export function HeaderActions() {
  return (
    <Group
      gap={4}
      style={{
        position: "absolute",
        top: 20,
        right: 20,
        zIndex: 100,
        border: "1px solid var(--mantine-color-default-border)",
        borderRadius: "var(--mantine-radius-xl)",
        padding: "3px 6px",
        backgroundColor: "var(--mantine-color-default)",
        boxShadow: "var(--mantine-shadow-xs)",
        alignItems: "center",
      }}
    >
      <LanguageSwitcher />
      <Box
        style={{
          width: "1px",
          height: "16px",
          backgroundColor: "var(--mantine-color-default-border)",
        }}
      />
      <ColorSchemeToggle />
    </Group>
  );
}
