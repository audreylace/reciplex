import Stack from "@mui/material/Stack";
import { NavigationButton } from "../navigation-button/navigation-button.component";
import Box from "@mui/material/Box";
import { PageSizeSelect } from "./page-size-select.component";
import {
  ArrowBack,
  ArrowForward,
  FirstPage,
  LastPage,
} from "@mui/icons-material";

export function TablePageControls({ previous, next }: ITablePageControlsProps) {
  return (
    <Stack direction={"row"} sx={{ width: "100%", py: 2 }}>
      <NavigationButton disabled={!previous} source="next">
        <FirstPage />
      </NavigationButton>
      <NavigationButton disabled={!previous} source="previous" at={previous}>
        <ArrowBack />
      </NavigationButton>
      <FlexSpacer />
      <Box>
        <PageSizeSelect />
      </Box>
      <FlexSpacer />
      <NavigationButton disabled={!next} source="next" at={next}>
        <ArrowForward />
      </NavigationButton>
      <NavigationButton disabled={!next} source="previous">
        <LastPage />
      </NavigationButton>
    </Stack>
  );
}

export interface ITablePageControlsProps {
  previous?: string;
  next?: string;
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
