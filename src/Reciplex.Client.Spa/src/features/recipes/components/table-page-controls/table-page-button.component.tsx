import IconButton from "@mui/material/IconButton";

/** Button for navigating a table */
export function TablePageButton({
  children,
  onClick,
  disabled,
  pending,
}: ITablePageButtonProps) {
  return (
    <IconButton loading={pending} disabled={disabled} onClick={onClick}>
      {children}
    </IconButton>
  );
}

/** props for table page button */
export interface ITablePageButtonProps {
  /** if the button is disabled */
  disabled?: boolean;
  /** component children in the button */
  children?: React.ReactNode;
  /** data for the action is loading */
  pending?: boolean;
  /** action to take on click */
  onClick?: () => void;
}
