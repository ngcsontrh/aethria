import {
  Button,
  Group,
  Stack,
  TextInput,
  Textarea,
  FileInput,
} from "@mantine/core";
import { Upload } from "lucide-react";
import type { GetResourceByIdResponse } from "../../services";
import { useResourceForm, type ResourceFormValues } from "./useResourceForm";

interface ResourceFormProps {
  initialData: GetResourceByIdResponse | null;
  onSubmit: (data: ResourceFormValues) => void;
  onCancel: () => void;
  submitting: boolean;
}

export default function ResourceForm({
  initialData,
  onSubmit,
  onCancel,
  submitting,
}: ResourceFormProps) {
  const { t, form, handleSubmit } = useResourceForm(initialData, onSubmit);

  return (
    <form onSubmit={handleSubmit} noValidate>
      <Stack gap="sm">
        <TextInput
          key={form.key("name")}
          label={t("resource.form.name")}
          required
          maxLength={100}
          {...form.getInputProps("name")}
        />
        <Textarea
          key={form.key("description")}
          label={t("resource.form.description")}
          maxLength={1000}
          minRows={2}
          {...form.getInputProps("description")}
        />

        {!initialData && (
          <FileInput
            key={form.key("file")}
            label={t("resource.form.file")}
            placeholder={t("resource.form.filePlaceholder")}
            leftSection={<Upload size={16} />}
            required
            clearable
            {...form.getInputProps("file")}
          />
        )}

        <Group justify="flex-end" mt="md">
          <Button variant="default" onClick={onCancel}>
            {t("resource.form.cancel")}
          </Button>
          <Button type="submit" loading={submitting}>
            {t("resource.form.save")}
          </Button>
        </Group>
      </Stack>
    </form>
  );
}
