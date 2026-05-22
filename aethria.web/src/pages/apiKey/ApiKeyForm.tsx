import { useState } from "react";
import {
  Button,
  Group,
  Stack,
  TextInput,
  Select,
  NumberInput,
} from "@mantine/core";
import { useApiKeyForm, type ApiKeyFormValues } from "./useApiKeyForm";

interface ApiKeyFormProps {
  onSubmit: (data: ApiKeyFormValues) => void;
  onCancel: () => void;
  submitting: boolean;
}

export default function ApiKeyForm({
  onSubmit,
  onCancel,
  submitting,
}: ApiKeyFormProps) {
  const { t, form, handleSubmit } = useApiKeyForm(onSubmit);
  const [expiryType, setExpiryType] = useState<string>("30");

  return (
    <form onSubmit={handleSubmit} noValidate>
      <Stack gap="sm">
        <TextInput
          key={form.key("name")}
          label={t("apiKey.form.name")}
          placeholder={t("apiKey.form.namePlaceholder")}
          required
          maxLength={100}
          {...form.getInputProps("name")}
        />

        <Select
          label={t("apiKey.form.expiration")}
          value={expiryType}
          onChange={(val) => {
            if (val) {
              setExpiryType(val);
              if (val !== "custom") {
                form.setFieldValue("expirationDays", parseInt(val, 10));
              }
            }
          }}
          data={[
            { value: "30", label: t("apiKey.form.expiration30") },
            { value: "60", label: t("apiKey.form.expiration60") },
            { value: "90", label: t("apiKey.form.expiration90") },
            { value: "365", label: t("apiKey.form.expiration365") },
            { value: "custom", label: t("apiKey.form.expirationCustom") },
          ]}
        />

        {expiryType === "custom" && (
          <NumberInput
            key={form.key("expirationDays")}
            label={t("apiKey.form.expirationDays")}
            required
            min={1}
            max={365}
            {...form.getInputProps("expirationDays")}
          />
        )}

        <Group justify="flex-end" mt="md">
          <Button variant="default" onClick={onCancel}>
            {t("apiKey.form.cancel")}
          </Button>
          <Button type="submit" loading={submitting}>
            {t("apiKey.form.save")}
          </Button>
        </Group>
      </Stack>
    </form>
  );
}
