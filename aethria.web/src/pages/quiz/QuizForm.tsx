import {
  Box,
  Button,
  Group,
  Stack,
  TextInput,
  Textarea,
  Select,
  SegmentedControl,
  NumberInput,
  Text,
  Tabs,
  Accordion,
  ActionIcon,
  Radio,
  Progress,
  Paper,
  Loader,
  Modal,
} from "@mantine/core";
import { Plus, Trash2, Settings, HelpCircle } from "lucide-react";
import {
  useQuizForm,
  type EditingQuizData,
  type QuizFormValues,
} from "./useQuizForm";

interface QuizFormProps {
  opened: boolean;
  initialData: EditingQuizData | null;
  onClose: () => void;
  onSubmit: (data: QuizFormValues) => void;
  submitting: boolean;
  aiGenerating: boolean;
  aiStatus: string;
  aiMessageIndex: number;
  resourcesOptions: { value: string; label: string }[];
  stopAiGeneration: () => void;
}

export default function QuizForm({
  opened,
  initialData,
  onClose,
  onSubmit,
  submitting,
  aiGenerating,
  aiStatus,
  aiMessageIndex,
  resourcesOptions,
  stopAiGeneration,
}: QuizFormProps) {
  const {
    t,
    form,
    addQuestion,
    removeQuestion,
    addOption,
    removeOption,
    setCorrectOption,
    handleSubmit,
  } = useQuizForm(initialData, onSubmit);

  const values = form.getValues();

  const renderFormContent = () => {
    // If AI generation is active, show the progress panel
    if (aiGenerating) {
      let progressPercent = 10;
      let progressColor = "blue";
      const loadingMessages = t("quiz.aiProgress.loadingMessages", {
        returnObjects: true,
      }) as string[];
      const fallbackMessages = [
        t("quiz.aiProgress.started"),
        t("quiz.aiProgress.generating"),
      ];
      const messages = Array.isArray(loadingMessages)
        ? loadingMessages
        : fallbackMessages;
      const loadingMessage = messages[aiMessageIndex % messages.length];

      if (aiStatus === "Started") {
        progressPercent = 30;
      } else if (aiStatus === "Completed") {
        progressPercent = 100;
        progressColor = "green";
      } else if (aiStatus === "Failed") {
        progressPercent = 100;
        progressColor = "red";
      }

      return (
        <Stack align="center" justify="center" py="xl" gap="md">
          <Loader size="lg" color={progressColor} />
          <Text fw={500} size="lg">
            {t("quiz.aiProgress.title")}
          </Text>
          <Box style={{ width: "100%", maxWidth: 450 }}>
            <Progress
              value={progressPercent}
              color={progressColor}
              animated={aiStatus !== "Completed" && aiStatus !== "Failed"}
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
              {t("quiz.aiProgress.statusLabel")}: {aiStatus}
            </Text>
            <Text size="sm" style={{ whiteSpace: "pre-wrap" }}>
              {aiStatus === "Completed"
                ? t("quiz.aiProgress.completed")
                : aiStatus === "Failed"
                  ? t("quiz.aiProgress.failed")
                  : aiStatus === "Canceled"
                    ? t("quiz.aiProgress.canceled")
                    : loadingMessage}
            </Text>
          </Paper>
          {aiStatus !== "Completed" && aiStatus !== "Failed" && aiStatus !== "Canceled" && (
            <Button variant="outline" color="red" onClick={stopAiGeneration}>
              {t("quiz.form.cancel")}
            </Button>
          )}
        </Stack>
      );
    }

    // Edit Mode UI (Tabs layout)
    if (initialData) {
      return (
        <form onSubmit={handleSubmit} noValidate>
          <Tabs defaultValue="settings">
            <Tabs.List mb="md">
              <Tabs.Tab value="settings" leftSection={<Settings size={14} />}>
                Cấu hình chung
              </Tabs.Tab>
              <Tabs.Tab
                value="questions"
                leftSection={<HelpCircle size={14} />}
              >
                Câu hỏi ({values.questions.length})
              </Tabs.Tab>
            </Tabs.List>

            <Tabs.Panel value="settings">
              <Stack gap="sm">
                <TextInput
                  key={form.key("name")}
                  label={t("quiz.form.name")}
                  required
                  maxLength={100}
                  {...form.getInputProps("name")}
                />
                <Textarea
                  key={form.key("description")}
                  label={t("quiz.form.description")}
                  maxLength={1000}
                  minRows={3}
                  {...form.getInputProps("description")}
                />
              </Stack>
            </Tabs.Panel>

            <Tabs.Panel value="questions">
              <Stack gap="md">
                <Group justify="space-between">
                  <Text size="sm" c="dimmed">
                    Quản lý danh sách câu hỏi và đáp án cho bài trắc nghiệm này.
                  </Text>
                  <Button
                    size="xs"
                    variant="light"
                    leftSection={<Plus size={14} />}
                    onClick={addQuestion}
                  >
                    Thêm câu hỏi
                  </Button>
                </Group>

                {values.questions.length === 0 ? (
                  <Paper
                    p="xl"
                    withBorder
                    style={{ textAlign: "center", borderStyle: "dashed" }}
                  >
                    <Text c="dimmed">
                      Chưa có câu hỏi nào. Nhấp vào nút "Thêm câu hỏi" ở trên để
                      bắt đầu.
                    </Text>
                  </Paper>
                ) : (
                  <Accordion variant="separated">
                    {values.questions.map((q, qIndex) => (
                      <Accordion.Item key={q.key} value={q.key}>
                        <Accordion.Control>
                          <Group
                            justify="space-between"
                            wrap="nowrap"
                            style={{ width: "95%" }}
                          >
                            <Text fw={500} size="sm" truncate>
                              {`Câu ${qIndex + 1}: ${q.text || "(Chưa nhập nội dung)"}`}
                            </Text>
                            <ActionIcon
                              color="red"
                              variant="subtle"
                              size="sm"
                              onClick={(e) => {
                                e.stopPropagation();
                                removeQuestion(qIndex);
                              }}
                            >
                              <Trash2 size={14} />
                            </ActionIcon>
                          </Group>
                        </Accordion.Control>
                        <Accordion.Panel>
                          <Stack gap="sm">
                            <Textarea
                              key={form.key(`questions.${qIndex}.text`)}
                              label="Nội dung câu hỏi"
                              required
                              minRows={2}
                              {...form.getInputProps(
                                `questions.${qIndex}.text`,
                              )}
                            />

                            <Text size="xs" fw={500} mt={4}>
                              Các lựa chọn trả lời (Tích chọn để làm đáp án
                              đúng)
                            </Text>

                            <Stack gap="xs">
                              {q.options.map((opt, optIndex) => (
                                <Group
                                  key={opt.key}
                                  gap="xs"
                                  align="center"
                                  wrap="nowrap"
                                >
                                  <Radio
                                    checked={q.correctOptionIndex === optIndex}
                                    onChange={() =>
                                      setCorrectOption(qIndex, optIndex)
                                    }
                                    aria-label="Mark correct answer"
                                  />
                                  <TextInput
                                    style={{ flex: 1 }}
                                    placeholder={`Lựa chọn ${optIndex + 1}`}
                                    key={form.key(
                                      `questions.${qIndex}.options.${optIndex}.text`,
                                    )}
                                    {...form.getInputProps(
                                      `questions.${qIndex}.options.${optIndex}.text`,
                                    )}
                                  />
                                  {q.options.length > 2 && (
                                    <ActionIcon
                                      color="red"
                                      variant="subtle"
                                      onClick={() =>
                                        removeOption(qIndex, optIndex)
                                      }
                                    >
                                      <Trash2 size={14} />
                                    </ActionIcon>
                                  )}
                                </Group>
                              ))}
                            </Stack>

                            <Button
                              size="xs"
                              variant="subtle"
                              leftSection={<Plus size={12} />}
                              onClick={() => addOption(qIndex)}
                              style={{ alignSelf: "flex-start" }}
                              mt={4}
                            >
                              Thêm lựa chọn
                            </Button>

                            <Textarea
                              key={form.key(`questions.${qIndex}.explanation`)}
                              label="Giải thích đáp án"
                              required
                              minRows={2}
                              placeholder="Tại sao đáp án này lại đúng..."
                              {...form.getInputProps(
                                `questions.${qIndex}.explanation`,
                              )}
                            />
                          </Stack>
                        </Accordion.Panel>
                      </Accordion.Item>
                    ))}
                  </Accordion>
                )}
              </Stack>
            </Tabs.Panel>

            <Group justify="flex-end" mt="xl">
              <Button variant="default" onClick={onClose}>
                {t("quiz.form.cancel")}
              </Button>
              <Button type="submit" loading={submitting}>
                {t("quiz.form.save")}
              </Button>
            </Group>
          </Tabs>
        </form>
      );
    }

    // Creation Mode UI
    return (
      <form onSubmit={handleSubmit} noValidate>
        <Stack gap="sm">
          <Box>
            <Text size="sm" fw={500} mb={4}>
              {t("quiz.form.mode")}
            </Text>
            <SegmentedControl
              fullWidth
              key={form.key("mode")}
              {...form.getInputProps("mode")}
              data={[
                { value: "manual", label: t("quiz.form.modeManual") },
                { value: "ai", label: t("quiz.form.modeAI") },
              ]}
            />
          </Box>

          <TextInput
            key={form.key("name")}
            label={t("quiz.form.name")}
            required
            maxLength={100}
            {...form.getInputProps("name")}
          />

          <Textarea
            key={form.key("description")}
            label={t("quiz.form.description")}
            maxLength={1000}
            minRows={2}
            {...form.getInputProps("description")}
          />

          {values.mode === "manual" ? (
            <Select
              key={form.key("resourceId")}
              label={t("quiz.form.resource")}
              placeholder={t("quiz.form.resourceSelect")}
              data={resourcesOptions}
              clearable
              {...form.getInputProps("resourceId")}
            />
          ) : (
            <>
              <Select
                key={form.key("resourceId")}
                label={t("quiz.form.resource")}
                placeholder={t("quiz.form.resourceSelect")}
                data={resourcesOptions}
                required
                {...form.getInputProps("resourceId")}
              />

              <TextInput
                key={form.key("prompt")}
                label={t("quiz.form.prompt")}
                placeholder={t("quiz.form.promptPlaceholder")}
                {...form.getInputProps("prompt")}
              />

              <NumberInput
                key={form.key("numberOfQuestions")}
                label={t("quiz.form.numberOfQuestions")}
                required
                min={1}
                max={50}
                {...form.getInputProps("numberOfQuestions")}
              />
            </>
          )}

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onClose}>
              {t("quiz.form.cancel")}
            </Button>
            <Button type="submit" loading={submitting}>
              {values.mode === "ai"
                ? t("quiz.form.generate")
                : t("quiz.form.save")}
            </Button>
          </Group>
        </Stack>
      </form>
    );
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={initialData ? t("quiz.editTitle") : t("quiz.createTitle")}
      size={initialData ? "xl" : "lg"}
      closeOnClickOutside={!aiGenerating}
      closeOnEscape={!aiGenerating}
      withCloseButton={!aiGenerating}
    >
      {renderFormContent()}
    </Modal>
  );
}
