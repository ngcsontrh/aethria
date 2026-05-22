import { useForm } from "@mantine/form";
import { useTranslation } from "react-i18next";

export interface RoadmapFormValues {
  name: string;
  description: string;
  resourceId: string;
  prompt: string;
}

const initialValues: RoadmapFormValues = {
  name: "",
  description: "",
  resourceId: "",
  prompt: "",
};

export function useRoadmapForm(onSubmit: (data: RoadmapFormValues) => void) {
  const { t } = useTranslation();

  const form = useForm<RoadmapFormValues>({
    mode: "uncontrolled",
    initialValues,
    validate: {
      name: (value) => {
        if (!value || value.trim().length === 0) {
          return t("roadmap.validation.nameRequired");
        }
        return null;
      },
      resourceId: (value) => {
        if (!value || value.trim().length === 0) {
          return t("roadmap.validation.resourceRequired");
        }
        return null;
      },
    },
  });

  const handleSubmit = form.onSubmit((values) => {
    onSubmit({
      name: values.name.trim(),
      description: values.description.trim(),
      resourceId: values.resourceId,
      prompt: values.prompt.trim(),
    });
  });

  return {
    t,
    form,
    handleSubmit,
  };
}
