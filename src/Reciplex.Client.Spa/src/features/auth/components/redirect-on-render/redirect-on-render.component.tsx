import { useEffect } from "react";
import { useSelectAccountNavigate } from "../../hooks/useSelectAccountNavigate.hook";

/** Component that when rendered goes to the account select page */
export function RedirectOnRender() {
  const goToSelect = useSelectAccountNavigate()[1];

  useEffect(() => {
    goToSelect({
      replace: true, // back button should go back to the page that summoned us
    });
  }, [goToSelect]);

  return null;
}
