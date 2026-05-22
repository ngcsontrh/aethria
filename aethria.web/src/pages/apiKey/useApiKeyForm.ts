import { useForm } from "@mantine/form";
import { useTranslation } from "react-i18next";

export interface ApiKeyFormValues {
  name: string;
  expirationDays: number;
}

const initialValues: ApiKeyFormValues = {
  name: "",
  expirationDays: 30, // Default to 30 days
};

export function useApiKeyForm(
  onSubmit: (data: ApiKeyFormValues) => void,
) {
  const { t } = useTranslation();

  const form = useForm<ApiKeyFormValues>({
    mode: "uncontrolled",
    initialValues,
    validate: {
      name: (value) => {
        if (!value || value.trim().length === 0) {
          return t("apiKey.validation.nameRequired");
        }
        if (value.length > 100) {
          return t("apiKey.validation.nameTooLong");
        }
        return null;
      },
      expirationDays: (value) => {
        if (value === undefined || value === null) {
          return t("apiKey.validation.expirationRequired");
        }
        if (value < 1 || value > 365) {
          return t("apiKey.validation.expirationRange");
        }
        return null;
      },
    },
  });

  const handleSubmit = form.onSubmit((values) => {
    onSubmit({
      name: values.name.trim(),
      expirationDays: values.expirationDays,
    });
  });

  return { t, form, handleSubmit };
}
