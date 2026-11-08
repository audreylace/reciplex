import Stack from "@mui/material/Stack";
import { TablePageButton } from "./table-page-button.component";
import Box from "@mui/material/Box";
import ArrowBack from "@mui/icons-material/ArrowBack";
import ArrowForward from "@mui/icons-material/ArrowForward";
import FirstPage from "@mui/icons-material/FirstPage";
import LastPage from "@mui/icons-material/LastPage";
import { PageSizeSelect } from "../page-size-select/page-size-select.component";
import { FlexSpacer } from "./flex-spacer.component";

/**
 * Component for rending page table controls
 */
export function TablePageControls({
  jumpToStartPending,
  jumpToStart,
  previousPending,
  goPrevious,
  nextPending,
  goNext,
  jumpToEndPending,
  jumpToEnd,
  pageSize,
  setPageSize,
}: ITablePageControlsProps) {
  return (
    <Stack direction={"row"} sx={{ width: "100%", py: 2 }}>
      <TablePageButton
        disabled={!jumpToStart}
        onClick={jumpToStart}
        pending={!jumpToStartPending}
      >
        <FirstPage />
      </TablePageButton>
      <TablePageButton
        disabled={!previousPending}
        onClick={goPrevious}
        pending={previousPending}
      >
        <ArrowBack />
      </TablePageButton>
      <FlexSpacer />
      <Box>
        <PageSizeSelect value={pageSize} onChange={setPageSize} />
      </Box>
      <FlexSpacer />
      <TablePageButton
        disabled={!goNext}
        pending={nextPending}
        onClick={goNext}
      >
        <ArrowForward />
      </TablePageButton>
      <TablePageButton
        disabled={!jumpToEnd}
        pending={jumpToEndPending}
        onClick={jumpToEnd}
      >
        <LastPage />
      </TablePageButton>
    </Stack>
  );
}

/**
 * Properties for `TablePageControls` component
 */
export interface ITablePageControlsProps extends ITablePageDirectionControlsProps {
  /**
   * the current page size
   */
  pageSize: number;
  /**
   * sets the page size
   * @param newSize the requested size
   */
  setPageSize: (newSize: number) => void;
}

/**
 * cursor properties exported by `TablePageControls`
 */
export interface ITablePageDirectionControlsProps {
  /**
   * if the load for the next page is pending
   */
  nextPending: boolean;
  /**
   * invoked when the next button is clicked. If not provided, then next is disabled.
   */
  goNext?: () => void;
  /**
   * if the load for the previous page is pending
   */
  previousPending: boolean;
  /**
   * invoked when the previous button is clicked. If not provided then previous is disabled.
   */
  goPrevious?: () => void;
  /**
   * if the load for the last page is still occurring
   */
  jumpToEndPending: boolean;
  /**
   * jumps to last page. If not provided, then the end button is disabled
   */
  jumpToEnd?: () => void;
  /**
   * if the load for the first page is still pending
   */
  jumpToStartPending: boolean;
  /**
   * jumps to first page. If not provided, then the first button is disabled
   */
  jumpToStart?: () => void;
}
