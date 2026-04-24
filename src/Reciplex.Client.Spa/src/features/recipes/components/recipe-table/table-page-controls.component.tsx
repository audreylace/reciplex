import Stack from "@mui/material/Stack";
import { NavigationButton } from "../navigation-button/navigation-button.component";
import Box from "@mui/material/Box";
import {
  ArrowBack,
  ArrowForward,
  FirstPage,
  LastPage,
} from "@mui/icons-material";
import { PageSizeSelect } from "../page-size-select/page-size-select.component";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";

export function TablePageControls({
  previous,
  next,
  nextLoading,
  previousLoading,
}: ITablePageControlsProps) {
  const pageSize = useRecipeClientStateContext((s) => s.recipeListPageSize);
  const setPageSize = useRecipeClientStateContext(
    (s) => s.setRecipeListPageSize,
  );
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
}

function FlexSpacer() {
  return (
    <Box
      sx={{
        flex: "1 1 auto",
      }}
    ></Box>
  );
}
