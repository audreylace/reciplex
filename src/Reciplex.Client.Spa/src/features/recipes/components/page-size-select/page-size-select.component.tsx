import TextField from "@mui/material/TextField";
import MenuItem from "@mui/material/MenuItem";

/**
 * Component for selecting the number of results in a list
 */
export function PageSizeSelect({ value, onChange }: IPageSizeSelectProps) {
  const textFieldOnChange = (event: Event) => {
    let parsedSize = parseInt((event.target as HTMLInputElement)?.value ?? "");
    if (isNaN(parsedSize)) {
      parsedSize = 10;
    }
    onChange(parsedSize);
  };

  return (
    <TextField
      onChange={textFieldOnChange}
      select
      label="Count"
      defaultValue="10"
      value={value}
      size="small"
      sx={{
        width: "5em",
      }}
    >
      <MenuItem value={5}>5</MenuItem>
      <MenuItem value={10}>10</MenuItem>
      <MenuItem value={20}>20</MenuItem>
      <MenuItem value={50}>50</MenuItem>
      <MenuItem value={100}>100</MenuItem>
    </TextField>
  );
}

export interface IPageSizeSelectProps {
  value: number;
  onChange: (value: number) => void;
}
