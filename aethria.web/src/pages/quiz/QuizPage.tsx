import {
  Box,
  Title,
  Button,
  Group,
  Pagination,
  Modal,
  Text,
  Stack,
  Divider,
  List,
  Alert,
} from "@mantine/core";
import { useState } from "react";
import { Plus, HelpCircle, FileQuestion, History } from "lucide-react";
import { useQuizPage } from "./useQuizPage";
import QuizForm from "./QuizForm";
import { QuizTable } from "./components/QuizTable";
import { AttemptModal } from "./components/AttemptModal";
import { HistoryModal } from "./components/HistoryModal";

export default function QuizPage() {
  const [helpOpened, setHelpOpened] = useState(false);
  const {
    t,
    quizzes,
    totalPages,
    page,
    setPage,
    loading,
    resourcesOptions,
    formOpened,
    editingQuiz,
    deleteTarget,
    setDeleteTarget,
    submitting,
    aiGenerating,
    aiStatus,
    aiMessageIndex,
    stopAiGeneration,
    openCreate,
    openEdit,
    closeForm,
    handleSubmit,
    handleDelete,
    // Attempt
    attemptQuiz,
    startAttempt,
    closeAttempt,
    // History
    historyQuiz,
    openHistory,
    closeHistory,
  } = useQuizPage();

  return (
    <Box p="md" style={{ overflow: "auto", flex: 1 }}>
      <Group justify="space-between" mb="md">
        <Title order={3}>{t("quiz.title")}</Title>
        <Group gap="xs">
          <Button
            variant="light"
            leftSection={<HelpCircle size={16} />}
            onClick={() => setHelpOpened(true)}
          >
            {t("quiz.help.button")}
          </Button>
          <Button leftSection={<Plus size={16} />} onClick={openCreate}>
            {t("quiz.create")}
          </Button>
        </Group>
      </Group>

      <QuizTable
        quizzes={quizzes}
        loading={loading}
        t={t}
        startAttempt={startAttempt}
        openHistory={openHistory}
        openEdit={openEdit}
        setDeleteTarget={setDeleteTarget}
      />

      {totalPages > 1 && (
        <Group justify="center" mt="md">
          <Pagination total={totalPages} value={page} onChange={setPage} />
        </Group>
      )}

      <Modal
        opened={helpOpened}
        onClose={() => setHelpOpened(false)}
        title={
          <Group gap="xs">
            <FileQuestion size={22} color="var(--mantine-color-blue-filled)" />
            <Text fw={700} size="lg" style={{ letterSpacing: 0 }}>
              {t("quiz.help.title")}
            </Text>
          </Group>
        }
        size="lg"
      >
        <Stack gap="md">
          <Text>{t("quiz.help.description")}</Text>

          <Box>
            <Text fw={600} mb="xs">
              {t("quiz.help.usedForTitle")}
            </Text>
            <List spacing="xs">
              <List.Item>{t("quiz.help.usedForItems.create")}</List.Item>
              <List.Item>{t("quiz.help.usedForItems.generate")}</List.Item>
              <List.Item>{t("quiz.help.usedForItems.practice")}</List.Item>
            </List>
          </Box>

          <Divider />

          <Box>
            <Text fw={600} mb="xs">
              {t("quiz.help.howToTitle")}
            </Text>
            <List spacing="xs">
              <List.Item>{t("quiz.help.howToItems.mode")}</List.Item>
              <List.Item>{t("quiz.help.howToItems.ai")}</List.Item>
              <List.Item>{t("quiz.help.howToItems.attempt")}</List.Item>
              <List.Item>{t("quiz.help.howToItems.history")}</List.Item>
            </List>
          </Box>

          <Alert color="blue" icon={<History size={18} />}>
            {t("quiz.help.note")}
          </Alert>
        </Stack>
      </Modal>

      {/* Create / Edit Modal */}
      {formOpened && (
        <QuizForm
          key={editingQuiz?.id ?? "create"}
          opened={formOpened}
          initialData={editingQuiz}
          onClose={closeForm}
          onSubmit={handleSubmit}
          submitting={submitting}
          aiGenerating={aiGenerating}
          aiStatus={aiStatus}
          aiMessageIndex={aiMessageIndex}
          resourcesOptions={resourcesOptions}
          stopAiGeneration={stopAiGeneration}
        />
      )}

      {/* Delete Confirm Modal */}
      <Modal
        opened={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        title={t("quiz.deleteTitle")}
        size="sm"
      >
        <Text mb="md">
          {t("quiz.deleteConfirm", { name: deleteTarget?.name })}
        </Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={() => setDeleteTarget(null)}>
            {t("quiz.form.cancel")}
          </Button>
          <Button color="red" onClick={handleDelete} loading={submitting}>
            {t("quiz.deleteBtn")}
          </Button>
        </Group>
      </Modal>

      <AttemptModal quiz={attemptQuiz} onClose={closeAttempt} />

      <HistoryModal quiz={historyQuiz} onClose={closeHistory} />
    </Box>
  );
}
