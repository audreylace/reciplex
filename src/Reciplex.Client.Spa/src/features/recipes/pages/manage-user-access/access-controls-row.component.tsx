import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import TableCell from "@mui/material/TableCell";
import TableRow from "@mui/material/TableRow";
import ToggleButton from "@mui/material/ToggleButton";
import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";
import ClearIcon from "@mui/icons-material/Clear";

/** row in a table with controls to manager the permissions for a single user */
export function AccessControlsRow({
  value,
  onChange,
  displayName,
  disabled,
}: IAccessControlsRowProps) {
  return (
    <TableRow>
      <TableCell>
        <Stack spacing={1} direction="row" sx={{ alignItems: "center" }}>
          <span>{displayName}</span>
          <Chip label="new" color="primary" size="small" />
        </Stack>
      </TableCell>
      <TableCell align="right">
        <ToggleButtonGroup
          exclusive
          value={value}
          disabled={disabled}
          onChange={(_, s) => onChange(s)}
        >
          <ToggleButton value="readwrite">Read/Write</ToggleButton>
          <ToggleButton value="read">Read</ToggleButton>
          <ToggleButton value="none">None</ToggleButton>
          <ToggleButton value="delete">
            <Stack direction="row">
              <ClearIcon />
              Remove
            </Stack>
          </ToggleButton>
        </ToggleButtonGroup>
      </TableCell>
    </TableRow>
  );
}

/**
 * props for `<AccessControlsRow />`
 */
export interface IAccessControlsRowProps {
  /** the user's display name */
  displayName: string;
  /** the current selected value */
  value: AccessControlsButtonsValues;
  /** invoked on value change */
  onChange: (selectedValue: AccessControlsButtonsValues) => void;
  /** if the control is disabled */
  disabled?: boolean;
}

/** possible button states */
export type AccessControlsButtonsValues =
  | "readwrite"
  | "read"
  | "delete"
  | "none";
