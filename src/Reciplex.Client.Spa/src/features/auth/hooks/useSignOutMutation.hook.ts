import { useMutation } from "@tanstack/react-query";
import { useSignOutClient } from "./useSignOutClient.hook";
import { useActiveUser } from "./useActiveUser.hook";

/**
 * mutation for triggering a sign out action
 * @returns the mutation
 */
export function useSignOutMutation() {
  const signOutClient = useSignOutClient();

  const resetUser = useActiveUser((s) => s.reset);

  return useMutation({
    mutationFn: () => signOutClient.signOutAsync(),
    onSuccess: () => {
      resetUser(); // clear json in local storage
    },
  });
}
