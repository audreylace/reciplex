import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";

export function StackedLabelValue({
  label,
  value,
  gutterBottom,
}: IStackedKeyValueProps) {
  return (
    <Typography
      variant="body1"
      component={
        /* needed to prevent `improper element nesting error from preact` */
        "div"
      }
      gutterBottom={gutterBottom}
    >
      <Stack direction={"column"} spacing={0}>
        <Typography variant="subtitle2">{label}</Typography>
        <span>{value}</span>
      </Stack>
    </Typography>
  );
}

export interface IStackedKeyValueProps {
  label: string;
  value: string;
  gutterBottom?: boolean;
}
