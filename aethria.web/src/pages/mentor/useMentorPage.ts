import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import {
  getPageMentors,
  getMentorById,
  createMentor,
  updateMentor,
  deleteMentor,
} from "../../services";
import type {
  MentorListItemResponse,
  GetMentorByIdResponse,
  CreateMentorRequest,
  UpdateMentorRequest,
} from "../../services";
import type { MentorFormValues } from "./useMentorForm";

const mentorKeys = {
  all: ["mentors"] as const,
  pages: () => [...mentorKeys.all, "page"] as const,
  page: (page: number, pageSize: number) =>
    [...mentorKeys.pages(), page, pageSize] as const,
  detail: (id: string) => [...mentorKeys.all, "detail", id] as const,
};

export function useMentorPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [formOpened, setFormOpened] = useState(false);
  const [editingMentor, setEditingMentor] =
    useState<GetMentorByIdResponse | null>(null);
  const [deleteTarget, setDeleteTarget] =
    useState<MentorListItemResponse | null>(null);
  const pageSize = 10;

  const mentorsQuery = useQuery({
    queryKey: mentorKeys.page(page, pageSize),
    queryFn: ({ signal }) => getPageMentors(page, pageSize, signal),
  });

  const saveMentorMutation = useMutation({
    mutationFn: async (data: MentorFormValues) => {
      if (editingMentor) {
        const updateRequest: UpdateMentorRequest = {
          name: data.name,
          description: data.description,
          instruction: data.instruction,
          tools: data.tools,
        };
        await updateMentor(editingMentor.id, updateRequest);
        return { isEdit: true };
      } else {
        const createRequest: CreateMentorRequest = {
          name: data.name,
          description: data.description,
          instruction: data.instruction,
          tools: data.tools,
        };
        await createMentor(createRequest);
        return { isEdit: false };
      }
    },
    onSuccess: async (data) => {
      await queryClient.invalidateQueries({ queryKey: mentorKeys.pages() });
      notifications.show({
        title: t("notifications.success"),
        message: data.isEdit
          ? t("notifications.mentor.updateSuccess")
          : t("notifications.mentor.createSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: editingMentor
          ? t("notifications.mentor.updateError")
          : t("notifications.mentor.createError"),
        color: "red",
      });
    },
  });

  const deleteMentorMutation = useMutation({
    mutationFn: (id: string) => deleteMentor(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: mentorKeys.pages() });
      notifications.show({
        title: t("notifications.success"),
        message: t("notifications.mentor.deleteSuccess"),
        color: "green",
      });
    },
    onError: () => {
      notifications.show({
        title: t("notifications.error"),
        message: t("notifications.mentor.deleteError"),
        color: "red",
      });
    },
  });

  const openCreate = () => {
    setEditingMentor(null);
    setFormOpened(true);
  };

  const openEdit = async (id: string) => {
    const mentor = await queryClient.fetchQuery({
      queryKey: mentorKeys.detail(id),
      queryFn: ({ signal }) => getMentorById(id, signal),
    });
    setEditingMentor(mentor);
    setFormOpened(true);
  };

  const closeForm = () => {
    setFormOpened(false);
    setEditingMentor(null);
  };

  const handleSubmit = async (data: MentorFormValues) => {
    await saveMentorMutation.mutateAsync(data);
    closeForm();
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteMentorMutation.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  const mentors = mentorsQuery.data?.items ?? [];
  const totalPages = mentorsQuery.data?.totalPages ?? 0;
  const submitting =
    saveMentorMutation.isPending || deleteMentorMutation.isPending;

  return {
    t,
    mentors,
    totalPages,
    page,
    setPage,
    pageSize,
    loading: mentorsQuery.isLoading,
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
  };
}
