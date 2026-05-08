import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { makeCreateRecipeBookPath } from "../../route-utils";
import { useNavigate } from "react-router";

export function BookTableHeader({ pending }: IBookTableHeaderProps) {
  const navigate = useNavigate();
  return (
    <TableHead>
      <TableRow>
        <TableCell colSpan={3}>
          <Stack direction={"row"} sx={{ width: "100%" }}>
            <Box sx={{ flex: "1 1 auto" }}>
              <Typography variant="h5" component="div">
                Books
              </Typography>
            </Box>
            <Button onClick={() => navigate(makeCreateRecipeBookPath())}>
              Add
            </Button>
          </Stack>
        </TableCell>
      </TableRow>
      {!pending && (
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
 * props for `BookTableHeader`
 * @see IBookTableHeaderProps
 */
export interface IBookTableHeaderProps {
  /** true when data is loading */
  pending?: boolean;
}
