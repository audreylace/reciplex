import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import { useMemo, useState } from "react";
import Tune from "@mui/icons-material/Tune";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import DialogContent from "@mui/material/DialogContent";
import Button from "@mui/material/Button";
import FormControlLabel from "@mui/material/FormControlLabel";
import FormLabel from "@mui/material/FormLabel";
import FormControl from "@mui/material/FormControl";
import Stack from "@mui/material/Stack";
import Autocomplete from "@mui/material/Autocomplete";
import TextField from "@mui/material/TextField";
import { useSearchRecipesAndBooks } from "../../hooks/useSearchRecipesAndBooks.hook";
import type { IConciseRecipeBookModel } from "../../services/recipe-types";
import RadioGroup from "@mui/material/RadioGroup";
import Radio from "@mui/material/Radio";

/**
 * Component that opens a search setting filter dialog
 */
export function FilterDialog({
  filterOptions,
  selectedFilterOption,
  onFilterChange,
}: IFilterDialogProps) {
  const [open, setOpen] = useState(false);
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("sm"));
  const [bookOptions, setBookOptions] = useState<
    { name: string; documentKey: string }[]
  >([]);
  const [inputValue, setInputValue] = useState<string>("");
  const searchQuery = useSearchRecipesAndBooks({
    searchString: inputValue,
    kind: ["book"],
  });

  const bookNames = useMemo(() => {
    const mapToBuild: Record<string, IConciseRecipeBookModel> = {};
    searchQuery.data?.books.forEach((b) => {
      mapToBuild[b.id] = b;
    });

    const results = [];
    for (const book of searchQuery.data?.hits ?? []) {
      if (book.kind === "book" && mapToBuild[book.documentKey]) {
        results.push({
          documentKey: book.documentKey,
          name: mapToBuild[book.documentKey].name,
        });
      }
    }

    return results;
  }, [searchQuery.data?.books, searchQuery.data?.hits]);

  return (
    <>
      <IconButton
        onClick={() => setOpen(true)}
        aria-label="Open search settings"
      >
        <Tune />
      </IconButton>
      <Dialog
        onClose={() => setOpen(false)}
        open={open}
        fullScreen={fullScreen}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Search Filters</DialogTitle>
        <DialogContent>
          <Stack spacing={4} sx={{ mt: 2 }}>
            <FormControl component="fieldset" variant="standard">
              <FormLabel component="legend">Include</FormLabel>
              <RadioGroup
                value={selectedFilterOption}
                onChange={(e) => onFilterChange(e.target.value)}
              >
                {filterOptions.map((option) => (
                  <FormControlLabel
                    key={option.id}
                    control={<Radio />}
                    label={option.caption}
                    value={option.id}
                  />
                ))}
              </RadioGroup>
            </FormControl>
            <Autocomplete
              multiple
              options={bookNames}
              getOptionLabel={(option) => option.name}
              value={bookOptions}
              onChange={(_, newValue) => {
                setBookOptions(newValue);
              }}
              onInputChange={(_, newInputValue) => {
                setInputValue(newInputValue);
              }}
              fullWidth
              filterOptions={(x) => x}
              renderInput={(params) => <TextField {...params} label="Books" />}
            />
            <Button onClick={() => setOpen(false)}>Close</Button>
          </Stack>
        </DialogContent>
      </Dialog>
    </>
  );
}

/**
 * Properties for `<FilterDialog />`
 */
export interface IFilterDialogProps {
  selectedFilterOption: string;
  /** filter options and their status */
  filterOptions: IFilterOptions[];
  /** fired when the option changes */
  onFilterChange(newValue: string): void;
}

/** set of filter options for the search dialog */
export interface IFilterOptions {
  /** caption of the filter option */
  caption: string;
  /** unique id identifying the option */
  id: string;
}
