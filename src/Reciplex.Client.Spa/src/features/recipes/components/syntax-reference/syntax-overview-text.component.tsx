import Typography from "@mui/material/Typography";
import type { ComponentChildren } from "preact";

export function SyntaxOverviewText({ children }: ISyntaxOverviewTextProps) {
  return (
    <Typography variant="body1" sx={{ mb: 3 }}>
      {children}
    </Typography>
  );
}

export interface ISyntaxOverviewTextProps {
  children?: ComponentChildren;
}
