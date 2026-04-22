import TextField from "@mui/material/TextField";
import { useRecipeListTableContext } from "./useRecipeListTableContext.hook";
import MenuItem from "@mui/material/MenuItem";

/**
 * Component for selecting the size of a page for the view recipe book page
 */
export function PageSizeSelect() {
  const selectedSize = useRecipeListTableContext((state) => state.size);
  const updateSize = useRecipeListTableContext((state) => state.updateSize);
  const textFieldOnChange = (event: Event) => {
    let parsedSize = parseInt((event.target as HTMLInputElement)?.value ?? "");
    if (isNaN(parsedSize)) {
      parsedSize = 10;
    }
    updateSize(parsedSize);
  };

  return (
    <TextField
      onChange={textFieldOnChange}
      select
      label="Count"
      defaultValue="10"
      value={selectedSize}
      size="small"
      sx={{
        width: "5em",
      }}
    >
      <MenuItem value={10}>10</MenuItem>
      <MenuItem value={20}>20</MenuItem>
      <MenuItem value={50}>50</MenuItem>
      <MenuItem value={100}>100</MenuItem>
    </TextField>
  );
}
