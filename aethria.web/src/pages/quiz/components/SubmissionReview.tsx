import {
  Stack,
  Card,
  Group,
  RingProgress,
  Text,
  Center,
  Badge,
  Box,
} from "@mantine/core";
import type { TFunction } from "i18next";
import type { ReviewQuestion } from "./ReviewQuestion";

interface SubmissionReviewProps {
  score: number;
  totalQuestions: number;
  isPassed: boolean;
  questions: ReviewQuestion[];
  t: TFunction;
}

export function SubmissionReview({
  score,
  totalQuestions,
  isPassed,
  questions,
  t,
}: SubmissionReviewProps) {
  const percent =
    totalQuestions > 0 ? Math.round((score / totalQuestions) * 100) : 0;

  return (
    <Stack gap="md">
      <Card withBorder p="md" radius="md">
        <Group justify="center" gap="xl" py="xs">
          <RingProgress
            size={120}
            thickness={10}
            roundCaps
            sections={[
              {
                value: totalQuestions > 0 ? (score / totalQuestions) * 100 : 0,
                color: isPassed ? "green" : "red",
              },
            ]}
            label={
              <Center>
                <Text fw={700} size="xl">{`${score}/${totalQuestions}`}</Text>
              </Center>
            }
          />
          <Stack gap="xs">
            <Group gap="xs">
              <Text fw={600} size="lg">
                {t("quiz.review.result")}
              </Text>
              {isPassed ? (
                <Badge color="green" size="lg">
                  {t("quiz.review.passed")}
                </Badge>
              ) : (
                <Badge color="red" size="lg">
                  {t("quiz.review.failed")}
                </Badge>
              )}
            </Group>
            <Text size="sm" c="dimmed">
              {t("quiz.review.accuracy", { percent })}
            </Text>
          </Stack>
        </Group>
      </Card>

      <Text fw={600} size="md" mt="xs">
        {t("quiz.review.reviewQuestions")}
      </Text>

      {questions.map((q, idx) => (
        <Card
          key={q.questionSnapshotId}
          withBorder
          p="md"
          radius="md"
          style={{
            borderColor: q.isCorrect ? "#4ade80" : "#f87171",
            backgroundColor: q.isCorrect
              ? "rgba(74, 222, 128, 0.02)"
              : "rgba(239, 68, 68, 0.02)",
          }}
        >
          <Stack gap="xs">
            <Group justify="space-between" align="flex-start" wrap="nowrap">
              <Text fw={600} size="sm" style={{ flex: 1, paddingRight: 8 }}>
                {t("quiz.review.questionLabel", {
                  index: idx + 1,
                  text: q.text,
                })}
              </Text>
              {q.isCorrect ? (
                <Badge color="green" variant="light">
                  {t("quiz.review.correct")}
                </Badge>
              ) : (
                <Badge color="red" variant="light">
                  {t("quiz.review.wrong")}
                </Badge>
              )}
            </Group>

            <Stack gap="xs" mt="xs">
              {q.options.map((opt) => {
                const isSelected = opt.id === q.selectedOptionId;
                const isCorrectOpt = opt.id === q.correctOptionId;

                let borderStyle = "1px solid rgba(0, 0, 0, 0.05)";
                let bg = "transparent";
                let fontWeight: 400 | 600 = 400;

                if (isCorrectOpt) {
                  borderStyle = "1px solid #4ade80";
                  bg = "rgba(74, 222, 128, 0.08)";
                  fontWeight = 600;
                } else if (isSelected && !q.isCorrect) {
                  borderStyle = "1px solid #ef4444";
                  bg = "rgba(239, 68, 68, 0.08)";
                  fontWeight = 600;
                } else if (isSelected) {
                  fontWeight = 600;
                }

                return (
                  <Group
                    key={opt.id}
                    p="xs"
                    style={{
                      border: borderStyle,
                      backgroundColor: bg,
                      borderRadius: 6,
                    }}
                    justify="space-between"
                  >
                    <Text size="sm" style={{ fontWeight }}>
                      {opt.text}
                    </Text>
                    {isCorrectOpt && (
                      <Badge color="green" size="xs">
                        {t("quiz.review.correctAnswer")}
                      </Badge>
                    )}
                    {isSelected && !q.isCorrect && (
                      <Badge color="red" size="xs">
                        {t("quiz.review.yourAnswer")}
                      </Badge>
                    )}
                  </Group>
                );
              })}
            </Stack>

            {q.explanation && (
              <Box
                mt="xs"
                p="xs"
                style={{
                  backgroundColor: "rgba(0, 0, 0, 0.02)",
                  borderRadius: 4,
                  borderLeft: "3px solid #228be6",
                }}
              >
                <Text size="xs" fw={700} c="dimmed">
                  {t("quiz.review.explanationLabel")}
                </Text>
                <Text size="sm" style={{ whiteSpace: "pre-wrap" }}>
                  {q.explanation}
                </Text>
              </Box>
            )}
          </Stack>
        </Card>
      ))}
    </Stack>
  );
}
