import {
  TextInput,
  PasswordInput,
  Anchor,
  Paper,
  Title,
  Text,
  Container,
  Button,
  Divider,
  Progress,
  Box,
  Stack,
} from "@mantine/core";
import { Link } from "@tanstack/react-router";
import { Check, X, Mail, Lock, UserPlus } from "lucide-react";
import { HeaderActions } from "./HeaderActions";
import { useRegisterPage } from "./useRegisterPage";
import logoIcon from "../../assets/logo-icon.png";

interface PasswordRequirementProps {
  meets: boolean;
  label: string;
}

function PasswordRequirement({ meets, label }: PasswordRequirementProps) {
  return (
    <Text
      component="div"
      c={meets ? "teal" : "red"}
      style={{ display: "flex", alignItems: "center", gap: 5 }}
      size="xs"
      mt={5}
    >
      {meets ? <Check size={14} /> : <X size={14} />}
      <Box ml={5}>{label}</Box>
    </Text>
  );
}

export default function RegisterPage() {
  const {
    form,
    loading,
    googleButtonRef,
    handleSubmit,
    strength,
    hasEightChars,
    requirements,
    t,
  } = useRegisterPage();

  const strengthColor =
    strength === 100 ? "teal" : strength > 50 ? "yellow" : "red";

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
              {t("auth.register.title")}
            </Title>
            <Text c="dimmed" size="sm" ta="center" mt="xs" mb="lg">
              {t("auth.register.subtitle")}
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

                {form.values.password.length > 0 && (
                  <Box>
                    <Progress
                      value={strength}
                      size="xs"
                      color={strengthColor}
                      my="xs"
                    />
                    <PasswordRequirement
                      meets={hasEightChars}
                      label={t("auth.passwordRequirements.length")}
                    />
                    {requirements.map((requirement, index) => (
                      <PasswordRequirement
                        key={index}
                        meets={requirement.re.test(form.values.password)}
                        label={t(requirement.key)}
                      />
                    ))}
                  </Box>
                )}

                <PasswordInput
                  required
                  label={t("auth.common.confirmPasswordLabel")}
                  placeholder={t("auth.common.confirmPasswordPlaceholder")}
                  leftSection={<Lock size={16} opacity={0.6} />}
                  size="md"
                  {...form.getInputProps("confirmPassword")}
                />

                <Button
                  type="submit"
                  fullWidth
                  mt="xl"
                  loading={loading}
                  size="md"
                  leftSection={<UserPlus size={16} />}
                >
                  {t("auth.register.submit")}
                </Button>
              </Stack>
            </form>

            <Text size="xs" c="dimmed" ta="center" mt="xl">
              {t("auth.register.haveAccount")}{" "}
              <Link to="/auth/login" style={{ textDecoration: "none" }}>
                <Anchor size="xs" component="span" fw={600}>
                  {t("auth.register.loginHere")}
                </Anchor>
              </Link>
            </Text>
          </Paper>
        </Container>
      </Box>
    </Box>
  );
}
