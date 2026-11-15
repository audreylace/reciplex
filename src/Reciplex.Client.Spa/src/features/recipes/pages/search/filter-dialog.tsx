import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import { useMemo, useState } from "react";
import Tune from "@mui/icons-material/Tune";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import DialogContent from "@mui/material/DialogContent";
import Button from "@mui/material/Button";
import FormGroup from "@mui/material/FormGroup";
import FormControlLabel from "@mui/material/FormControlLabel";
import Checkbox from "@mui/material/Checkbox";
import FormLabel from "@mui/material/FormLabel";
import FormControl from "@mui/material/FormControl";
import Stack from "@mui/material/Stack";
import Autocomplete from "@mui/material/Autocomplete";
import TextField from "@mui/material/TextField";
import { useSearchRecipesAndBooks } from "../../hooks/useSearchRecipesAndBooks.hook";
import type { IRecipeBookModel } from "../../services/recipe-types";

/**
 * Component that opens a search setting filter dialog
 */
export function FilterDialog({}: IFilterDialogProps) {
  const [open, setOpen] = useState(false);
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("sm"));
  const [bookOptions, setBookOptions] = useState<string[]>([]);
  const [inputValue, setInputValue] = useState<string>("");
  const searchQuery = useSearchRecipesAndBooks({
    searchString: inputValue,
    kind: ["book"],
  });

  const bookNames = useMemo(() => {
    const mapToBuild: Record<string, IRecipeBookModel> = {};
    searchQuery.data?.books.forEach((b) => {
      mapToBuild[b.id] = b;
    });

    const results = [];
    for (const book of searchQuery.data?.hits ?? []) {
      if (book.kind === "book" && mapToBuild[book.documentKey]) {
        results.push(mapToBuild[book.documentKey].name);
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
      >
        <DialogTitle>Search Filters</DialogTitle>
        <DialogContent>
          <Stack>
            <FormControl component="fieldset" variant="standard">
              <FormLabel component="legend">Include</FormLabel>
              <FormGroup>
                <FormControlLabel control={<Checkbox />} label="Recipes" />
                <FormControlLabel control={<Checkbox />} label="Books" />
              </FormGroup>
            </FormControl>
            <Autocomplete
              multiple
              options={bookNames}
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
export interface IFilterDialogProps {}

const opts = ["a", "b", "c", "d", "e"];
