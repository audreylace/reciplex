import LinearProgress from "@mui/material/LinearProgress";
import { OperationFailedAlert } from "../../../core/components/operation-failed-alert/operation-failed-alert.component";
import { useRefreshPage } from "../../../core/hooks/useRefreshPage.hook";
import { ContentWrapper } from "../../../core/components/content-wrapper/content-wrapper.component";
import { ContentTitle } from "../../../core/components/content-title/content-title.component";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import { StackedLabelValue } from "../../../core/components/stacked-label-value/stacked-label-value.component";
import Button from "@mui/material/Button";

/** template for invite forms */
export function InvitePageBodyTemplate({
  name,
  shortDescription,
  buttonCaption,
  buttonDisabled,
  title,
  onClick,
  showErrorBanner,
  showLoadingIndicator,
}: IInvitePageBodyTemplateProps) {
  const refreshPage = useRefreshPage()[1];
  return (
    <>
      {showErrorBanner && <OperationFailedAlert onRetry={refreshPage} />}
      {showLoadingIndicator && <LinearProgress />}
      <ContentWrapper topGutter>
        <ContentTitle>{title}</ContentTitle>
        <Box sx={{ mt: 2, my: 3 }}>
          <Stack sx={{ gap: 1 }}>
            <StackedLabelValue label="Name" value={name} />
            {shortDescription && (
              <StackedLabelValue
                label="Short Description"
                value={shortDescription}
              />
            )}
          </Stack>
        </Box>
        <Button disabled={buttonDisabled} variant="contained" onClick={onClick}>
          {buttonCaption}
        </Button>
      </ContentWrapper>
    </>
  );
}

/** props for `<InvitePageBodyTemplate />` */
export interface IInvitePageBodyTemplateProps {
  /** book name */
  name: string;
  /** book short description */
  shortDescription: string;
  /** caption for the button */
  buttonCaption: string;
  /** disabled the button */
  buttonDisabled?: boolean;
  /** title of the page body*/
  title: string;
  /** invoked on button click */
  onClick: () => void;
  /** when true the operation failed banner is shown */
  showErrorBanner?: boolean;
  /** when true the loading indicator is shown */
  showLoadingIndicator?: boolean;
}
