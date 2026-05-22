import { useForm } from "@mantine/form";
import { useTranslation } from "react-i18next";
import type { GetResourceByIdResponse } from "../../services";

export interface ResourceFormValues {
  name: string;
  description: string;
  file: File | null;
}

const initialValues: ResourceFormValues = {
  name: "",
  description: "",
  file: null,
};

export function useResourceForm(
  initialData: GetResourceByIdResponse | null,
  onSubmit: (data: ResourceFormValues) => void,
) {
  const { t } = useTranslation();

  const form = useForm<ResourceFormValues>({
    mode: "uncontrolled",
    initialValues: initialData
      ? {
          name: initialData.name,
          description: initialData.description ?? "",
          file: null,
        }
      : initialValues,
    validate: {
      name: (value) => {
        if (!value || value.trim().length === 0) {
          return t("resource.validation.nameRequired");
        }
        if (value.length > 100) {
          return t("resource.validation.nameTooLong");
        }
        return null;
      },
      description: (value) => {
        if (value && value.length > 1000) {
          return t("resource.validation.descriptionTooLong");
        }
        return null;
      },
      file: (value) => {
        if (!initialData && !value) {
          return t("resource.validation.fileRequired");
        }
        return null;
      },
    },
  });

  const handleSubmit = form.onSubmit((values) => {
    onSubmit({
      name: values.name.trim(),
      description: values.description.trim(),
      file: values.file,
    });
  });

  return { t, form, handleSubmit };
}
