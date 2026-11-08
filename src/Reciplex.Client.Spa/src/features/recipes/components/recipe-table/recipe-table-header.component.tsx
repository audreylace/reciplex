import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import {
  RecipeTableTitle,
  type IRecipeTableSearchProps,
} from "./recipe-table-title.component";

/** component for rendering the recipe list table header */
export function RecipeTableHeader({
  hideHeader,
  setSearchValue,
  searchValue,
  showSearch,
  focusSearch,
}: IRecipeTableHeaderProps) {
  return (
    <TableHead>
      <TableRow>
        <TableCell colSpan={3}>
          <RecipeTableTitle
            setSearchValue={setSearchValue}
            searchValue={searchValue}
            showSearch={showSearch}
            focusSearch={focusSearch}
          />
        </TableCell>
      </TableRow>
      {!hideHeader && (
        <TableRow>
          <TableCell>Name</TableCell>
          <TableCell>Description</TableCell>
          <TableCell padding="checkbox"></TableCell>
        </TableRow>
      )}
    </TableHead>
  );
}

/**
 * props for `RecipeTableHeader`
 * @see RecipeTableHeader
 */
export interface IRecipeTableHeaderProps extends IRecipeTableSearchProps {
  /** if true, the table header is hidden */
  hideHeader?: boolean;
}
