import {
  Modal,
  Box,
  LoadingOverlay,
  Center,
  Loader,
  Text,
  Stack,
  Group,
  Pagination,
  Table,
  Badge,
  ActionIcon,
  Button,
} from "@mantine/core";
import { Eye } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { QuizPageItem } from "../../../services";
import { useQuizHistory } from "./useQuizHistory";
import { SubmissionReview } from "./SubmissionReview";

interface HistoryModalProps {
  quiz: QuizPageItem | null;
  onClose: () => void;
}

export function HistoryModal({ quiz, onClose }: HistoryModalProps) {
  const { t } = useTranslation();
  const {
    loading: historyLoading,
    submissions: historySubmissions,
    totalPages: historyTotalPages,
    page: historyPage,
    selectedSubmission,
    selectedSubmissionLoading,
    changePage: changeHistoryPage,
    loadSubmissionDetail,
    closeSubmissionDetail,
  } = useQuizHistory(quiz);

  return (
    <>
      <Modal
        opened={!!quiz}
        onClose={onClose}
        title={t("quiz.history.title", { name: quiz?.name })}
        size="lg"
      >
        <Box pos="relative" mih={200}>
          <LoadingOverlay
            visible={historyLoading && historySubmissions.length > 0}
            overlayProps={{ radius: "sm", blur: 1.5 }}
          />
          {historyLoading && historySubmissions.length === 0 ? (
            <Center py="xl">
              <Loader size="md" />
            </Center>
          ) : historySubmissions.length === 0 ? (
            <Text c="dimmed" ta="center" py="xl">
              {t("quiz.history.empty")}
            </Text>
          ) : (
            <Stack gap="md">
              <Table.ScrollContainer minWidth={600}>
                <Table striped highlightOnHover>
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>{t("quiz.history.table.date")}</Table.Th>
                      <Table.Th style={{ textAlign: "center" }}>
                        {t("quiz.history.table.score")}
                      </Table.Th>
                      <Table.Th style={{ textAlign: "center" }}>
                        {t("quiz.history.table.status")}
                      </Table.Th>
                      <Table.Th w={80}>
                        {t("quiz.history.table.actions")}
                      </Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {historySubmissions.map((sub) => (
                      <Table.Tr key={sub.submissionId}>
                        <Table.Td>
                          {new Date(sub.submittedAt).toLocaleString()}
                        </Table.Td>
                        <Table.Td
                          style={{ textAlign: "center", fontWeight: 600 }}
                        >
                          {`${sub.score}/${sub.totalQuestions}`}
                        </Table.Td>
                        <Table.Td style={{ textAlign: "center" }}>
                          {sub.isPassed ? (
                            <Badge color="green">
                              {t("quiz.history.statusPassed")}
                            </Badge>
                          ) : (
                            <Badge color="red">
                              {t("quiz.history.statusFailed")}
                            </Badge>
                          )}
                        </Table.Td>
                        <Table.Td>
                          <ActionIcon
                            variant="subtle"
                            onClick={() =>
                              loadSubmissionDetail(sub.submissionId)
                            }
                            title={t("quiz.history.viewDetail")}
                            aria-label={t("quiz.history.viewDetail")}
                          >
                            <Eye size={16} />
                          </ActionIcon>
                        </Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
              </Table.ScrollContainer>
              {historyTotalPages > 1 && (
                <Group justify="center" mt="sm">
                  <Pagination
                    total={historyTotalPages}
                    value={historyPage}
                    onChange={changeHistoryPage}
                  />
                </Group>
              )}
            </Stack>
          )}
        </Box>
      </Modal>

      {/* Submission Detail Modal */}
      <Modal
        opened={!!selectedSubmission}
        onClose={closeSubmissionDetail}
        title={t("quiz.history.detailTitle", {
          date: selectedSubmission
            ? new Date(selectedSubmission.submittedAt).toLocaleString()
            : "",
        })}
        size="lg"
      >
        <Box pos="relative" mih={200}>
          <LoadingOverlay
            visible={selectedSubmissionLoading && !!selectedSubmission}
            overlayProps={{ radius: "sm", blur: 1.5 }}
          />
          {selectedSubmissionLoading && !selectedSubmission ? (
            <Center py="xl">
              <Loader size="md" />
            </Center>
          ) : selectedSubmission ? (
            <Stack gap="md">
              <SubmissionReview
                score={selectedSubmission.score}
                totalQuestions={selectedSubmission.totalQuestions}
                isPassed={selectedSubmission.isPassed}
                questions={selectedSubmission.questions.map((q) => ({
                  questionSnapshotId: q.questionSnapshotId,
                  text: q.text,
                  explanation: q.explanation,
                  selectedOptionId: q.selectedOptionId,
                  correctOptionId: q.correctOptionId,
                  isCorrect: q.isCorrect,
                  options: q.options,
                }))}
                t={t}
              />
              <Group justify="flex-end" mt="md">
                <Button onClick={closeSubmissionDetail}>
                  {t("quiz.attempt.close")}
                </Button>
              </Group>
            </Stack>
          ) : null}
        </Box>
      </Modal>
    </>
  );
}
