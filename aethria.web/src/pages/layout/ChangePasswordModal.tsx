import { useState } from "react";
import {
  Modal,
  Stack,
  PasswordInput,
  Button,
  Group,
  Text,
  Progress,
  List,
  ThemeIcon,
} from "@mantine/core";
import { schemaResolver, useForm } from "@mantine/form";
import { z } from "zod/v4";
import { useTranslation } from "react-i18next";
import { notifications } from "@mantine/notifications";
import { CheckCircle, XCircle } from "lucide-react";
import { changePassword } from "../../services";
import { ChangePasswordRequestSchema } from "../../services/api/types";

const changePasswordFormSchema = ChangePasswordRequestSchema.extend({
  confirmPassword: z.string(),
}).refine((data) => data.newPassword === data.confirmPassword, {
  path: ["confirmPassword"],
});

type FormValues = {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
};

interface PasswordRequirement {
  re: RegExp;
  label: string;
}

function getPasswordStrength(
  password: string,
  requirements: PasswordRequirement[],
): number {
  const met = requirements.filter((r) => r.re.test(password)).length;
  return Math.round((met / requirements.length) * 100);
}

function PasswordStrengthIndicator({
  password,
  requirements,
}: {
  password: string;
  requirements: PasswordRequirement[];
}) {
  const strength = getPasswordStrength(password, requirements);
  const color = strength < 40 ? "red" : strength < 70 ? "yellow" : "teal";

  return (
    <Stack gap={6}>
      <Progress value={strength} color={color} size="xs" radius="xl" />
      <List size="xs" spacing={2}>
        {requirements.map((req) => {
          const met = req.re.test(password);
          return (
            <List.Item
              key={req.label}
              icon={
                <ThemeIcon
                  color={met ? "teal" : "red"}
                  size={14}
                  radius="xl"
                  style={{ border: "none" }}
                >
                  {met ? <CheckCircle size={10} /> : <XCircle size={10} />}
                </ThemeIcon>
              }
            >
              <Text size="xs" c={met ? "teal" : "dimmed"}>
                {req.label}
              </Text>
            </List.Item>
          );
        })}
      </List>
    </Stack>
  );
}

interface ChangePasswordModalProps {
  opened: boolean;
  onClose: () => void;
}

export function ChangePasswordModal({
  opened,
  onClose,
}: ChangePasswordModalProps) {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);

  const requirements: PasswordRequirement[] = [
    { re: /.{8,}/, label: t("auth.passwordRequirements.length") },
    { re: /[0-9]/, label: t("auth.passwordRequirements.number") },
    { re: /[a-z]/, label: t("auth.passwordRequirements.lowercase") },
    { re: /[A-Z]/, label: t("auth.passwordRequirements.uppercase") },
    {
      re: /[$&+,:;=?@#|'<>.^*()%!-]/,
      label: t("auth.passwordRequirements.symbol"),
    },
  ];

  const form = useForm<FormValues>({
    mode: "controlled",
    initialValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
    validate: schemaResolver(changePasswordFormSchema, { sync: true }),
  });

  const handleClose = () => {
    form.reset();
    onClose();
  };

  const handleSubmit = async (values: FormValues) => {
    setLoading(true);
    try {
      await changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      });
      notifications.show({
        title: t("auth.common.success"),
        message: t("changePassword.successMessage"),
        color: "green",
      });
      form.reset();
      onClose();
    } catch (err) {
      console.error(err);
      notifications.show({
        title: t("auth.common.error"),
        message: t("changePassword.errorMessage"),
        color: "red",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      opened={opened}
      onClose={handleClose}
      title={
        <Text fw={600} size="md">
          {t("changePassword.title")}
        </Text>
      }
      size="sm"
      centered
    >
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="md">
          <PasswordInput
            label={t("changePassword.currentPasswordLabel")}
            placeholder={t("changePassword.currentPasswordPlaceholder")}
            {...form.getInputProps("currentPassword")}
            id="change-password-current"
          />

          <Stack gap={6}>
            <PasswordInput
              label={t("changePassword.newPasswordLabel")}
              placeholder={t("changePassword.newPasswordPlaceholder")}
              {...form.getInputProps("newPassword")}
              id="change-password-new"
            />
            {form.values.newPassword.length > 0 && (
              <PasswordStrengthIndicator
                password={form.values.newPassword}
                requirements={requirements}
              />
            )}
          </Stack>

          <PasswordInput
            label={t("changePassword.confirmPasswordLabel")}
            placeholder={t("changePassword.confirmPasswordPlaceholder")}
            {...form.getInputProps("confirmPassword")}
            error={
              form.errors.confirmPassword
                ? t("auth.validation.passwordsMustMatch")
                : undefined
            }
            id="change-password-confirm"
          />

          <Group justify="flex-end" mt="xs">
            <Button variant="default" onClick={handleClose} disabled={loading}>
              {t("changePassword.cancel")}
            </Button>
            <Button type="submit" loading={loading} id="change-password-submit">
              {t("changePassword.submit")}
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  );
}
