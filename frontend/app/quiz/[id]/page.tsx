"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";

type QuizDetail = {
  id: number;
  title: string;
  timeLimitInMinutes: number;
  maxAttemptsPerUser: number;
  passingScorePercentage: number;
  isPublished: boolean;
  questionCount: number;
};

export default function QuizDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();

  const [quiz, setQuiz] = useState<QuizDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  useEffect(() => {
    //check login
    const token = localStorage.getItem("token");
    setIsLoggedIn(!!token);

    async function fetchQuiz() {
      try {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_BASE_URL}/api/quiz/${id}`
        );

        if (res.status === 404) {
          setError("Quiz not found.");
          return;
        }

        if (!res.ok) throw new Error();

        setQuiz(await res.json());
      } catch {
        setError("Failed to load quiz.");
      } finally {
        setLoading(false);
      }
    }

    fetchQuiz();
  }, [id]);

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        Loading quiz...
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-screen items-center justify-center text-red-600">
        {error}
      </div>
    );
  }

  if (!quiz) return null;

  return (
    <div className="min-h-screen bg-zinc-50 px-6 py-12 dark:bg-black">
      <div className="mx-auto max-w-3xl rounded-xl bg-white p-8 shadow dark:bg-zinc-900">
        <h1 className="mb-4 text-3xl font-semibold">
          {quiz.title}
        </h1>

        <ul className="mb-8 space-y-2 text-zinc-700 dark:text-zinc-300">
          <li>🧠 Questions: {quiz.questionCount}</li>
          <li>⏱ Time limit: {quiz.timeLimitInMinutes} min</li>
          <li>🔁 Max attempts: {quiz.maxAttemptsPerUser}</li>
          <li>🎯 Passing score: {quiz.passingScorePercentage}%</li>
        </ul>

        <div className="flex gap-4">
          {!isLoggedIn ? (
            <button
              onClick={() =>
                router.push(`/login?redirect=/quiz/${quiz.id}`)
              }
              className="rounded bg-black px-6 py-2 text-white dark:bg-white dark:text-black"
            >
              Login to start quiz
            </button>
          ) : (
            <button
              disabled={!quiz.isPublished || quiz.questionCount === 0}
              onClick={() => router.push(`/quiz/${quiz.id}/take`)}
              className="rounded bg-black px-6 py-2 text-white disabled:opacity-50 dark:bg-white dark:text-black"
            >
              Start Quiz
            </button>
          )}

          <button
            onClick={() => router.back()}
            className="rounded border px-6 py-2"
          >
            Back
          </button>
        </div>
      </div>
    </div>
  );
}
