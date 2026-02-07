import { useNavigate } from "react-router";
import { TableMessage } from "./table-message/table-message.component";

export function LoadFailedUi() {
  const navigate = useNavigate();
  return (
    <TableMessage>
      Retrieving list of recipe books failed.{" "}
      <a
        href="#"
        onClick={(event) => {
          event.preventDefault();
          navigate(0);
        }}
      >
        Try again?
      </a>
    </TableMessage>
  );
}
