import { useNavigate } from "react-router";
import MenuItem from "@mui/material/MenuItem";
import ListItemIcon from "@mui/material/ListItemIcon";
import ShareIcon from "@mui/icons-material/Share";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { makeSharePath, makeShareSettingsPath } from "../../route-utils";

/** button in the recipe book menu either triggering a share or taking user to share settings page */
export function ShareButton({ bookId }: IShareButtonProps) {
  const bookQuery = useGetRecipeBookById(bookId);
  const navigate = useNavigate();
  const shareMethod = async () => {
    if (navigator.share) {
      if (
        bookQuery.data &&
        bookQuery.data.mayShare &&
        bookQuery.data.shareKey
      ) {
        try {
          await navigator.share({
            title: "Join my book",
            text: `Join my recipe book: ${bookQuery.data.name}`,
            url: makeSharePath(bookId, bookQuery.data.shareKey),
          });
          // eslint-disable-next-line @typescript-eslint/no-unused-vars
        } catch (_) {
          /* empty */
        }
        return;
      }
      navigate(makeShareSettingsPath(bookId));
    }
  };
  if (bookQuery.data && bookQuery.data.mayShare) {
    return (
      <MenuItem onClick={shareMethod}>
        <ListItemIcon>
          <ShareIcon fontSize="small" />
        </ListItemIcon>
        Invite
      </MenuItem>
    );
  }
}

/** props for `<ShareButton />` */
export interface IShareButtonProps {
  /** the book id */
  bookId: string;
}
