import Chip from "@mui/material/Chip";
import { makeSharePath } from "../../route-utils";
import { NavLink } from "react-router";
import ShareIcon from "@mui/icons-material/Share";
/**
 * Chip indicating if a recipe book is shared.
 * Does not load data. Caller assumes responsibility
 * of hiding/showing it.
 */
export function SharedRecipeBookIndicatorNoLoad({
  bookId,
}: ISharedRecipeBookIndicatorNoLoadProps) {
  return (
    <Chip
      label="Shared with You"
      icon={<ShareIcon />}
      variant="outlined"
      clickable
      component={NavLink}
      to={makeSharePath(bookId, "")}
      size={"small"}
    />
  );
}

/** props for `<SharedRecipeBookIndicatorNoLoad />` */
export interface ISharedRecipeBookIndicatorNoLoadProps {
  /** the id of the book */
  bookId: string;
}
