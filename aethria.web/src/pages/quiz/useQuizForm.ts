import { useForm } from "@mantine/form";
import { useTranslation } from "react-i18next";
import type {
  GetQuizByIdResponse,
  QuizQuestionForEditItem,
} from "../../services";

export interface QuizFormValues {
  mode: "manual" | "ai";
  name: string;
  description: string;
  resourceId: string;
  prompt: string;
  numberOfQuestions?: number;
  questions: {
    key: string;
    id?: string;
    text: string;
    explanation: string;
    orderIndex: number;
    correctOptionIndex: number;
    options: {
      key: string;
      id?: string;
      text: string;
      orderIndex: number;
    }[];
  }[];
}

const initialValues: QuizFormValues = {
  mode: "manual",
  name: "",
  description: "",
  resourceId: "",
  prompt: "",
  numberOfQuestions: undefined,
  questions: [],
};

export interface EditingQuizData extends GetQuizByIdResponse {
  questions: QuizQuestionForEditItem[];
}

export function useQuizForm(
  initialData: EditingQuizData | null,
  onSubmit: (data: QuizFormValues) => void,
) {
  const { t } = useTranslation();

  // Helper to construct UI values from API data when editing
  const getInitialValues = (): QuizFormValues => {
    if (!initialData) return initialValues;

    return {
      mode: "manual",
      name: initialData.name,
      description: initialData.description ?? "",
      resourceId: initialData.resourceId ?? "",
      prompt: "",
      numberOfQuestions: undefined,
      questions: (initialData.questions ?? []).map((q) => ({
        key: `q-${q.id || Math.random().toString(36).substr(2, 9)}`,
        id: q.id,
        text: q.text,
        explanation: q.explanation,
        orderIndex: q.orderIndex,
        correctOptionIndex: q.correctOptionIndex,
        options: (q.options ?? []).map((opt) => ({
          key: `opt-${opt.id || Math.random().toString(36).substr(2, 9)}`,
          id: opt.id,
          text: opt.text,
          orderIndex: opt.orderIndex,
        })),
      })),
    };
  };

  const form = useForm<QuizFormValues>({
    mode: "uncontrolled",
    initialValues: getInitialValues(),
    validate: {
      name: (value) => {
        if (!value || value.trim().length === 0) {
          return t("quiz.validation.nameRequired");
        }
        if (value.length > 100) {
          return t("quiz.validation.nameTooLong");
        }
        return null;
      },
      resourceId: (value, values) => {
        if (values.mode === "ai" && (!value || value.trim().length === 0)) {
          return t("quiz.validation.resourceRequired");
        }
        return null;
      },
      numberOfQuestions: (value, values) => {
        if (values.mode === "ai") {
          if (value === undefined || value === null) {
            return t("quiz.validation.questionsCountRequired");
          }
          if (value < 1 || value > 50) {
            return t("quiz.validation.questionsCountRange");
          }
        }
        return null;
      },
      questions: {
        text: (value) =>
          !value || value.trim().length === 0
            ? "Question text is required"
            : null,
        explanation: (value) =>
          !value || value.trim().length === 0
            ? "Explanation is required"
            : null,
        options: {
          text: (value) =>
            !value || value.trim().length === 0
              ? "Option text is required"
              : null,
        },
      },
    },
  });

  const addQuestion = () => {
    const questions = form.getValues().questions;
    form.insertListItem("questions", {
      key: `q-${Math.random().toString(36).substr(2, 9)}`,
      text: "",
      explanation: "",
      orderIndex: questions.length,
      correctOptionIndex: 0,
      options: [
        {
          key: `opt-${Math.random().toString(36).substr(2, 9)}`,
          text: "",
          orderIndex: 0,
        },
        {
          key: `opt-${Math.random().toString(36).substr(2, 9)}`,
          text: "",
          orderIndex: 1,
        },
      ],
    });
  };

  const removeQuestion = (index: number) => {
    form.removeListItem("questions", index);
  };

  const addOption = (questionIndex: number) => {
    const options = form.getValues().questions[questionIndex].options;
    form.insertListItem(`questions.${questionIndex}.options`, {
      key: `opt-${Math.random().toString(36).substr(2, 9)}`,
      text: "",
      orderIndex: options.length,
    });
  };

  const removeOption = (questionIndex: number, optionIndex: number) => {
    form.removeListItem(`questions.${questionIndex}.options`, optionIndex);
  };

  const setCorrectOption = (questionIndex: number, optionIndex: number) => {
    form.setFieldValue(
      `questions.${questionIndex}.correctOptionIndex`,
      optionIndex,
    );
  };

  const handleSubmit = form.onSubmit((values) => {
    onSubmit(values);
  });

  return {
    t,
    form,
    addQuestion,
    removeQuestion,
    addOption,
    removeOption,
    setCorrectOption,
    handleSubmit,
  };
}
