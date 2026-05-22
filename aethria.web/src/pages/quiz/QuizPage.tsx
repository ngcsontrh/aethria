import {
  Box,
  Title,
  Button,
  Group,
  Pagination,
  Modal,
  Text,
} from "@mantine/core";
import { Plus } from "lucide-react";
import { useQuizPage } from "./useQuizPage";
import QuizForm from "./QuizForm";
import { QuizTable } from "./components/QuizTable";
import { AttemptModal } from "./components/AttemptModal";
import { HistoryModal } from "./components/HistoryModal";

export default function QuizPage() {
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
    aiMessage,
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
        <Button leftSection={<Plus size={16} />} onClick={openCreate}>
          {t("quiz.create")}
        </Button>
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
          aiMessage={aiMessage}
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
