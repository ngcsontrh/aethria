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
  Badge,
} from "@mantine/core";
import { Plus, Pencil, Trash2, Download } from "lucide-react";
import { useResourcePage } from "./useResourcePage";
import ResourceForm from "./ResourceForm";

function formatFileSize(bytes: number) {
  if (bytes <= 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
}

function getFileTypeColor(type: string) {
  const t = type.toLowerCase();
  if (t === "pdf") return "red";
  if (t === "doc" || t === "docx") return "blue";
  if (t === "xls" || t === "xlsx") return "green";
  if (t === "csv") return "teal";
  if (t === "txt" || t === "md") return "gray";
  return "violet";
}

export default function ResourcePage() {
  const {
    t,
    resources,
    totalPages,
    page,
    setPage,
    loading,
    formOpened,
    editingResource,
    deleteTarget,
    setDeleteTarget,
    downloadingId,
    submitting,
    uploadProcessing,
    uploadStatus,
    uploadMessageIndex,
    stopUpload,
    openCreate,
    openEdit,
    closeForm,
    handleSubmit,
    handleDelete,
    handleDownload,
  } = useResourcePage();

  return (
    <Box p="md" style={{ overflow: "auto", flex: 1 }}>
      <Group justify="space-between" mb="md">
        <Title order={3}>{t("resource.title")}</Title>
        <Button leftSection={<Plus size={16} />} onClick={openCreate}>
          {t("resource.create")}
        </Button>
      </Group>

      <Box pos="relative" mih={200}>
        <LoadingOverlay
          visible={loading && resources.length > 0}
          overlayProps={{ radius: "sm", blur: 1.5 }}
        />
        <Table.ScrollContainer minWidth={800}>
          <Table striped highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t("resource.table.name")}</Table.Th>
                <Table.Th>{t("resource.table.description")}</Table.Th>
                <Table.Th w={100}>{t("resource.table.fileType")}</Table.Th>
                <Table.Th w={100}>{t("resource.table.fileSize")}</Table.Th>
                <Table.Th>{t("resource.table.createdAt")}</Table.Th>
                <Table.Th w={130}>{t("resource.table.actions")}</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {loading && resources.length === 0
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
                          <Skeleton height={20} radius="sm" width="40px" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="50px" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="40%" />
                        </Table.Td>
                        <Table.Td>
                          <Group gap={8}>
                            <Skeleton height={28} width={28} radius="sm" />
                            <Skeleton height={28} width={28} radius="sm" />
                            <Skeleton height={28} width={28} radius="sm" />
                          </Group>
                        </Table.Td>
                      </Table.Tr>
                    ))
                : resources.map((r) => (
                    <Table.Tr key={r.id}>
                      <Table.Td style={{ fontWeight: 500 }}>{r.name}</Table.Td>
                      <Table.Td
                        style={{
                          maxWidth: 300,
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                          whiteSpace: "nowrap",
                        }}
                      >
                        {r.description || "-"}
                      </Table.Td>
                      <Table.Td>
                        <Badge
                          color={getFileTypeColor(r.fileType)}
                          variant="light"
                        >
                          {r.fileType.toUpperCase()}
                        </Badge>
                      </Table.Td>
                      <Table.Td style={{ whiteSpace: "nowrap" }}>
                        {formatFileSize(r.fileSize)}
                      </Table.Td>
                      <Table.Td>
                        {new Date(r.createdAt).toLocaleDateString()}
                      </Table.Td>
                      <Table.Td>
                        <Group gap={4}>
                          <ActionIcon
                            variant="subtle"
                            color="blue"
                            onClick={() =>
                              handleDownload(r.id, r.name + "." + r.fileType)
                            }
                            loading={downloadingId === r.id}
                            aria-label="Download"
                          >
                            <Download size={16} />
                          </ActionIcon>
                          <ActionIcon
                            variant="subtle"
                            onClick={() => openEdit(r.id)}
                            aria-label="Edit"
                            disabled={downloadingId === r.id}
                          >
                            <Pencil size={16} />
                          </ActionIcon>
                          <ActionIcon
                            variant="subtle"
                            color="red"
                            onClick={() => setDeleteTarget(r)}
                            aria-label="Delete"
                            disabled={downloadingId === r.id}
                          >
                            <Trash2 size={16} />
                          </ActionIcon>
                        </Group>
                      </Table.Td>
                    </Table.Tr>
                  ))}
              {!loading && resources.length === 0 && (
                <Table.Tr>
                  <Table.Td colSpan={6}>
                    <Text ta="center" c="dimmed" py="lg">
                      {t("resource.empty")}
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
        opened={formOpened}
        onClose={closeForm}
        title={
          editingResource ? t("resource.editTitle") : t("resource.createTitle")
        }
        size="lg"
        closeOnClickOutside={!uploadProcessing}
        closeOnEscape={!uploadProcessing}
        withCloseButton={!uploadProcessing}
      >
        <ResourceForm
          key={editingResource?.id ?? "create"}
          initialData={editingResource}
          onSubmit={handleSubmit}
          onCancel={closeForm}
          submitting={submitting}
          uploadProcessing={uploadProcessing}
          uploadStatus={uploadStatus}
          uploadMessageIndex={uploadMessageIndex}
          stopUpload={stopUpload}
        />
      </Modal>

      <Modal
        opened={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        title={t("resource.deleteTitle")}
        size="sm"
      >
        <Text mb="md">
          {t("resource.deleteConfirm", { name: deleteTarget?.name })}
        </Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={() => setDeleteTarget(null)}>
            {t("resource.form.cancel")}
          </Button>
          <Button color="red" onClick={handleDelete} loading={submitting}>
            {t("resource.deleteBtn")}
          </Button>
        </Group>
      </Modal>
    </Box>
  );
}
