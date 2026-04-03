import { useEffect } from "preact/hooks";
import { useNavigate } from "react-router";
import { InformationBanner } from "../../../core/components/banner/banner.component";
import { useActiveUser } from "../../hooks/useActiveUser.hook";
import { useRehydrateActiveUser } from "../../hooks/useRehydrateActiveUser.hook";
import type { ComponentChildren } from "preact";

export function AuthenticatedRouteGuard({
  children,
}: {
  children: ComponentChildren;
}) {
  useRehydrateActiveUser();

  const isSynced = useActiveUser((s) => s.synced);
  const challengeNeeded = useActiveUser((s) => s.challengeNeeded);
  const userKey = useActiveUser((s) => s.userKey);

  const navigate = useNavigate();

  useEffect(() => {
    if (isSynced && (!userKey || challengeNeeded)) {
      navigate("/accounts/-/sign-in", {
        state: {
          redirect: window.location.href,
        },
      });
    }
  }, [navigate, isSynced, userKey, challengeNeeded]);

  if (!isSynced) {
    return (
      <InformationBanner title="Checking Sign-In status"></InformationBanner>
    );
  }
  if (challengeNeeded || !userKey) {
    return <InformationBanner title="Sign-In Needed"></InformationBanner>;
  }

  return children;
}
