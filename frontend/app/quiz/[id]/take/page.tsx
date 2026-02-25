"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { useAuthGuard } from "@/app/hooks/useAuthGuard";

type AnswerOption = {
  id: number;
  text: string;
};

type Question = {
  id: number;
  text: string;
  answerOptions: AnswerOption[];
};

type QuizTake = {
  id: number;
  title: string;
  questions: Question[];
};

export default function TakeQuizPage() {
  useAuthGuard();

  const { id } = useParams<{ id: string }>();
  const router = useRouter();

  const [quiz, setQuiz] = useState<QuizTake | null>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [answers, setAnswers] = useState<Record<number, number>>({});
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    async function fetchQuiz() {
      try {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_BASE_URL}/api/quiz/${id}/take`,
          {
            headers: {
              Authorization: `Bearer ${localStorage.getItem("token")}`,
            },
          }
        );

        if (!res.ok) {
          if (res.status === 401) {
            alert("Unauthorized");
            return;
          }

          if (res.status === 404) {
            alert("Quiz not found");
            return;
          }

          throw new Error(`HTTP ${res.status}`);
        }

        const text = await res.text();
        if (!text) {
          throw new Error("Empty response body");
        }

        const data: QuizTake = JSON.parse(text);
        setQuiz(data);
      } catch (err) {
        console.error(err);
        alert("Failed to load quiz");
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

  if (!quiz) return null;

  const question = quiz.questions[currentIndex];

  function selectAnswer(optionId: number) {
    setAnswers((prev) => ({
      ...prev,
      [question.id]: optionId,
    }));
  }

  async function submitQuiz() {
    if (!quiz) return;

    setSubmitting(true);

    const payload = {
      quizId: quiz.id,
      answers: quiz.questions.map((q) => ({
        questionId: q.id,
        selectedAnswerOptionId: answers[q.id] ?? null,
      })),
    };

    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_BASE_URL}/api/quiz/${quiz.id}/submit`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
          body: JSON.stringify(payload),
        }
      );

      if (!res.ok) {
        const error = await res.text();
        throw new Error(error);
      }

      const result = await res.json();

      alert(
        `Score: ${result.scorePercentage}%\nPassed: ${result.isPassed}`
      );

      router.push("/");
    } catch (err) {
      console.error(err);
      alert("Failed to submit quiz");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="min-h-screen bg-zinc-50 px-6 py-12 dark:bg-black">
      <div className="mx-auto max-w-3xl rounded-xl bg-white p-8 shadow dark:bg-zinc-900">
        <h1 className="mb-6 text-2xl font-semibold">{quiz.title}</h1>

        <p className="mb-4 text-sm text-zinc-500">
          Question {currentIndex + 1} of {quiz.questions.length}
        </p>

        <div className="mb-6 rounded border p-4 dark:border-zinc-700">
          <h2 className="mb-4 text-lg font-medium">{question.text}</h2>

          <ul className="space-y-2">
            {question.answerOptions.map((option) => (
              <li key={option.id}>
                <label className="flex cursor-pointer items-center gap-2">
                  <input
                    type="radio"
                    name={`question-${question.id}`}
                    checked={answers[question.id] === option.id}
                    onChange={() => selectAnswer(option.id)}
                  />
                  {option.text}
                </label>
              </li>
            ))}
          </ul>
        </div>

        <div className="flex items-center justify-between">
          <button
            disabled={currentIndex === 0}
            onClick={() => setCurrentIndex((i) => i - 1)}
            className="rounded border px-4 py-2 disabled:opacity-50"
          >
            Previous
          </button>

          {currentIndex < quiz.questions.length - 1 ? (
            <button
              onClick={() => setCurrentIndex((i) => i + 1)}
              className="rounded bg-black px-4 py-2 text-white dark:bg-white dark:text-black"
            >
              Next
            </button>
          ) : (
            <button
              onClick={submitQuiz}
              disabled={submitting}
              className="rounded bg-green-600 px-6 py-2 text-white disabled:opacity-50"
            >
              {submitting ? "Submitting..." : "Submit Quiz"}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
