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
  Tabs,
  Stack,
  Card,
  Center,
  Loader,
} from "@mantine/core";
import { Plus, Trash2, Eye, Map, FileText } from "lucide-react";
import { useRoadmapPage } from "./useRoadmapPage";
import RoadmapForm from "./RoadmapForm";
import { Mermaid } from "../../components/Mermaid";
import { marked } from "marked";

export default function RoadmapPage() {
  const {
    t,
    roadmaps,
    totalPages,
    page,
    setPage,
    loading,
    resourcesOptions,
    formOpened,
    deleteTarget,
    setDeleteTarget,
    submitting,
    aiGenerating,
    aiStatus,
    aiMessageIndex,
    stopAiGeneration,
    openCreate,
    closeForm,
    handleSubmit,
    handleDelete,
    // Detail
    viewingRoadmapId,
    viewingRoadmap,
    viewingRoadmapLoading,
    openViewDetail,
    closeViewDetail,
  } = useRoadmapPage();

  // Render markdown content safely
  const renderMarkdown = (content: string) => {
    try {
      const html = marked.parse(content) as string;
      return (
        <div
          className="markdown-content"
          dangerouslySetInnerHTML={{ __html: html }}
        />
      );
    } catch (e) {
      console.error(e);
      return <Text>{content}</Text>;
    }
  };

  return (
    <Box p="md" style={{ overflow: "auto", flex: 1 }}>
      <Group justify="space-between" mb="md">
        <Title order={3}>{t("roadmap.title")}</Title>
        <Button leftSection={<Plus size={16} />} onClick={openCreate}>
          {t("roadmap.create")}
        </Button>
      </Group>

      <Box pos="relative" mih={200}>
        <LoadingOverlay
          visible={loading && roadmaps.length > 0}
          overlayProps={{ radius: "sm", blur: 1.5 }}
        />
        <Table.ScrollContainer minWidth={700}>
          <Table striped highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t("roadmap.table.name")}</Table.Th>
                <Table.Th>{t("roadmap.table.description")}</Table.Th>
                <Table.Th>{t("roadmap.table.createdAt")}</Table.Th>
                <Table.Th w={100}>{t("roadmap.table.actions")}</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {loading && roadmaps.length === 0
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
                          <Skeleton height={20} radius="sm" width="40%" />
                        </Table.Td>
                        <Table.Td>
                          <Group gap={4}>
                            <Skeleton height={28} width={28} radius="sm" />
                            <Skeleton height={28} width={28} radius="sm" />
                          </Group>
                        </Table.Td>
                      </Table.Tr>
                    ))
                : roadmaps.map((r) => (
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
                        {r.description || (
                          <Text span c="dimmed" fs="italic">
                            Không có mô tả
                          </Text>
                        )}
                      </Table.Td>
                      <Table.Td>
                        {new Date(r.createdAt).toLocaleDateString()}
                      </Table.Td>
                      <Table.Td>
                        <Group gap={4}>
                          <ActionIcon
                            variant="subtle"
                            color="blue"
                            onClick={() => openViewDetail(r.id)}
                            title={t("roadmap.detailTitle")}
                            aria-label="View Details"
                          >
                            <Eye size={16} />
                          </ActionIcon>
                          <ActionIcon
                            variant="subtle"
                            color="red"
                            onClick={() => setDeleteTarget(r)}
                            title={t("roadmap.deleteTitle")}
                            aria-label="Delete"
                          >
                            <Trash2 size={16} />
                          </ActionIcon>
                        </Group>
                      </Table.Td>
                    </Table.Tr>
                  ))}
              {!loading && roadmaps.length === 0 && (
                <Table.Tr>
                  <Table.Td colSpan={4}>
                    <Text ta="center" c="dimmed" py="lg">
                      {t("roadmap.empty")}
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

      {/* AI Generate Modal */}
      <Modal
        opened={formOpened}
        onClose={closeForm}
        title={t("roadmap.createTitle")}
        size="lg"
        closeOnClickOutside={!aiGenerating}
        closeOnEscape={!aiGenerating}
        withCloseButton={!aiGenerating}
      >
        <RoadmapForm
          resourcesOptions={resourcesOptions}
          onSubmit={handleSubmit}
          onCancel={closeForm}
          submitting={submitting}
          aiGenerating={aiGenerating}
          aiStatus={aiStatus}
          aiMessageIndex={aiMessageIndex}
          stopAiGeneration={stopAiGeneration}
        />
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal
        opened={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        title={t("roadmap.deleteTitle")}
        size="sm"
      >
        <Text mb="md">
          {t("roadmap.deleteConfirm", { name: deleteTarget?.name })}
        </Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={() => setDeleteTarget(null)}>
            {t("roadmap.form.cancel")}
          </Button>
          <Button color="red" onClick={handleDelete} loading={submitting}>
            {t("roadmap.deleteBtn")}
          </Button>
        </Group>
      </Modal>

      {/* Roadmap Detail Modal */}
      <Modal
        opened={!!viewingRoadmapId}
        onClose={closeViewDetail}
        title={
          viewingRoadmap
            ? t("roadmap.detail.title", { name: viewingRoadmap.name })
            : t("roadmap.detailTitle")
        }
        size="90%"
        yOffset="3vh"
      >
        <Box pos="relative" style={{ minHeight: 400 }}>
          <LoadingOverlay
            visible={viewingRoadmapLoading && !viewingRoadmap}
            overlayProps={{ radius: "sm", blur: 1.5 }}
          />
          {viewingRoadmapLoading && !viewingRoadmap ? (
            <Center py="xl" style={{ height: 300 }}>
              <Loader size="md" />
            </Center>
          ) : viewingRoadmap ? (
            <Stack gap="md">
              {viewingRoadmap.description && (
                <Card withBorder p="md" radius="md">
                  <Text size="sm" c="dimmed">
                    {viewingRoadmap.description}
                  </Text>
                </Card>
              )}

              <Tabs defaultValue="content">
                <Tabs.List mb="md">
                  <Tabs.Tab
                    value="content"
                    leftSection={<FileText size={14} />}
                  >
                    {t("roadmap.detail.markdownTab")}
                  </Tabs.Tab>
                  <Tabs.Tab value="flowchart" leftSection={<Map size={14} />}>
                    {t("roadmap.detail.mermaidTab")}
                  </Tabs.Tab>
                </Tabs.List>

                <Tabs.Panel value="content">
                  <Box
                    p="xs"
                    style={{
                      border: "1px solid var(--mantine-color-default-border)",
                      borderRadius: 8,
                      padding: "1rem",
                      minHeight: 200,
                    }}
                  >
                    {renderMarkdown(viewingRoadmap.content)}
                  </Box>
                </Tabs.Panel>

                <Tabs.Panel value="flowchart">
                  {viewingRoadmap.mermaid ? (
                    <Mermaid chart={viewingRoadmap.mermaid} />
                  ) : (
                    <Center py="xl">
                      <Text c="dimmed">
                        {t(
                          "roadmap.emptyFlowchart",
                          "Không có biểu đồ trực quan cho lộ trình này.",
                        )}
                      </Text>
                    </Center>
                  )}
                </Tabs.Panel>
              </Tabs>

              <Group justify="flex-end" mt="md">
                <Button onClick={closeViewDetail}>
                  {t("roadmap.detail.close")}
                </Button>
              </Group>
            </Stack>
          ) : null}
        </Box>
      </Modal>
    </Box>
  );
}
