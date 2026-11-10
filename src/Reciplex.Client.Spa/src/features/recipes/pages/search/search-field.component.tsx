import IconButton from "@mui/material/IconButton";
import InputAdornment from "@mui/material/InputAdornment";
import Paper from "@mui/material/Paper";
import TextField from "@mui/material/TextField";
import SearchIcon from "@mui/icons-material/Search";
import CircularProgress from "@mui/material/CircularProgress";

/**
 * Main search field for the search page
 */
export function SearchField({ value, onChange, loading }: ISearchFieldProps) {
  return (
    <Paper sx={{ p: 2 }}>
      <TextField
        label="Search"
        variant="outlined"
        fullWidth
        value={value}
        onChange={(e) => {
          onChange(e.target.value ?? "");
        }}
        slotProps={{
          htmlInput: {
            maxLength: 100,
          },
          input: {
            endAdornment: (
              <InputAdornment position="end">
                <IconButton type="button" aria-label="search">
                  {loading ? (
                    <CircularProgress aria-label="Loading…" />
                  ) : (
                    <SearchIcon />
                  )}
                </IconButton>
              </InputAdornment>
            ),
          },
        }}
      />
    </Paper>
  );
}

/**
 * Properties for `<SearchField />`
 */
export interface ISearchFieldProps {
  /**
   * the value of the field
   */
  value: string;
  /**
   * Invoked on input change
   * @param value the new value
   */
  onChange: (value: string) => void;
  /**
   * When true, show the loading indicator instead of the button
   */
  loading?: boolean;
}
