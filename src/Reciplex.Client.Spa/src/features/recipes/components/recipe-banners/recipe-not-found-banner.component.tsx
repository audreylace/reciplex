import { useNavigate } from "react-router";
import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import { makeBookListPath } from "../../route-utils";

/** banner when a user tries to load a non-existent recipe */
export function RecipeNotFoundBanner() {
  const navigate = useNavigate();
  return (
    <NotFoundAlert
      href={makeBookListPath()}
      onClick={() => navigate(makeBookListPath())}
      caption="View Books"
      entityName="Recipe"
    />
  );
}
