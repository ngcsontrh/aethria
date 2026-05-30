import {
  Box,
  Button,
  Group,
  Stack,
  TextInput,
  Textarea,
  Select,
  Text,
  Progress,
  Paper,
  Loader,
} from "@mantine/core";
import { useRoadmapForm, type RoadmapFormValues } from "./useRoadmapForm";

interface RoadmapFormProps {
  resourcesOptions: { value: string; label: string }[];
  onSubmit: (data: RoadmapFormValues) => void;
  onCancel: () => void;
  submitting: boolean;
  aiGenerating: boolean;
  aiStatus: string;
  aiMessageIndex: number;
  stopAiGeneration: () => void;
}

export default function RoadmapForm({
  resourcesOptions,
  onSubmit,
  onCancel,
  submitting,
  aiGenerating,
  aiStatus,
  aiMessageIndex,
  stopAiGeneration,
}: RoadmapFormProps) {
  const { t, form, handleSubmit } = useRoadmapForm(onSubmit);

  // If AI generation is active, show the progress panel
  if (aiGenerating) {
    let progressPercent = 10;
    let progressColor = "blue";
    const loadingMessages = t("roadmap.aiProgress.loadingMessages", {
      returnObjects: true,
    }) as string[];
    const fallbackMessages = [
      t("roadmap.aiProgress.started"),
      t("roadmap.aiProgress.generating"),
    ];
    const messages = Array.isArray(loadingMessages)
      ? loadingMessages
      : fallbackMessages;
    const loadingMessage = messages[aiMessageIndex % messages.length];

    if (aiStatus === "Started") {
      progressPercent = 35;
    } else if (aiStatus === "Completed") {
      progressPercent = 100;
      progressColor = "green";
    } else if (aiStatus === "Failed" || aiStatus === "Canceled") {
      progressPercent = 100;
      progressColor = "red";
    }

    return (
      <Stack align="center" justify="center" py="xl" gap="md">
        <Loader size="lg" color={progressColor} />
        <Text fw={500} size="lg">
          {t("roadmap.aiProgress.title")}
        </Text>
        <Box style={{ width: "100%", maxWidth: 450 }}>
          <Progress
            value={progressPercent}
            color={progressColor}
            animated={
              aiStatus !== "Completed" &&
              aiStatus !== "Failed" &&
              aiStatus !== "Canceled"
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
            {t("roadmap.aiProgress.statusLabel")}: {aiStatus}
          </Text>
          <Text size="sm" style={{ whiteSpace: "pre-wrap" }}>
            {aiStatus === "Completed"
              ? t("roadmap.aiProgress.completed")
              : aiStatus === "Failed"
                ? t("roadmap.aiProgress.failed")
                : aiStatus === "Canceled"
                  ? t("roadmap.aiProgress.canceled")
                  : loadingMessage}
          </Text>
        </Paper>
        {aiStatus !== "Completed" &&
          aiStatus !== "Failed" &&
          aiStatus !== "Canceled" && (
            <Button variant="outline" color="red" onClick={stopAiGeneration}>
              {t("roadmap.form.cancel")}
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
          label={t("roadmap.form.name")}
          required
          maxLength={100}
          {...form.getInputProps("name")}
        />

        <Textarea
          key={form.key("description")}
          label={t("roadmap.form.description")}
          maxLength={1000}
          minRows={2}
          {...form.getInputProps("description")}
        />

        <Select
          key={form.key("resourceId")}
          label={t("roadmap.form.resource")}
          placeholder={t("roadmap.form.resourceSelect")}
          data={resourcesOptions}
          required
          {...form.getInputProps("resourceId")}
        />

        <TextInput
          key={form.key("prompt")}
          label={t("roadmap.form.prompt")}
          placeholder={t("roadmap.form.promptPlaceholder")}
          {...form.getInputProps("prompt")}
        />

        <Group justify="flex-end" mt="md">
          <Button variant="default" onClick={onCancel}>
            {t("roadmap.form.cancel")}
          </Button>
          <Button type="submit" loading={submitting}>
            {t("roadmap.form.generate")}
          </Button>
        </Group>
      </Stack>
    </form>
  );
}
