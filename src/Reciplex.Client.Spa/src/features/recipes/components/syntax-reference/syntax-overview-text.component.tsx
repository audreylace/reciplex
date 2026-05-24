import Typography from "@mui/material/Typography";

export function SyntaxOverviewText({ children }: ISyntaxOverviewTextProps) {
  return (
    <Typography variant="body1" sx={{ mb: 3 }}>
      {children}
    </Typography>
  );
}

export interface ISyntaxOverviewTextProps {
  children?: React.ReactNode;
}
