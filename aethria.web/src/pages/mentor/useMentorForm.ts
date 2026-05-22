import { schemaResolver, useForm } from "@mantine/form";
import { useTranslation } from "react-i18next";
import {
  CreateMentorRequestSchema,
  UpdateMentorRequestSchema,
  MentorToolSchema,
} from "../../services";
import type { GetMentorByIdResponse } from "../../services";

export const AVAILABLE_TOOLS = MentorToolSchema.options;

export interface MentorFormValues {
  name: string;
  description: string;
  instruction: string;
  tools: string[];
}

const initialValues: MentorFormValues = {
  name: "",
  description: "",
  instruction: "",
  tools: [],
};

export function useMentorForm(
  initialData: GetMentorByIdResponse | null,
  onSubmit: (data: MentorFormValues) => void,
) {
  const { t } = useTranslation();
  const schema = initialData
    ? UpdateMentorRequestSchema
    : CreateMentorRequestSchema;

  const form = useForm<MentorFormValues>({
    mode: "uncontrolled",
    initialValues: initialData
      ? {
          name: initialData.name,
          description: initialData.description,
          instruction: initialData.instruction,
          tools: initialData.tools,
        }
      : initialValues,
    validate: schemaResolver(schema, { sync: true }),
  });

  const toggleTool = (tool: string) => {
    const tools = form.getValues().tools;
    form.setFieldValue(
      "tools",
      tools.includes(tool)
        ? tools.filter((item) => item !== tool)
        : [...tools, tool],
    );
  };

  const handleSubmit = form.onSubmit((values) => {
    onSubmit({
      name: values.name.trim(),
      description: values.description.trim(),
      instruction: values.instruction.trim(),
      tools: values.tools,
    });
  });

  return { t, form, toggleTool, handleSubmit };
}
