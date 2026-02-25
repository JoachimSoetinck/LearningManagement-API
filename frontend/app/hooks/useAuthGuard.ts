"use client";

import { useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";

export function useAuthGuard() {
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    const token = localStorage.getItem("token");

    if (!token) {
      router.replace(`/login?redirect=${encodeURIComponent(pathname)}`);
    }
  }, [pathname, router]); // 🔥 pathname toevoegen
}
