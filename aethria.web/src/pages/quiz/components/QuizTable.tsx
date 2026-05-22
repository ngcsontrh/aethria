import {
  Box,
  Table,
  Group,
  ActionIcon,
  Text,
  LoadingOverlay,
  Skeleton,
} from "@mantine/core";
import { Pencil, Trash2, Play, History } from "lucide-react";
import type { TFunction } from "i18next";
import type { QuizPageItem } from "../../../services";

interface QuizTableProps {
  quizzes: QuizPageItem[];
  loading: boolean;
  t: TFunction;
  startAttempt: (q: QuizPageItem) => void;
  openHistory: (q: QuizPageItem) => void;
  openEdit: (id: string) => void;
  setDeleteTarget: (q: QuizPageItem) => void;
}

export function QuizTable({
  quizzes,
  loading,
  t,
  startAttempt,
  openHistory,
  openEdit,
  setDeleteTarget,
}: QuizTableProps) {
  return (
    <Box pos="relative" mih={200}>
      <LoadingOverlay
        visible={loading && quizzes.length > 0}
        overlayProps={{ radius: "sm", blur: 1.5 }}
      />
      <Table.ScrollContainer minWidth={800}>
        <Table striped highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>{t("quiz.table.name")}</Table.Th>
              <Table.Th>{t("quiz.table.description")}</Table.Th>
              <Table.Th w={120} style={{ textAlign: "center" }}>
                {t("quiz.table.questions")}
              </Table.Th>
              <Table.Th>{t("quiz.table.createdAt")}</Table.Th>
              <Table.Th w={150}>{t("quiz.table.actions")}</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {loading && quizzes.length === 0
              ? Array(5)
                  .fill(0)
                  .map((_, index) => (
                    <Table.Tr key={`skeleton-${index}`}>
                      <Table.Td>
                        <Skeleton height={20} radius="sm" width="50%" />
                      </Table.Td>
                      <Table.Td>
                        <Skeleton height={20} radius="sm" width="80%" />
                      </Table.Td>
                      <Table.Td>
                        <Skeleton
                          height={20}
                          radius="sm"
                          width="30%"
                          mx="auto"
                        />
                      </Table.Td>
                      <Table.Td>
                        <Skeleton height={20} radius="sm" width="40%" />
                      </Table.Td>
                      <Table.Td>
                        <Group gap={4}>
                          <Skeleton height={28} width={28} radius="sm" />
                          <Skeleton height={28} width={28} radius="sm" />
                          <Skeleton height={28} width={28} radius="sm" />
                          <Skeleton height={28} width={28} radius="sm" />
                        </Group>
                      </Table.Td>
                    </Table.Tr>
                  ))
              : quizzes.map((q) => (
                  <Table.Tr key={q.id}>
                    <Table.Td style={{ fontWeight: 500 }}>{q.name}</Table.Td>
                    <Table.Td
                      style={{
                        maxWidth: 300,
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                        whiteSpace: "nowrap",
                      }}
                    >
                      {q.description || (
                        <Text span c="dimmed" fs="italic">
                          {t("quiz.noDescription")}
                        </Text>
                      )}
                    </Table.Td>
                    <Table.Td style={{ textAlign: "center" }}>
                      {q.questionCount}
                    </Table.Td>
                    <Table.Td>
                      {new Date(q.createdAt).toLocaleDateString()}
                    </Table.Td>
                    <Table.Td>
                      <Group gap={4}>
                        <ActionIcon
                          variant="subtle"
                          color="blue"
                          onClick={() => startAttempt(q)}
                          disabled={q.questionCount === 0}
                          title={t("quiz.attempt.attemptLabel")}
                          aria-label={t("quiz.attempt.attemptLabel")}
                        >
                          <Play size={16} />
                        </ActionIcon>
                        <ActionIcon
                          variant="subtle"
                          color="teal"
                          onClick={() => openHistory(q)}
                          title={t("quiz.attempt.historyLabel")}
                          aria-label={t("quiz.attempt.historyLabel")}
                        >
                          <History size={16} />
                        </ActionIcon>
                        <ActionIcon
                          variant="subtle"
                          onClick={() => openEdit(q.id)}
                          aria-label={t("quiz.editTitle")}
                        >
                          <Pencil size={16} />
                        </ActionIcon>
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={() => setDeleteTarget(q)}
                          aria-label={t("quiz.deleteTitle")}
                        >
                          <Trash2 size={16} />
                        </ActionIcon>
                      </Group>
                    </Table.Td>
                  </Table.Tr>
                ))}
            {!loading && quizzes.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text ta="center" c="dimmed" py="lg">
                    {t("quiz.empty")}
                  </Text>
                </Table.Td>
              </Table.Tr>
            )}
          </Table.Tbody>
        </Table>
      </Table.ScrollContainer>
    </Box>
  );
}
