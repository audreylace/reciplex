import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import { RecipeTableTitle } from "./recipe-table-title.component";

/** component for rendering the recipe list table header */
export function RecipeTableHeader({
  hideHeader,
  searchOnChange,
  searchValue,
  showSearch,
}: IRecipeTableHeaderProps) {
  return (
    <TableHead>
      <TableRow>
        <TableCell colSpan={3}>
          <RecipeTableTitle
            onChange={searchOnChange}
            value={searchValue}
            showSearch={showSearch}
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
export interface IRecipeTableHeaderProps {
  /** if true, the table header is hidden */
  hideHeader?: boolean;
  /** fired when the search input changes */
  searchOnChange?: (s: string) => void;
  /** value for the search form */
  searchValue?: string;
  /** if the search feature is enabled */
  showSearch?: boolean;
}
