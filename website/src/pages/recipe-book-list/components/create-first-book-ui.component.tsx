import { Link } from "react-router";
import { makeCreateRecipeBookPath } from "../../../features/recipes/route-utils";
import { TableMessage } from "./table-message/table-message.component";

export function CreateFirstBookUI() {
  return (
    <TableMessage>
      <Link to={makeCreateRecipeBookPath()}>Create your first recipe book</Link>
    </TableMessage>
  );
}
