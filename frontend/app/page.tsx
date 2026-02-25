"use client";

import { useEffect, useState } from "react";
import Link from "next/link";

type QuizOverview = {
  id: number;
  title: string;
  timeLimitInMinutes: number;
  maxAttemptsPerUser: number;
  passingScorePercentage: number;
};

type Status = "loading" | "error" | "success";

export default function Page() {
  const [quizzes, setQuizzes] = useState<QuizOverview[]>([]);
  const [status, setStatus] = useState<Status>("loading");

  async function fetchQuizzes() {
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_BASE_URL}/api/Quiz/`
      );

      if (!res.ok) throw new Error();

      const data = await res.json();
   
      const publishedQuizzes = data.filter(q => q.isPublished);

      setQuizzes(publishedQuizzes);
      setStatus("success");
    } catch {
      setStatus("error");
    }
  }

  useEffect(() => {
    // Eerste load
    fetchQuizzes();

    // Alleen retry’en zolang het NIET success is
    if (status === "success") return;
    

    const interval = setInterval(fetchQuizzes, 5000);
    return () => clearInterval(interval);
  }, [status]);

  return (
    <div className="min-h-screen bg-zinc-50 px-6 py-12 dark:bg-black">
      <div className="mx-auto max-w-4xl">
        <h1 className="mb-8 text-3xl font-semibold text-zinc-900 dark:text-zinc-50">
          Available Quizzes
        </h1>

        {status === "loading" && (
          <p className="text-zinc-600 dark:text-zinc-400">
            Loading quizzes...
          </p>
        )}

        {status === "error" && (
          <div className="rounded-xl border border-red-200 bg-red-50 p-4 text-red-700 dark:border-red-900 dark:bg-red-900/20 dark:text-red-400">
            Could not load quizzes. Retrying automatically...
          </div>
        )}

        {status === "success" && quizzes.length === 0 && (
          <p className="text-zinc-600 dark:text-zinc-400">
            No quizzes available.
          </p>
        )}

        {status === "success" && quizzes.length > 0 && (
          <ul className="mt-4 space-y-4">
            {quizzes.map((quiz) => (
              <li
                key={quiz.id}
                className="flex items-center justify-between rounded-xl border border-zinc-200 bg-white p-6 shadow-sm dark:border-zinc-800 dark:bg-zinc-900"
              >
                <div>
                  <h2 className="text-lg font-medium text-zinc-900 dark:text-zinc-50">
                    {quiz.title}
                  </h2>
                  <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">
                    ⏱ {quiz.timeLimitInMinutes} min · 🎯 Pass{" "}
                    {quiz.passingScorePercentage}% · 🔁{" "}
                    {quiz.maxAttemptsPerUser} attempts
                  </p>
                </div>

                <Link
                  href={`/quiz/${quiz.id}`}
                  className="rounded-full bg-black px-5 py-2 text-sm font-medium text-white hover:bg-zinc-800 dark:bg-white dark:text-black dark:hover:bg-zinc-200"
                >
                  View Quiz
                </Link>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
