import Box from "@mui/material/Box";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";

export function SyntaxExample({
  children,
  description,
  output,
}: React.PropsWithChildren<ISyntaxExampleProps>) {
  return (
    <Paper elevation={3} sx={{ mb: 3, p: 2 }}>
      <Box sx={{ mb: 1 }}>
        <Stack sx={{ gap: 1 }} direction="row">
          <Typography variant="body1" component="code">
            {children}
          </Typography>
          <Box>&rarr;</Box>
          <Typography variant="body1">{output}</Typography>
        </Stack>
      </Box>
      <Typography variant="body2">{description}</Typography>
    </Paper>
  );
}

export interface ISyntaxExampleProps {
  description: React.ReactNode;
  output: React.ReactNode;
}
