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
  Alert,
  TextInput,
  CopyButton,
  Stack,
  Divider,
  List,
  Code,
} from "@mantine/core";
import { useState } from "react";
import { Plus, Ban, AlertTriangle, KeyRound, HelpCircle } from "lucide-react";
import { useApiKeyPage } from "./useApiKeyPage";
import ApiKeyForm from "./ApiKeyForm";

function getStatusBadge(status: string) {
  const s = status.toLowerCase();
  if (s === "active") return <Badge color="green" variant="light">Active</Badge>;
  if (s === "revoked") return <Badge color="red" variant="light">Revoked</Badge>;
  if (s === "expired") return <Badge color="yellow" variant="light">Expired</Badge>;
  return <Badge color="gray" variant="light">{status}</Badge>;
}

export default function ApiKeyPage() {
  const [helpOpened, setHelpOpened] = useState(false);
  const {
    t,
    apiKeys,
    totalPages,
    page,
    setPage,
    loading,
    formOpened,
    createdApiKey,
    setCreatedApiKey,
    deleteTarget,
    setDeleteTarget,
    submitting,
    openCreate,
    closeForm,
    handleSubmit,
    handleRevoke,
  } = useApiKeyPage();

  return (
    <Box p="md" style={{ overflow: "auto", flex: 1 }}>
      <Group justify="space-between" mb="md">
        <Title order={3}>{t("apiKey.title")}</Title>
        <Group gap="xs">
          <Button
            variant="light"
            leftSection={<HelpCircle size={16} />}
            onClick={() => setHelpOpened(true)}
          >
            {t("apiKey.help.button")}
          </Button>
          <Button leftSection={<Plus size={16} />} onClick={openCreate}>
            {t("apiKey.create")}
          </Button>
        </Group>
      </Group>

      <Box pos="relative" mih={200}>
        <LoadingOverlay
          visible={loading && apiKeys.length > 0}
          overlayProps={{ radius: "sm", blur: 1.5 }}
        />
        <Table.ScrollContainer minWidth={800}>
          <Table striped highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t("apiKey.table.name")}</Table.Th>
                <Table.Th w={200}>{t("apiKey.table.lastFourChars")}</Table.Th>
                <Table.Th w={120}>{t("apiKey.table.status")}</Table.Th>
                <Table.Th w={150}>{t("apiKey.table.createdAt")}</Table.Th>
                <Table.Th w={150}>{t("apiKey.table.expiresAt")}</Table.Th>
                <Table.Th w={100}>{t("apiKey.table.actions")}</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {loading && apiKeys.length === 0
                ? Array(5)
                    .fill(0)
                    .map((_, index) => (
                      <Table.Tr key={`skeleton-${index}`}>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="60%" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="40%" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="50px" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="60%" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={20} radius="sm" width="60%" />
                        </Table.Td>
                        <Table.Td>
                          <Skeleton height={28} width={28} radius="sm" />
                        </Table.Td>
                      </Table.Tr>
                    ))
                : apiKeys.map((keyItem) => {
                    const isRevocable = keyItem.status.toLowerCase() === "active";
                    return (
                      <Table.Tr key={keyItem.id}>
                        <Table.Td style={{ fontWeight: 500 }}>{keyItem.name}</Table.Td>
                        <Table.Td style={{ fontFamily: "monospace", letterSpacing: "1px" }}>
                          ••••••••••••{keyItem.lastFourChars}
                        </Table.Td>
                        <Table.Td>{getStatusBadge(keyItem.status)}</Table.Td>
                        <Table.Td>{new Date(keyItem.createdAt).toLocaleDateString()}</Table.Td>
                        <Table.Td>
                          {keyItem.expiresAt
                            ? new Date(keyItem.expiresAt).toLocaleDateString()
                            : "-"}
                        </Table.Td>
                        <Table.Td>
                          <ActionIcon
                            variant="subtle"
                            color="red"
                            disabled={!isRevocable}
                            onClick={() => setDeleteTarget(keyItem)}
                            aria-label={t("apiKey.deleteTitle")}
                          >
                            <Ban size={16} />
                          </ActionIcon>
                        </Table.Td>
                      </Table.Tr>
                    );
                  })}
              {!loading && apiKeys.length === 0 && (
                <Table.Tr>
                  <Table.Td colSpan={6}>
                    <Text ta="center" c="dimmed" py="lg">
                      {t("apiKey.empty")}
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
            <KeyRound size={22} color="var(--mantine-color-blue-filled)" />
            <Text fw={700} size="lg" style={{ letterSpacing: 0 }}>
              {t("apiKey.help.title")}
            </Text>
          </Group>
        }
        size="lg"
      >
        <Stack gap="md">
          <Text>{t("apiKey.help.description")}</Text>

          <Box>
            <Text fw={600} mb="xs">
              {t("apiKey.help.usedForTitle")}
            </Text>
            <List spacing="xs">
              <List.Item>{t("apiKey.help.usedForItems.manage")}</List.Item>
              <List.Item>{t("apiKey.help.usedForItems.chat")}</List.Item>
              <List.Item>{t("apiKey.help.usedForItems.secure")}</List.Item>
            </List>
          </Box>

          <Divider />

          <Box>
            <Text fw={600} mb="xs">
              {t("apiKey.help.connectTitle")}
            </Text>
            <List spacing="xs">
              <List.Item>{t("apiKey.help.connectItems.create")}</List.Item>
              <List.Item>{t("apiKey.help.connectItems.url")}</List.Item>
              <List.Item>
                {t("apiKey.help.connectItems.header")}{" "}
                <Code>X-Api-Key: {"<your-api-key>"}</Code>
              </List.Item>
              <List.Item>{t("apiKey.help.connectItems.tools")}</List.Item>
            </List>
          </Box>

          <Alert color="blue" icon={<KeyRound size={18} />}>
            {t("apiKey.help.note")}
          </Alert>
        </Stack>
      </Modal>

      {/* Creation Modal */}
      <Modal
        opened={formOpened}
        onClose={closeForm}
        title={t("apiKey.createTitle")}
        size="lg"
      >
        <ApiKeyForm
          onSubmit={handleSubmit}
          onCancel={closeForm}
          submitting={submitting}
        />
      </Modal>

      {/* Success Modal showing newly created API Key (Token) */}
      <Modal
        opened={!!createdApiKey}
        onClose={() => setCreatedApiKey(null)}
        title={t("apiKey.successTitle")}
        size="md"
        closeOnClickOutside={false}
        closeOnEscape={false}
        withCloseButton={false}
      >
        <Stack gap="md">
          <Alert
            color="orange"
            title={t("apiKey.successTitle")}
            icon={<AlertTriangle size={18} />}
          >
            {t("apiKey.successWarning")}
          </Alert>

          <Group align="flex-end" gap="xs">
            <TextInput
              label={t("apiKey.form.name")}
              value={createdApiKey?.token || ""}
              readOnly
              style={{ flex: 1 }}
              styles={{ input: { fontFamily: "monospace" } }}
            />
            <CopyButton value={createdApiKey?.token || ""}>
              {({ copied, copy }) => (
                <Button
                  color={copied ? "teal" : "blue"}
                  onClick={copy}
                  leftSection={<KeyRound size={14} />}
                >
                  {copied ? t("apiKey.copied") : t("apiKey.copyBtn")}
                </Button>
              )}
            </CopyButton>
          </Group>

          <Group justify="flex-end" mt="xs">
            <Button onClick={() => setCreatedApiKey(null)}>
              {t("apiKey.closeBtn")}
            </Button>
          </Group>
        </Stack>
      </Modal>

      {/* Revocation Confirmation Modal */}
      <Modal
        opened={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        title={t("apiKey.deleteTitle")}
        size="sm"
      >
        <Text mb="md">
          {t("apiKey.deleteConfirm", { name: deleteTarget?.name })}
        </Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={() => setDeleteTarget(null)}>
            {t("apiKey.form.cancel")}
          </Button>
          <Button color="red" onClick={handleRevoke} loading={submitting}>
            {t("apiKey.deleteBtn")}
          </Button>
        </Group>
      </Modal>
    </Box>
  );
}
