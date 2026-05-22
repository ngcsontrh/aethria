import {
  Box,
  Button,
  Checkbox,
  Group,
  Stack,
  Text,
  TextInput,
  Textarea,
} from "@mantine/core";
import type { GetMentorByIdResponse } from "../../services";
import { useMentorForm, AVAILABLE_TOOLS } from "./useMentorForm";
import type { MentorFormValues } from "./useMentorForm";

interface MentorFormProps {
  initialData: GetMentorByIdResponse | null;
  onSubmit: (data: MentorFormValues) => void;
  onCancel: () => void;
  submitting: boolean;
}

export default function MentorForm({
  initialData,
  onSubmit,
  onCancel,
  submitting,
}: MentorFormProps) {
  const { t, form, toggleTool, handleSubmit } = useMentorForm(
    initialData,
    onSubmit,
  );
  const values = form.getValues();

  return (
    <form onSubmit={handleSubmit} noValidate>
      <Stack gap="sm">
        <TextInput
          key={form.key("name")}
          label={t("mentor.form.name")}
          required
          maxLength={100}
          {...form.getInputProps("name")}
        />
        <Textarea
          key={form.key("description")}
          label={t("mentor.form.description")}
          required
          maxLength={1000}
          minRows={2}
          {...form.getInputProps("description")}
        />
        <Textarea
          key={form.key("instruction")}
          label={t("mentor.form.instruction")}
          required
          maxLength={4000}
          minRows={4}
          {...form.getInputProps("instruction")}
        />
        <Box>
          <Text size="sm" fw={500} mb={4}>
            {t("mentor.form.tools")}
          </Text>
          {AVAILABLE_TOOLS.map((tool) => (
            <Checkbox
              key={tool}
              label={t(`chat.tools.${tool}`, tool)}
              checked={values.tools.includes(tool)}
              onChange={() => toggleTool(tool)}
              mb={4}
            />
          ))}
        </Box>
        <Group justify="flex-end" mt="md">
          <Button variant="default" onClick={onCancel}>
            {t("mentor.form.cancel")}
          </Button>
          <Button type="submit" loading={submitting}>
            {t("mentor.form.save")}
          </Button>
        </Group>
      </Stack>
    </form>
  );
}
