import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";

/**
 * Helper for simple key-value syntax rows
 */
export function SyntaxItem({
  label,
  code,
  desc,
}: {
  label: string;
  code: string;
  desc: string;
}) {
  return (
    <Box sx={{ display: "flex", alignItems: "baseline", gap: 1 }}>
      <Typography
        variant="body2"
        component="code"
        sx={{ fontWeight: "bold", minWidth: "70px" }}
      >
        {label}:
      </Typography>
      <Typography
        variant="body2"
        component="code"
        sx={{ color: "text.secondary" }}
      >
        {code}
      </Typography>
      <Typography variant="caption" sx={{ color: "text.disabled" }}>
        ({desc})
      </Typography>
    </Box>
  );
}
