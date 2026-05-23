import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Dialog from "@mui/material/Dialog";
import Typography from "@mui/material/Typography";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardActions from "@mui/material/CardActions";

/** dialog to confirm destructive share link mutation */
export function ContinueDialog({
  onCancel,
  onContinue,
  title,
}: IContinueDialogProps) {
  return (
    <Dialog open onClose={onCancel}>
      <Card variant="outlined">
        <CardContent>
          <Typography variant="h5" gutterBottom>
            {title}
          </Typography>
          <Stack gap={1}>
            <Typography variant="body1">
              Approved users will still have access to this recipe book.
            </Typography>
          </Stack>
        </CardContent>
        <CardActions>
          <Stack direction="row" gap={1}>
            <Button onClick={onCancel}>Cancel</Button>
            <Button color="error" onClick={onContinue}>
              Continue
            </Button>
          </Stack>
        </CardActions>
      </Card>
    </Dialog>
  );
}

/** props for `<ContinueDialog />` */
export interface IContinueDialogProps {
  /** invoked when the user continues the action */
  onContinue: () => void;
  /** invoked on dialog cancel or close */
  onCancel: () => void;
  /** dialog title */
  title: string;
}
