import {
  TextInput,
  PasswordInput,
  Checkbox,
  Anchor,
  Paper,
  Title,
  Text,
  Container,
  Group,
  Button,
  Divider,
  Box,
  Stack,
} from "@mantine/core";
import { Link } from "@tanstack/react-router";
import { Mail, Lock, LogIn } from "lucide-react";
import { HeaderActions } from "./HeaderActions";
import { useLoginPage } from "./useLoginPage";
import logoIcon from "../../assets/logo-icon.png";

export default function LoginPage() {
  const { form, loading, googleButtonRef, handleSubmit, t } = useLoginPage();

  return (
    <Box
      style={{
        display: "flex",
        minHeight: "100vh",
        alignItems: "center",
        justifyContent: "center",
        padding: "20px",
      }}
    >
      <HeaderActions />
      <Box
        style={{
          width: "100%",
          maxWidth: "440px",
          zIndex: 5,
        }}
      >
        <Container size="xs" style={{ width: "100%" }} p={0}>
          <Paper radius="md" p="xl" withBorder shadow="md">
            <Box
              style={{
                display: "flex",
                justifyContent: "center",
                marginBottom: "20px",
              }}
            >
              <img
                src={logoIcon}
                alt="Aethria Logo"
                style={{ height: 64, width: 64, objectFit: "contain" }}
              />
            </Box>
            <Title order={2} ta="center" size="h2" fw={700}>
              {t("auth.login.title")}
            </Title>
            <Text c="dimmed" size="sm" ta="center" mt="xs" mb="lg">
              {t("auth.login.subtitle")}
            </Text>

            <Box
              style={{
                display: "flex",
                justifyContent: "center",
                marginBottom: "15px",
              }}
            >
              <div ref={googleButtonRef} />
            </Box>

            <Divider
              label={t("auth.common.orSocial")}
              labelPosition="center"
              my="lg"
            />

            <form onSubmit={form.onSubmit(handleSubmit)}>
              <Stack gap="md">
                <TextInput
                  required
                  label={t("auth.common.emailLabel")}
                  placeholder={t("auth.common.emailPlaceholder")}
                  leftSection={<Mail size={16} opacity={0.6} />}
                  size="md"
                  {...form.getInputProps("email")}
                />

                <PasswordInput
                  required
                  label={t("auth.common.passwordLabel")}
                  placeholder={t("auth.common.passwordPlaceholder")}
                  leftSection={<Lock size={16} opacity={0.6} />}
                  size="md"
                  {...form.getInputProps("password")}
                />

                <Group justify="space-between" mt="xs">
                  <Checkbox
                    label={t("auth.common.rememberMe")}
                    {...form.getInputProps("rememberMe", { type: "checkbox" })}
                  />
                  <Anchor size="xs" href="#">
                    {t("auth.login.forgotPassword")}
                  </Anchor>
                </Group>

                <Button
                  type="submit"
                  fullWidth
                  mt="xl"
                  loading={loading}
                  size="md"
                  leftSection={<LogIn size={16} />}
                >
                  {t("auth.login.submit")}
                </Button>
              </Stack>
            </form>

            <Text size="xs" c="dimmed" ta="center" mt="xl">
              {t("auth.login.noAccount")}{" "}
              <Link to="/auth/register" style={{ textDecoration: "none" }}>
                <Anchor size="xs" component="span" fw={600}>
                  {t("auth.login.registerHere")}
                </Anchor>
              </Link>
            </Text>
          </Paper>
        </Container>
      </Box>
    </Box>
  );
}
