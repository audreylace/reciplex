import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import InputAdornment from "@mui/material/InputAdornment";
import Typography from "@mui/material/Typography";
import SearchIcon from "@mui/icons-material/Search";
import { useEffect, useId, useRef } from "react";
import TextField from "@mui/material/TextField";
import css from "./recipe-table-title.module.css";

/** renders the recipe table title component */
export function RecipeTableTitle({
  searchValue,
  setSearchValue,
  showSearch,
  focusSearch,
}: IRecipeTableTitleProps) {
  const inputId = useId();
  const textRef = useRef<HTMLInputElement>(null);
  const firstFocusRef = useRef(true);
  useEffect(() => {
    if (firstFocusRef.current && textRef.current && focusSearch) {
      textRef.current.focus();
    }
    firstFocusRef.current = false;
  }, [focusSearch]);
  return (
    <Box>
      <Typography variant="h5" component="div">
        <div className={css.searchFormWrapper}>
          <span className={css.tableTitle}>Recipes</span>
          {showSearch && (
            <>
              <div className={css.flexGrow}></div>
              <TextField
                inputRef={textRef}
                label="search"
                id={inputId}
                size="small"
                variant="outlined"
                value={searchValue}
                onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                  setSearchValue?.(event.target.value);
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
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
interface IRecipeTableTitleProps extends IRecipeTableSearchProps {}

/** props specific to searching */
export interface IRecipeTableSearchProps {
  /** if the search feature is enabled */
  showSearch: boolean;
  /** the value for the search box */
  searchValue: string;
  /** invoked when a value is typed into the search box */
  setSearchValue: (value: string) => void;
  /** if we should focus search */
  focusSearch?: boolean;
}
