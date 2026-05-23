import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import type { ComponentChild } from "preact";

/** reusable header for all Reciplex pages */
export function PageHeader({
  title,
  sideComponent,
  subTitle,
  titleComponent,
}: IPageHeaderProps) {
  if (
    typeof title !== "string" &&
    !sideComponent &&
    typeof subTitle !== "string"
  ) {
    return null;
  }

  return (
    <Box
      sx={{
        mb: 3,
      }}
    >
      {(typeof title === "string" || sideComponent) && (
        <Stack direction={"row"} sx={{ width: "100%", mb: 1 }}>
          <Stack
            direction={"row"}
            alignItems={"center"}
            gap={2}
            sx={{
              flex: "1 1 auto",
            }}
          >
            <Typography variant="h2">{title}</Typography>
            {titleComponent}
          </Stack>
          {sideComponent}
        </Stack>
      )}
      {typeof subTitle === "string" && (
        <Typography variant="h5">{subTitle}</Typography>
      )}
    </Box>
  );
}

/** props for `<PageHeader />` */
export interface IPageHeaderProps {
  /** page title */
  title?: string;
  /** page sub title */
  subTitle?: string;
  /** component to render on the right */
  sideComponent?: ComponentChild;
  /** component to render after the title string */
  titleComponent?: ComponentChild;
}
