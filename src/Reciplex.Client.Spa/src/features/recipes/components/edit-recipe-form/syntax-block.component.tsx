import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";

/**
 * Helper for larger code blocks with titles
 */
export function SyntaxBlock({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <Box>
      <Typography
        variant="caption"
        sx={{ fontWeight: 700, display: "block", mb: 0.5 }}
      >
        {title}
      </Typography>
      <Box>{children}</Box>
    </Box>
  );
}
