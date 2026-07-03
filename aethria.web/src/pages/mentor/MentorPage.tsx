import {
  Box,
  Title,
  Button,
  Table,
  Group,
  ActionIcon,
  Pagination,
  Modal,
  Text,
  LoadingOverlay,
  Skeleton,
  Stack,
  Divider,
  List,
  Alert,
} from "@mantine/core";
import { useState } from "react";
import {
  Plus,
  Pencil,
  Trash2,
  HelpCircle,
  Users,
  MessageSquare,
} from "lucide-react";
import { useMentorPage } from "./useMentorPage";
import MentorForm from "./MentorForm";

export default function MentorPage() {
  const [helpOpened, setHelpOpened] = useState(false);
  const {
    t,
    mentors,
    totalPages,
    page,
    setPage,
    loading,
    formOpened,
    editingMentor,
    deleteTarget,
    setDeleteTarget,
    submitting,
    openCreate,
    openEdit,
    closeForm,
    handleSubmit,
    handleDelete,
  } = useMentorPage();

  return (
    <Box p="md" style={{ overflow: "auto", flex: 1 }}>
      <Group justify="space-between" mb="md">
        <Title order={3}>{t("mentor.title")}</Title>
        <Group gap="xs">
          <Button
            variant="light"
            leftSection={<HelpCircle size={16} />}
            onClick={() => setHelpOpened(true)}
          >
            {t("mentor.help.button")}
          </Button>
          <Button leftSection={<Plus size={16} />} onClick={openCreate}>
            {t("mentor.create")}
          </Button>
        </Group>
      </Group>

      <Box pos="relative" mih={200}>
        <LoadingOverlay
          visible={loading && mentors.length > 0}
          overlayProps={{ radius: "sm", blur: 1.5 }}
        />
        <Table.ScrollContainer minWidth={700}>
          <Table striped highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t("mentor.table.name")}</Table.Th>
                <Table.Th>{t("mentor.table.description")}</Table.Th>
                <Table.Th>{t("mentor.table.createdAt")}</Table.Th>
                <Table.Th w={100}>{t("mentor.table.actions")}</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {loading && mentors.length === 0
                ? Array(5)
                    .fill(0)
                    .map((_, index) => (
                      <Table.Tr key={`skeleton-${index}`}>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="60%" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="85%" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="40%" />
                        </Table.Td>
                        <Table.Td>
                          <Group gap={8}>
                            <Skeleton height={28} width={28} radius="sm" />
                            <Skeleton height={28} width={28} radius="sm" />
                          </Group>
                        </Table.Td>
                      </Table.Tr>
                    ))
                : mentors.map((m) => (
                    <Table.Tr key={m.id}>
                      <Table.Td>{m.name}</Table.Td>
                      <Table.Td
                        style={{
                          maxWidth: 300,
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                          whiteSpace: "nowrap",
                        }}
                      >
                        {m.description}
                      </Table.Td>
                      <Table.Td>
                        {new Date(m.createdAt).toLocaleDateString()}
                      </Table.Td>
                      <Table.Td>
                        <Group gap={4}>
                          <ActionIcon
                            variant="subtle"
                            onClick={() => openEdit(m.id)}
                            aria-label="Edit"
                          >
                            <Pencil size={16} />
                          </ActionIcon>
                          <ActionIcon
                            variant="subtle"
                            color="red"
                            onClick={() => setDeleteTarget(m)}
                            aria-label="Delete"
                          >
                            <Trash2 size={16} />
                          </ActionIcon>
                        </Group>
                      </Table.Td>
                    </Table.Tr>
                  ))}
              {!loading && mentors.length === 0 && (
                <Table.Tr>
                  <Table.Td colSpan={4}>
                    <Text ta="center" c="dimmed" py="lg">
                      {t("mentor.empty")}
                    </Text>
                  </Table.Td>
                </Table.Tr>
              )}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      </Box>

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
            <Users size={22} color="var(--mantine-color-blue-filled)" />
            <Text fw={700} size="lg" style={{ letterSpacing: 0 }}>
              {t("mentor.help.title")}
            </Text>
          </Group>
        }
        size="lg"
      >
        <Stack gap="md">
          <Text>{t("mentor.help.description")}</Text>

          <Box>
            <Text fw={600} mb="xs">
              {t("mentor.help.usedForTitle")}
            </Text>
            <List spacing="xs">
              <List.Item>{t("mentor.help.usedForItems.specialize")}</List.Item>
              <List.Item>{t("mentor.help.usedForItems.guide")}</List.Item>
              <List.Item>{t("mentor.help.usedForItems.tools")}</List.Item>
            </List>
          </Box>

          <Divider />

          <Box>
            <Text fw={600} mb="xs">
              {t("mentor.help.howToTitle")}
            </Text>
            <List spacing="xs">
              <List.Item>{t("mentor.help.howToItems.create")}</List.Item>
              <List.Item>{t("mentor.help.howToItems.instruction")}</List.Item>
              <List.Item>{t("mentor.help.howToItems.tools")}</List.Item>
              <List.Item>{t("mentor.help.howToItems.chat")}</List.Item>
            </List>
          </Box>

          <Alert color="blue" icon={<MessageSquare size={18} />}>
            {t("mentor.help.note")}
          </Alert>
        </Stack>
      </Modal>

      <Modal
        opened={formOpened}
        onClose={closeForm}
        title={editingMentor ? t("mentor.editTitle") : t("mentor.createTitle")}
        size="lg"
      >
        <MentorForm
          key={editingMentor?.id ?? "create"}
          initialData={editingMentor}
          onSubmit={handleSubmit}
          onCancel={closeForm}
          submitting={submitting}
        />
      </Modal>

      <Modal
        opened={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        title={t("mentor.deleteTitle")}
        size="sm"
      >
        <Text mb="md">
          {t("mentor.deleteConfirm", { name: deleteTarget?.name })}
        </Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={() => setDeleteTarget(null)}>
            {t("mentor.form.cancel")}
          </Button>
          <Button color="red" onClick={handleDelete} loading={submitting}>
            {t("mentor.deleteBtn")}
          </Button>
        </Group>
      </Modal>
    </Box>
  );
}
