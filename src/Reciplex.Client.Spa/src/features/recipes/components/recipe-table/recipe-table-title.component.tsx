import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import InputAdornment from "@mui/material/InputAdornment";
import Typography from "@mui/material/Typography";
import SearchIcon from "@mui/icons-material/Search";
import { useId } from "react";
import TextField from "@mui/material/TextField";
import css from "./recipe-table-title.module.css";

/** renders the recipe table title component */
export function RecipeTableTitle({
  onChange,
  value,
  showSearch,
}: IRecipeTableTitleProps) {
  const inputId = useId();
  return (
    <Box>
      <Typography variant="h5" component="div">
        <div className={css.searchFormWrapper}>
          <span className={css.tableTitle}>Recipes</span>
          {showSearch && (
            <>
              <div className={css.flexGrow}></div>
              <TextField
                label="search"
                id={inputId}
                size="small"
                variant="outlined"
                value={value}
                onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                  onChange?.(event.target.value);
                }}
                slotProps={{
                  htmlInput: {
                    maxLength: 100,
                  },
                  input: {
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton type="button" aria-label="search">
                          <SearchIcon />
                        </IconButton>
                      </InputAdornment>
                    ),
                  },
                }}
              />
            </>
          )}
        </div>
      </Typography>
    </Box>
  );
}

/**
 * props for `RecipeTableTitle` component
 * @see RecipeTableTitle
 */
interface IRecipeTableTitleProps {
  /** fired when the input changes */
  onChange?: (s: string) => void;
  /** value for the form */
  value?: string;
  /** if the search feature is enabled */
  showSearch?: boolean;
}
