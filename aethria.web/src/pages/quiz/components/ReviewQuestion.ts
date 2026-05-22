export interface ReviewQuestion {
  questionSnapshotId: string;
  text: string;
  explanation: string;
  selectedOptionId: string;
  correctOptionId: string;
  isCorrect: boolean;
  options: { id: string; text: string }[];
}
