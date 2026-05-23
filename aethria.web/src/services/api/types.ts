import { z } from "zod/v4";

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export const CreateApiKeyRequestSchema = z.object({
  name: z.string(),
  expirationDays: z.number().optional(),
});

export type CreateApiKeyRequest = z.infer<typeof CreateApiKeyRequestSchema>;

export interface CreateApiKeyResponse {
  id: string;
  name: string;
  token: string;
  expiresAt: string;
  createdAt: string;
}

export interface ApiKeyListItem {
  id: string;
  name: string;
  lastFourChars: string;
  status: string;
  createdAt: string;
  expiresAt: string;
}

export const GetPageApiKeysRequestSchema = z.object({
  pageNumber: z.number().optional(),
  pageSize: z.number().optional(),
});

export type GetPageApiKeysRequest = z.infer<typeof GetPageApiKeysRequestSchema>;

export type GetPageApiKeysResponse = PagedResponse<ApiKeyListItem>;

export const LoginRequestSchema = z.object({
  email: z.email(),
  password: z.string().min(1),
});

export type LoginRequest = z.infer<typeof LoginRequestSchema>;

export const RegisterRequestSchema = z.object({
  email: z.email(),
  password: z
    .string()
    .min(8)
    .regex(/[0-9]/)
    .regex(/[a-z]/)
    .regex(/[A-Z]/)
    .regex(/[$&+,:;=?@#|'<>.^*()%!-]/),
});

export type RegisterRequest = z.infer<typeof RegisterRequestSchema>;

export const GoogleRequestSchema = z.object({
  idToken: z.string(),
});

export type GoogleRequest = z.infer<typeof GoogleRequestSchema>;

export const ChangePasswordRequestSchema = z.object({
  currentPassword: z.string(),
  newPassword: z.string(),
});

export type ChangePasswordRequest = z.infer<typeof ChangePasswordRequestSchema>;

export interface AuthResponse {
  userId: string;
  email: string;
  accessToken: string;
  accessTokenExpiresAt: string;
}

export interface AuthUser {
  userId: string;
  email: string;
}

export const ChatRequestSchema = z.object({
  message: z.string(),
  sessionId: z.string().optional(),
  tools: z.array(z.string()).optional(),
});

export type ChatRequest = z.infer<typeof ChatRequestSchema>;

export const MentorChatRequestSchema = z.object({
  message: z.string(),
  sessionId: z.string().optional(),
});

export type MentorChatRequest = z.infer<typeof MentorChatRequestSchema>;

export const ResourceChatRequestSchema = z.object({
  message: z.string(),
  sessionId: z.string().optional(),
});

export type ResourceChatRequest = z.infer<typeof ResourceChatRequestSchema>;

export interface AgentTool {
  id: string;
  name: string;
  description: string;
}

export interface ChatStreamResponse {
  status: "started" | "delta" | "completed" | "failed";
  delta?: string;
  answer?: string;
  sessionId?: string;
  message?: string;
}

export interface ChatSessionItemResponse {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export type ChatSessionsPageResponse = PagedResponse<ChatSessionItemResponse>;

export interface ChatMessageItemResponse {
  id: string;
  role: string;
  content: string;
  createdAt: string;
}

export interface ChatHistoryResponse {
  messages: ChatMessageItemResponse[];
}

export interface ChatSession {
  id: string;
  title: string;
  description: string;
  createdAt: string;
  updatedAt: string;
}

export interface ChatMessage {
  id: string;
  role: "user" | "assistant";
  content: string;
  createdAt: string;
  isError?: boolean;
}

export interface MentorListItemResponse {
  id: string;
  name: string;
  description: string;
  createdAt: string;
  updatedAt: string;
}

export type GetPageMentorsResponse = PagedResponse<MentorListItemResponse>;

export interface GetMentorByIdResponse {
  id: string;
  userId: string;
  name: string;
  description: string;
  instruction: string;
  tools: string[];
  createdAt: string;
  updatedAt: string;
}

export type MentorDetailResponse = GetMentorByIdResponse;

export const MentorToolSchema = z.enum(["web_search", "web_extract"]);

export const CreateMentorRequestSchema = z.object({
  name: z.string().trim().min(1).max(100),
  description: z.string().trim().min(1).max(1000),
  instruction: z.string().trim().min(1).max(4000),
  tools: z.array(z.string()),
});

export type CreateMentorRequest = z.infer<typeof CreateMentorRequestSchema>;

export const UpdateMentorRequestSchema = CreateMentorRequestSchema;

export type UpdateMentorRequest = z.infer<typeof UpdateMentorRequestSchema>;

export interface QuizPageItem {
  id: string;
  name: string;
  description?: string;
  resourceId?: string;
  questionCount: number;
  createdAt: string;
  updatedAt: string;
}

export type GetPageQuizzesResponse = PagedResponse<QuizPageItem>;

export interface GetQuizByIdResponse {
  id: string;
  name: string;
  description?: string;
  resourceId?: string;
  resourceName?: string;
  questionCount: number;
  currentVersionNumber: number;
  createdAt: string;
  updatedAt: string;
}

export interface QuestionOptionItem {
  id: string;
  text: string;
  orderIndex: number;
}

export interface QuizQuestionItem {
  id: string;
  text: string;
  orderIndex: number;
  options: QuestionOptionItem[];
}

export interface GetQuizQuestionsResponse {
  quizVersionId?: string;
  questions: QuizQuestionItem[];
}

export const SubmitAnswerModelSchema = z.object({
  questionSnapshotId: z.string(),
  selectedOptionId: z.string(),
});

export type SubmitAnswerModel = z.infer<typeof SubmitAnswerModelSchema>;

export const SubmitQuizRequestSchema = z.object({
  quizVersionId: z.string(),
  answers: z.array(SubmitAnswerModelSchema),
});

export type SubmitQuizRequest = z.infer<typeof SubmitQuizRequestSchema>;

export interface AnswerResultItem {
  questionSnapshotId: string;
  selectedOptionId: string;
  correctOptionId: string;
  isCorrect: boolean;
  explanation: string;
}

export interface SubmitQuizAnswersResponse {
  submissionId: string;
  score: number;
  totalQuestions: number;
  isPassed: boolean;
  answerResults: AnswerResultItem[];
}

export interface QuizQuestionForEditItem {
  id: string;
  text: string;
  explanation: string;
  orderIndex: number;
  correctOptionIndex: number;
  options: QuestionOptionItem[];
}

export interface GetQuizQuestionsForEditResponse {
  quizVersionId?: string;
  questions: QuizQuestionForEditItem[];
}

export interface SubmissionOptionItem {
  id: string;
  text: string;
  orderIndex: number;
}

export interface SubmissionQuestionItem {
  questionSnapshotId: string;
  text: string;
  explanation: string;
  orderIndex: number;
  selectedOptionId: string;
  correctOptionId: string;
  isCorrect: boolean;
  options: SubmissionOptionItem[];
}

export interface GetSubmissionByIdResponse {
  submissionId: string;
  score: number;
  totalQuestions: number;
  isPassed: boolean;
  versionNumber: number;
  submittedAt: string;
  questions: SubmissionQuestionItem[];
}

export interface QuizSubmissionHistoryItem {
  submissionId: string;
  quizVersionId: string;
  versionNumber: number;
  score: number;
  totalQuestions: number;
  isPassed: boolean;
  submittedAt: string;
}

export type GetQuizSubmissionHistoryResponse = PagedResponse<QuizSubmissionHistoryItem>;

export const CreateBlankQuizRequestSchema = z.object({
  name: z.string(),
  description: z.string().optional(),
  resourceId: z.string().optional(),
});

export type CreateBlankQuizRequest = z.infer<
  typeof CreateBlankQuizRequestSchema
>;

export const CreateAIQuizRequestSchema = z.object({
  name: z.string(),
  description: z.string().optional(),
  resourceId: z.string(),
  prompt: z.string().optional(),
  numberOfQuestions: z.number(),
});

export type CreateAIQuizRequest = z.infer<typeof CreateAIQuizRequestSchema>;

export type CreateAIQuizStreamStatus =
  | "Started"
  | "GeneratingQuestions"
  | "Completed"
  | "Failed";

export interface CreateAIQuizStreamEvent {
  status: CreateAIQuizStreamStatus;
  message: string;
  quizId?: string;
}

export const UpdateOptionModelSchema = z.object({
  text: z.string(),
  orderIndex: z.number(),
});

export type UpdateOptionModel = z.infer<typeof UpdateOptionModelSchema>;

export const UpdateQuestionModelSchema = z.object({
  text: z.string(),
  explanation: z.string(),
  orderIndex: z.number(),
  options: z.array(UpdateOptionModelSchema),
  correctOptionIndex: z.number(),
});

export type UpdateQuestionModel = z.infer<typeof UpdateQuestionModelSchema>;

export const UpdateQuizRequestSchema = z.object({
  name: z.string().optional(),
  description: z.string().optional(),
  questions: z.array(UpdateQuestionModelSchema).optional(),
});

export type UpdateQuizRequest = z.infer<typeof UpdateQuizRequestSchema>;

export interface ResourceListItem {
  id: string;
  name: string;
  description?: string;
  fileType: string;
  fileSize: number;
  createdAt: string;
  updatedAt: string;
}

export interface ResourcePageItemResponse {
  id: string;
  name: string;
  description?: string;
  fileType: string;
  fileSize: number;
  createdAt: string;
  updatedAt: string;
}

export type GetPageResourcesResponse = PagedResponse<ResourcePageItemResponse>;

export interface GetResourceByIdResponse {
  id: string;
  name: string;
  description?: string;
  fileName: string;
  downloadUrl: string;
  createdAt: string;
  updatedAt: string;
}

export interface ResourceSelectorItemResponse {
  id: string;
  name: string;
}

export interface GetResourceSelectorResponse {
  resources: ResourceSelectorItemResponse[];
}

export const UpdateResourceRequestSchema = z.object({
  name: z.string(),
  description: z.string().optional(),
});

export type UpdateResourceRequest = z.infer<typeof UpdateResourceRequestSchema>;

export interface RoadmapListItemResponse {
  id: string;
  name: string;
  description?: string;
  resourceId: string;
  createdAt: string;
}

export type GetPageRoadmapsResponse = PagedResponse<RoadmapListItemResponse>;

export interface GetRoadmapByIdResponse {
  id: string;
  name: string;
  description?: string;
  content: string;
  mermaid?: string;
  resourceId: string;
  createdAt: string;
}

export const GenerateAIRoadmapRequestSchema = z.object({
  name: z.string(),
  description: z.string().optional(),
  resourceId: z.string(),
  prompt: z.string().optional(),
});

export type GenerateAIRoadmapRequest = z.infer<
  typeof GenerateAIRoadmapRequestSchema
>;

export type GenerateAIRoadmapStreamStatus =
  | "Started"
  | "GeneratingRoadmap"
  | "Completed"
  | "Failed";

export interface GenerateAIRoadmapStreamEvent {
  status: GenerateAIRoadmapStreamStatus;
  message: string;
  roadmapId?: string;
}

export interface NotificationPageItemResponse {
  id: string;
  type: string;
  data: Record<string, string>;
  isRead: boolean;
  createdAt: string;
  updatedAt: string;
}

export type GetPageNotificationsResponse = PagedResponse<NotificationPageItemResponse>;
