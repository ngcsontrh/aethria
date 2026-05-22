import {
  Modal,
  Box,
  LoadingOverlay,
  Center,
  Loader,
  Text,
  Stack,
  Group,
  Button,
  Radio,
} from "@mantine/core";
import { useTranslation } from "react-i18next";
import type { QuizPageItem } from "../../../services";
import { SubmissionReview } from "./SubmissionReview";
import type { ReviewQuestion } from "./ReviewQuestion";
import { useAttemptQuiz } from "./useAttemptQuiz";

interface AttemptModalProps {
  quiz: QuizPageItem | null;
  onClose: () => void;
}

export function AttemptModal({ quiz, onClose }: AttemptModalProps) {
  const { t } = useTranslation();
  const {
    loading: attemptLoading,
    submitting: attemptSubmitting,
    questions: attemptQuestions,
    answers: attemptAnswers,
    result: attemptResult,
    setAnswer: setAttemptAnswer,
    submitAttempt,
  } = useAttemptQuiz(quiz);

  const reviewQuestions: ReviewQuestion[] = attemptResult
    ? attemptResult.answerResults.map((res) => {
        const originalQ = attemptQuestions.find(
          (q) => q.id === res.questionSnapshotId,
        );
        return {
          questionSnapshotId: res.questionSnapshotId,
          text: originalQ?.text ?? "",
          explanation: res.explanation,
          selectedOptionId: res.selectedOptionId,
          correctOptionId: res.correctOptionId,
          isCorrect: res.isCorrect,
          options: originalQ?.options ?? [],
        };
      })
    : [];

  return (
    <Modal
      opened={!!quiz}
      onClose={onClose}
      title={t("quiz.attempt.title", { name: quiz?.name })}
      size="lg"
      closeOnClickOutside={!attemptSubmitting}
      closeOnEscape={!attemptSubmitting}
      withCloseButton={!attemptSubmitting}
    >
      <Box pos="relative" mih={200}>
        <LoadingOverlay
          visible={attemptSubmitting}
          overlayProps={{ radius: "sm", blur: 1.5 }}
        />
        {attemptLoading ? (
          <Center py="xl">
            <Loader size="md" />
          </Center>
        ) : attemptQuestions.length === 0 ? (
          <Text c="dimmed" ta="center" py="xl">
            {t("quiz.attempt.emptyQuestions")}
          </Text>
        ) : attemptResult ? (
          <Stack gap="md">
            <SubmissionReview
              score={attemptResult.score}
              totalQuestions={attemptResult.totalQuestions}
              isPassed={attemptResult.isPassed}
              questions={reviewQuestions}
              t={t}
            />
            <Group justify="flex-end" mt="md">
              <Button onClick={onClose}>{t("quiz.attempt.close")}</Button>
            </Group>
          </Stack>
        ) : (
          <Stack gap="md">
            {attemptQuestions.map((q, idx) => (
              <Stack
                key={q.id}
                gap="xs"
                p="md"
                style={{
                  border: "1px solid rgba(0,0,0,0.08)",
                  borderRadius: 8,
                }}
              >
                <Text fw={600}>
                  {t("quiz.review.questionLabel", {
                    index: idx + 1,
                    text: q.text,
                  })}
                </Text>
                <Radio.Group
                  value={attemptAnswers[q.id] || ""}
                  onChange={(val) => setAttemptAnswer(q.id, val)}
                >
                  <Stack gap="xs" mt="xs">
                    {q.options.map((opt) => (
                      <Radio key={opt.id} value={opt.id} label={opt.text} />
                    ))}
                  </Stack>
                </Radio.Group>
              </Stack>
            ))}
            <Group justify="flex-end" mt="md">
              <Button variant="default" onClick={onClose}>
                {t("quiz.form.cancel")}
              </Button>
              <Button
                onClick={submitAttempt}
                loading={attemptSubmitting}
                disabled={
                  Object.keys(attemptAnswers).length < attemptQuestions.length
                }
              >
                {t("quiz.attempt.submit")}
              </Button>
            </Group>
          </Stack>
        )}
      </Box>
    </Modal>
  );
}
