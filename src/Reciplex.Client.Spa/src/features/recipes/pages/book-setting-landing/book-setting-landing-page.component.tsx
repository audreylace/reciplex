import { useNavigate, useParams } from "react-router";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { BookSettingsMenuButton } from "../../components/book-settings-menu/book-settings-menu-button.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { ContentWrapper } from "../../../core/components/content-wrapper/content-wrapper.component";
import { ContentTitle } from "../../../core/components/content-title/content-title.component";
import List from "@mui/material/List";
import ListItemText from "@mui/material/ListItemText";
import EditIcon from "@mui/icons-material/Edit";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import GroupsIcon from "@mui/icons-material/Groups";
import Delete from "@mui/icons-material/Delete";
import ShareIcon from "@mui/icons-material/Share";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemIcon from "@mui/material/ListItemIcon";
import {
  makeBookDeletePath,
  makeBookDetailsSettingsPath,
  makeManageAccessPath,
  makeShareSettingsPath,
  makeViewRecipeBookPath,
} from "../../route-utils";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

export function BookSettingLandingPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const { data, isError, isPending } = useGetRecipeBookById(bookId);
  const navigate = useNavigate();

  if (isError) {
    <LoadingFailedAlert />;
  }

  if (isPending) {
    return <LoadingIndicator />;
  }

  if (!bookId || !data) {
    return <RecipeBookNotFoundBanner />;
  }

  return (
    <>
      <BrowserTitle title="Book Settings" />
      <PageHeader
        title="Manage Book"
        subTitle={`Book - ${data.name}`}
        sideComponent={
          <BookSettingsMenuButton
            bookId={bookId}
            mayEdit={data.mayEdit ?? false}
            mayDelete={data.mayDelete ?? false}
            mayManageShareAccess
          />
        }
      />
      <ContentWrapper>
        <ContentTitle>Manage settings for this recipe book</ContentTitle>
        <List>
          <ListItemButton
            onClick={() => navigate(makeViewRecipeBookPath(bookId))}
          >
            <ListItemIcon>
              <ArrowBackIcon />
            </ListItemIcon>
            <ListItemText
              primary="Recipe List"
              secondary="View all recipes in this book"
            />
          </ListItemButton>
          <ListItemButton
            onClick={() => navigate(makeBookDetailsSettingsPath(bookId))}
          >
            <ListItemIcon>
              <EditIcon />
            </ListItemIcon>
            <ListItemText
              primary="Details"
              secondary="Edit book name and description"
            />
          </ListItemButton>
          <ListItemButton onClick={() => navigate(makeBookDeletePath(bookId))}>
            <ListItemIcon>
              <Delete />
            </ListItemIcon>
            <ListItemText
              primary="Delete"
              secondary="Form for permanently deleting this book"
            />
          </ListItemButton>
          <ListItemButton
            onClick={() => navigate(makeShareSettingsPath(bookId))}
          >
            <ListItemIcon>
              <ShareIcon />
            </ListItemIcon>
            <ListItemText
              primary="Invite Settings"
              secondary="Copy and manage the recipe book share link"
            />
          </ListItemButton>
          <ListItemButton
            onClick={() => navigate(makeManageAccessPath(bookId))}
          >
            <ListItemIcon>
              <GroupsIcon />
            </ListItemIcon>

            <ListItemText
              primary="Manage Access"
              secondary="Control per user access to this recipe book"
            />
          </ListItemButton>
        </List>
      </ContentWrapper>
    </>
  );
}
