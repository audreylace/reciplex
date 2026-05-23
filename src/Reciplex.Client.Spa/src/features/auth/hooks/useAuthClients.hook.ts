import { useContext } from "react";
import { createContext } from "react";
import type { ChallengeHttpClient } from "../http-clients/challenge-http-client";
import type { UsersHttpClient } from "../http-clients/users-http-client";

/**
 * context for storing the recipe book store
 */
export const AuthClients = createContext<IAuthClients | null>(null);

export interface IAuthClients {
  challengeClient: ChallengeHttpClient;
  usersClient: UsersHttpClient;
}

/**
 * hook that returns the recipe store
 * @returns the recipe store throwing on error
 */
export function useAuthClients(): IAuthClients {
  const context = useContext(AuthClients);
  if (!context) {
    throw Error("IAuthClients context missing");
  }
  return context;
}
