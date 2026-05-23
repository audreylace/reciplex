import Typography from "@mui/material/Typography";

/** title for a content section */
export function ContentTitle({ children }: IContentTitleProps) {
  return (
    <Typography variant="h5" gutterBottom>
      {children}
    </Typography>
  );
}

/** props for `<ContentTitle />` */
export interface IContentTitleProps {
  /** component children */
  children?: React.ReactNode;
}
