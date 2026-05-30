import {
  Button,
  Group,
  Stack,
  TextInput,
  Textarea,
  FileInput,
  Text,
  Box,
  Progress,
  Paper,
  Loader,
} from "@mantine/core";
import { Upload } from "lucide-react";
import type { GetResourceByIdResponse } from "../../services";
import { useResourceForm, type ResourceFormValues } from "./useResourceForm";

interface ResourceFormProps {
  initialData: GetResourceByIdResponse | null;
  onSubmit: (data: ResourceFormValues) => void;
  onCancel: () => void;
  submitting: boolean;
  uploadProcessing: boolean;
  uploadStatus: string;
  uploadMessageIndex: number;
  stopUpload: () => void;
}

export default function ResourceForm({
  initialData,
  onSubmit,
  onCancel,
  submitting,
  uploadProcessing,
  uploadStatus,
  uploadMessageIndex,
  stopUpload,
}: ResourceFormProps) {
  const { t, form, handleSubmit } = useResourceForm(initialData, onSubmit);

  if (uploadProcessing) {
    let progressPercent = 15;
    let progressColor = "blue";
    const loadingMessages = t("resource.uploadProgress.loadingMessages", {
      returnObjects: true,
    }) as string[];
    const fallbackMessages = [
      t("resource.uploadProgress.uploading"),
      t("resource.uploadProgress.embedding"),
    ];
    const messages = Array.isArray(loadingMessages)
      ? loadingMessages
      : fallbackMessages;
    const loadingMessage = messages[uploadMessageIndex % messages.length];

    if (uploadStatus === "Started") {
      progressPercent = 35;
    } else if (uploadStatus === "Completed") {
      progressPercent = 100;
      progressColor = "green";
    } else if (uploadStatus === "Failed" || uploadStatus === "Canceled") {
      progressPercent = 100;
      progressColor = "red";
    }

    return (
      <Stack align="center" justify="center" py="xl" gap="md">
        <Loader size="lg" color={progressColor} />
        <Text fw={500} size="lg">
          {t("resource.uploadProgress.title")}
        </Text>
        <Box style={{ width: "100%", maxWidth: 450 }}>
          <Progress
            value={progressPercent}
            color={progressColor}
            animated={
              uploadStatus !== "Completed" &&
              uploadStatus !== "Failed" &&
              uploadStatus !== "Canceled"
            }
          />
        </Box>
        <Paper
          p="md"
          withBorder
          style={{
            width: "100%",
            maxWidth: 450,
            backgroundColor: "rgba(0, 0, 0, 0.02)",
          }}
        >
          <Text size="xs" fw={700} c="dimmed" mb={4}>
            {t("resource.uploadProgress.statusLabel")}: {uploadStatus}
          </Text>
          <Text size="sm" style={{ whiteSpace: "pre-wrap" }}>
            {uploadStatus === "Completed"
              ? t("resource.uploadProgress.completed")
              : uploadStatus === "Failed"
                ? t("resource.uploadProgress.failed")
                : uploadStatus === "Canceled"
                  ? t("resource.uploadProgress.canceled")
                  : loadingMessage}
          </Text>
        </Paper>
        {uploadStatus !== "Completed" &&
          uploadStatus !== "Failed" &&
          uploadStatus !== "Canceled" && (
            <Button variant="outline" color="red" onClick={stopUpload}>
              {t("resource.form.cancel")}
            </Button>
          )}
      </Stack>
    );
  }

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
