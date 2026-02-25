"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";

export default function TopBar() {
  const router = useRouter();
  const pathname = usePathname();
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("token");
    setIsLoggedIn(!!token);
  }, [pathname]); // 🔥 rerun on every route change

  function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    setIsLoggedIn(false);
    router.push("/login");
  }

  return (
    <header className="border-b bg-black">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <Link href="/" className="text-lg font-semibold text-white">
          LMS
        </Link>

        <nav className="flex items-center gap-2">
          {!isLoggedIn ? (
            <>
              <Link
                href="/login"
                className="rounded-full px-4 py-2 text-sm text-zinc-300 hover:bg-zinc-800 hover:text-white"
              >
                Login
              </Link>

              <Link
                href="/register"
                className="rounded-full bg-white px-4 py-2 text-sm font-medium text-black hover:bg-zinc-200"
              >
                Register
              </Link>
            </>
          ) : (
            <button
              onClick={logout}
              className="rounded-full bg-white px-4 py-2 text-sm font-medium text-black hover:bg-zinc-200"
            >
              Logout
            </button>
          )}
        </nav>
      </div>
    </header>
  );
}
