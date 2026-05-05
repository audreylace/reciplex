import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import type { ComponentChild } from "preact";

export function PageHeader({
  title,
  sideComponent,
  subTitle,
}: {
  title?: string;
  subTitle?: string;
  sideComponent?: ComponentChild;
}) {
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
          <Box
            sx={{
              flex: "1 1 auto",
            }}
          >
            <Typography variant="h2">{title}</Typography>
          </Box>
          {sideComponent}
        </Stack>
      )}
      {typeof subTitle === "string" && (
        <Typography variant="h5">{subTitle}</Typography>
      )}
    </Box>
  );
}
