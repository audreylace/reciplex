import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Skeleton from "@mui/material/Skeleton";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";

/** skeleton for the form shown while it loads */
export function DeleteWithNameVerificationSkeleton({
  entityType,
}: {
  /** the entity type */
  entityType: string;
}) {
  return (
    <>
      <Typography variant="h5" gutterBottom>
        <Stack direction={"row"} gap={2}>
          <span>Deleting {entityType}</span>
        </Stack>
      </Typography>
      <Typography variant="h6">
        <Stack direction="row" gap={1}>
          <Skeleton>
            <span>wwwwwwwwwwwww</span>
          </Skeleton>
        </Stack>
      </Typography>
      <Typography variant="body1">
        <Stack direction={"row"} gap={1}>
          <Skeleton>
            <span>
              fjff f jf jalfjf jf. fjk fjkklfj kf jkf jskf js fj j. jdjsklfdkj
              kf s fsd
            </span>
          </Skeleton>
        </Stack>
      </Typography>
      <Typography variant="body2" gutterBottom>
        <Stack direction="row" gap={1}>
          <span>Key: </span>
          <Skeleton>
            <span>wwwwwwwwwwww</span>
          </Skeleton>
        </Stack>
      </Typography>
      <Stack spacing={2} marginTop={3}>
        <Skeleton>
          <Typography variant="body1" gutterBottom>
            Type www www wwwww ww to delete
          </Typography>
        </Skeleton>
        <Skeleton width={"100%"}>
          <TextField fullWidth variant="filled" />
        </Skeleton>
        <Skeleton>
          <Box>
            <Button variant="contained">Delete</Button>
          </Box>
        </Skeleton>
      </Stack>
    </>
  );
}
