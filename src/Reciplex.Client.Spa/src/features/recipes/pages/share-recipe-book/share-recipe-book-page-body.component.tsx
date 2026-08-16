import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { BookMutationNotAuthorizedBanner } from "../../components/book-mutation-not-authorized-banner/book-mutation-not-authorized-banner.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { useEffect, useRef, useState } from "react";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { useUpdateRecipeBookShareKeyMutation } from "../../hooks/useUpdateRecipeBookShareMutation.hook";
import LinearProgress from "@mui/material/LinearProgress";
import { ContinueDialog } from "./continue-dialog.component";
import { ShareSettingsForm } from "./share-settings-form.component";
import { OperationFailedAlert } from "../../../core/components/operation-failed-alert/operation-failed-alert.component";
import { useRefreshPage } from "../../../core/hooks/useRefreshPage.hook";
import { ConcurrencyConflictAlert } from "../../../core/components/concurrency-conflict-alert/concurrency-conflict-alert.component";
import { HttpError } from "../../../core/utils/http-error";
import { BookSettingsMenuButtonViaModel } from "../../components/book-settings-menu/book-settings-menu-button-via-model.component";
import { bookPageSubtitle } from "../../utils/book-page-subtitle/book-page-subtitle";

/** body for the share recipe book page */
export function ShareRecipeBookPageBody({
  bookId,
}: IShareRecipeBookPageBodyProps) {
  const [dialogMode, setDialogMode] = useState<null | "clear" | "regenerate">(
    null,
  );

  const refresh = useRefreshPage()[1];
  const shareMutation = useUpdateRecipeBookShareKeyMutation();
  const { isError, isPending, data, refetch } = useGetRecipeBookById(bookId, {
    alwaysFresh: true,
  });

  const { updateTag, newChanges, hasNewChanges, clearNewChangesFlag } =
    useWatchForChanges(data?.versionTag);

  const doMutation = async (kind: "clear" | "regenerate") => {
    if (!data?.versionTag || !shareMutation.isIdle) {
      return;
    }
    setDialogMode(null);
    try {
      const { versionTag } = await shareMutation.mutateAsync({
        bookId,
        kind,
        versionTag: data?.versionTag,
      });
      shareMutation.reset();
      updateTag(versionTag);
      clearNewChangesFlag();
    } catch (error) {
      if (error instanceof HttpError) {
        if (error.status === 412 || error.status === 409) {
          hasNewChanges();
          const model = await refetch();
          updateTag(model.data?.versionTag);
          shareMutation.reset();
        }
      }
    }
  };

  if (isPending) {
    return <LoadingIndicator />;
  }

  if (isError) {
    return <LoadingFailedAlert />;
  }

  if (!data) {
    return <RecipeBookNotFoundBanner />;
  }

  if (!data.mayManageAccess) {
    return (
      <BookMutationNotAuthorizedBanner
        message="not authorized to mange this book"
        bookId={bookId}
      />
    );
  }

  const formDisabled = !shareMutation.isIdle;
  return (
    <>
      <PageHeader
        title="Invitation Settings"
        subTitle={bookPageSubtitle(data.name)}
        sideComponent={<BookSettingsMenuButtonViaModel book={data} />}
      />

      {shareMutation.isPending && <LinearProgress />}

      {newChanges && (
        <ConcurrencyConflictAlert
          onReset={() => {
            clearNewChangesFlag();
          }}
        />
      )}

      {shareMutation.isError && <OperationFailedAlert onRetry={refresh} />}

      <ShareSettingsForm
        shareKey={data.shareKey}
        bookId={data.id}
        disableForm={formDisabled}
        onAction={(kind) => {
          if (data?.shareKey) {
            setDialogMode(kind);
          } else if (!data?.shareKey && kind === "regenerate") {
            doMutation("regenerate");
          }
        }}
      />

      {dialogMode && !formDisabled && (
        <ContinueDialog
          onCancel={() => {
            setDialogMode(null);
          }}
          onContinue={() => {
            if (dialogMode) {
              doMutation(dialogMode);
            }
            setDialogMode(null);
          }}
          title={
            dialogMode === "clear"
              ? "Disable Invite Link?"
              : "Reset Invite link?"
          }
        />
      )}
    </>
  );
}

/** props for `<ShareRecipeBookPageBody />` */
export interface IShareRecipeBookPageBodyProps {
  /** id of the book to mutate */
  bookId: string;
}

/**
 * hook watching for async changes to the book
 * @param versionTag the current observed version tag
 */
function useWatchForChanges(
  versionTag: string | null | undefined,
): IUseWatchForChangesReturn {
  const [newChanges, setNewChanges] = useState(false);
  const lastTagRef = useRef<string | null>(null);
  useEffect(() => {
    if (lastTagRef.current !== versionTag && versionTag && lastTagRef.current) {
      setNewChanges(true);
    }
    lastTagRef.current = versionTag ?? lastTagRef.current;
  }, [versionTag]);

  return {
    newChanges,
    hasNewChanges: () => setNewChanges(true),
    clearNewChangesFlag: () => setNewChanges(false),
    updateTag: (newTag?: string | null) => {
      lastTagRef.current = newTag ?? null;
    },
  };
}

/** return from useWatchForChanges */
interface IUseWatchForChangesReturn {
  /** if there has been async changes */
  newChanges: boolean;
  /** invoke to set new changes flag */
  hasNewChanges: () => void;
  /** invoke to clear new changes flag */
  clearNewChangesFlag: () => void;
  /** updates cached version tag */
  updateTag: (newTag?: string | null) => void;
}
