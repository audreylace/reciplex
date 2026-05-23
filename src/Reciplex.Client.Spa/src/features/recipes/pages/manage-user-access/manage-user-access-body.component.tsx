import TableContainer from "@mui/material/TableContainer";
import { ContentTitle } from "../../../core/components/content-title/content-title.component";
import { ContentWrapper } from "../../../core/components/content-wrapper/content-wrapper.component";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { BookMutationNotAuthorizedBanner } from "../../components/book-mutation-not-authorized-banner/book-mutation-not-authorized-banner.component";
import { BookSettingsMenuButton } from "../../components/book-settings-menu/book-settings-menu-button.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetUsersWithBookAccess } from "../../hooks/useGetUsersWithBookAccess.hook";
import Table from "@mui/material/Table";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import TableBody from "@mui/material/TableBody";
import Button from "@mui/material/Button";
import {
  AccessControlsRow,
  type AccessControlsButtonsValues,
} from "./access-controls-row.component";
import {
  useManageUserAccessBodyState,
  type IManageUserAccessBodyStateEntry,
} from "./useManageUserAccessBodyState.hook";
import { usePatchUsersWithBookAccess } from "../../hooks/usePatchUsersWithBookAccess.hook";
import LinearProgress from "@mui/material/LinearProgress";
import { useNavigate } from "react-router";
import { makeViewRecipeBookPath } from "../../route-utils";
import { OperationFailedAlert } from "../../../core/components/operation-failed-alert/operation-failed-alert.component";
import { useRefreshPage } from "../../../core/hooks/useRefreshPage.hook";

/** page body for managing who has access to a recipe book */
export function ManageUserAccessBody({ bookId }: IManageUserAccessBodyProps) {
  const navigate = useNavigate();
  const refresh = useRefreshPage()[1];
  const { data, isError, isPending } = useGetRecipeBookById(bookId);
  const {
    isPending: accessListLoadPending,
    isError: accessListLoadError,
    data: accessListData,
  } = useGetUsersWithBookAccess(bookId, {
    enabled: data?.mayManageAccess ?? false,
  });

  const { state, updateUserStateEntry, buildResponse } =
    useManageUserAccessBodyState(data?.mayManageAccess ? accessListData : null);

  const {
    mutateAsync,
    isIdle: mutationIdle,
    isPending: mutationPending,
    isError: mutationError,
  } = usePatchUsersWithBookAccess();
  const submit = async () => {
    const data = buildResponse();
    if (!data || data.length === 0 || !mutationIdle) {
      return;
    }

    await mutateAsync({ bookId: bookId, data });
    navigate(makeViewRecipeBookPath(bookId));
  };

  if (data && !data.mayManageAccess) {
    return (
      <BookMutationNotAuthorizedBanner
        bookId={bookId}
        message="You may not manage user access for this book"
      />
    );
  }
  if (isError || accessListLoadError) {
    return <LoadingFailedAlert />;
  }

  if (isPending || accessListLoadPending) {
    return <LoadingIndicator />;
  }

  if (!data || !accessListData) {
    return <RecipeBookNotFoundBanner />;
  }

  if (!state) {
    return <LoadingIndicator />;
  }

  return (
    <>
      <PageHeader
        title="Manage User Access"
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
        <ContentTitle>
          Add, remove, and approve access to recipe book
        </ContentTitle>
        {mutationPending && <LinearProgress />}
        {mutationError && <OperationFailedAlert onRetry={refresh} />}
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Display Name</TableCell>
                <TableCell align="right">Permissions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {state.map((u) => (
                <AccessControlsRow
                  disabled={!mutationIdle}
                  key={u.prev.userKey}
                  displayName={u.prev.userDisplayName}
                  value={mapAccessButtonSelection(u)}
                  onChange={(newValue: AccessControlsButtonsValues) =>
                    updateUserStateEntry(u.prev.userKey, newValue)
                  }
                />
              ))}
            </TableBody>
          </Table>
        </TableContainer>
        <Button variant="contained" disabled={!mutationIdle} onClick={submit}>
          Save
        </Button>
      </ContentWrapper>
    </>
  );
}

/** props for `<ManageUserAccessBody /> */
export interface IManageUserAccessBodyProps {
  /** the id of the book */
  bookId: string;
}

function mapAccessButtonSelection(u: IManageUserAccessBodyStateEntry) {
  let selectedValue: AccessControlsButtonsValues = "read";
  if (!u.new) {
    selectedValue = "delete";
  } else if (!u.new.reviewed) {
    selectedValue = "none";
  } else if (!u.new.mayViewBook) {
    selectedValue = "none";
  } else if (u.new.mayEditBook) {
    selectedValue = "readwrite";
  }
  return selectedValue;
}
