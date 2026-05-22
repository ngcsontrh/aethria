import { useState, useEffect, useRef } from "react";
import axios from "axios";
import {
  getQuizSubmissionHistory,
  getQuizSubmissionById,
} from "../../../services";
import type {
  QuizPageItem,
  QuizSubmissionHistoryItem,
  GetSubmissionByIdResponse,
} from "../../../services";

export function useQuizHistory(quiz: QuizPageItem | null) {
  const [loading, setLoading] = useState(false);
  const [submissions, setSubmissions] = useState<QuizSubmissionHistoryItem[]>(
    [],
  );
  const [totalPages, setTotalPages] = useState(0);
  const [page, setPage] = useState(1);
  const pageSize = 5;

  const [selectedSubmission, setSelectedSubmission] =
    useState<GetSubmissionByIdResponse | null>(null);
  const [selectedSubmissionLoading, setSelectedSubmissionLoading] =
    useState(false);

  const historyAbortControllerRef = useRef<AbortController | null>(null);
  const detailAbortControllerRef = useRef<AbortController | null>(null);

  const fetchHistory = async (pageNumber: number) => {
    if (!quiz) return;

    if (historyAbortControllerRef.current) {
      historyAbortControllerRef.current.abort();
    }
    const controller = new AbortController();
    historyAbortControllerRef.current = controller;

    setLoading(true);
    try {
      const response = await getQuizSubmissionHistory(
        quiz.id,
        pageNumber,
        pageSize,
        controller.signal,
      );
      setSubmissions(response.items);
      setTotalPages(response.totalPages);
      setPage(pageNumber);
    } catch (err) {
      if (axios.isCancel(err)) return;
      console.error(err);
    } finally {
      if (historyAbortControllerRef.current === controller) {
        historyAbortControllerRef.current = null;
        setLoading(false);
      }
    }
  };

  /* eslint-disable react-hooks/set-state-in-effect */
  useEffect(() => {
    if (!quiz) {
      setSubmissions([]);
      setTotalPages(0);
      setPage(1);
      setSelectedSubmission(null);
      return;
    }
    fetchHistory(1);
    
    return () => {
      historyAbortControllerRef.current?.abort();
      detailAbortControllerRef.current?.abort();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [quiz]);
  /* eslint-enable react-hooks/set-state-in-effect */

  const changePage = (newPage: number) => {
    fetchHistory(newPage);
  };

  const loadSubmissionDetail = async (submissionId: string) => {
    if (!quiz) return;

    if (detailAbortControllerRef.current) {
      detailAbortControllerRef.current.abort();
    }
    const controller = new AbortController();
    detailAbortControllerRef.current = controller;

    setSelectedSubmissionLoading(true);
    try {
      const response = await getQuizSubmissionById(
        quiz.id,
        submissionId,
        controller.signal,
      );
      setSelectedSubmission(response);
    } catch (err) {
      if (axios.isCancel(err)) return;
      console.error(err);
    } finally {
      if (detailAbortControllerRef.current === controller) {
        detailAbortControllerRef.current = null;
        setSelectedSubmissionLoading(false);
      }
    }
  };

  const closeSubmissionDetail = () => {
    if (detailAbortControllerRef.current) {
      detailAbortControllerRef.current.abort();
    }
    setSelectedSubmission(null);
  };

  return {
    loading,
    submissions,
    totalPages,
    page,
    pageSize,
    selectedSubmission,
    selectedSubmissionLoading,
    changePage,
    loadSubmissionDetail,
    closeSubmissionDetail,
  };
}
