import Stack from "@mui/material/Stack";
import { NavigationButton } from "../navigation-button/navigation-button.component";
import Box from "@mui/material/Box";
import ArrowBack from "@mui/icons-material/ArrowBack";
import ArrowForward from "@mui/icons-material/ArrowForward";
import FirstPage from "@mui/icons-material/FirstPage";
import LastPage from "@mui/icons-material/LastPage";
import { PageSizeSelect } from "../page-size-select/page-size-select.component";
import { FlexSpacer } from "./flex-spacer.component";

export function TablePageControls({
  previous,
  next,
  nextLoading,
  previousLoading,
  pageSize,
  setPageSize,
}: ITablePageControlsProps) {
  return (
    <Stack direction={"row"} sx={{ width: "100%", py: 2 }}>
      <NavigationButton
        disabled={!previous}
        source="next"
        loading={previousLoading}
      >
        <FirstPage />
      </NavigationButton>
      <NavigationButton
        disabled={!previous}
        source="previous"
        at={previous}
        loading={previousLoading}
      >
        <ArrowBack />
      </NavigationButton>
      <FlexSpacer />
      <Box>
        <PageSizeSelect value={pageSize} onChange={setPageSize} />
      </Box>
      <FlexSpacer />
      <NavigationButton
        disabled={!next}
        source="next"
        at={next}
        loading={nextLoading}
      >
        <ArrowForward />
      </NavigationButton>
      <NavigationButton
        disabled={!next}
        source="previous"
        loading={nextLoading}
      >
        <LastPage />
      </NavigationButton>
    </Stack>
  );
}

export interface ITablePageControlsProps {
  previous?: string;
  next?: string;
  nextLoading?: boolean;
  previousLoading?: boolean;
  pageSize: number;
  setPageSize: (newSize: number) => void;
}
