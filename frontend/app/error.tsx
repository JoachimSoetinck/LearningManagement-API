"use client";

import { useRouter } from "next/navigation";

export default function Error({
  error,
  reset,
}: {
  error: Error;
  reset: () => void;
}) {
  const router = useRouter();

  return (
    <div className="flex min-h-screen items-center justify-center bg-zinc-50 dark:bg-black">
      <div className="rounded-xl bg-white p-6 shadow dark:bg-zinc-900">
        <h2 className="mb-2 text-xl font-semibold text-red-600">
          Something went wrong
        </h2>

        <p className="mb-4 text-zinc-600 dark:text-zinc-400">
          {error.message}
        </p>

        <button
          onClick={() => {
            reset();
            router.refresh();
          }}
          className="rounded bg-black px-4 py-2 text-white hover:bg-zinc-800"
        >
          Try again
        </button>
      </div>
    </div>
  );
}
