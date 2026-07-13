import { useContext } from "react";
import { createContext } from "react";
import type { SignOutHttpClient } from "../http-clients/sign-out-http-client";

/**
 * context for storing the recipe book store
 */
export const SignOutClient = createContext<SignOutHttpClient | null>(null);

/**
 * hook that returns the recipe store
 * @returns the recipe store throwing on error
 */
export function useSignOutClient(): SignOutHttpClient {
  const context = useContext(SignOutClient);
  if (!context) {
    throw Error("Sign out client missing");
  }
  return context;
}
