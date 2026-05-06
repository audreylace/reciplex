import { useContext, useMemo } from "preact/hooks";
import { RecipeDetailsContext } from "./recipe-details-context.component";
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import Typography from "@mui/material/Typography";
import AccordionDetails from "@mui/material/AccordionDetails";
import TableContainer from "@mui/material/TableContainer";
import Table from "@mui/material/Table";
import TableRow from "@mui/material/TableRow";
import TableHead from "@mui/material/TableHead";
import TableCell from "@mui/material/TableCell";
import TableBody from "@mui/material/TableBody";

export function IngredientList() {
  const detailsContext = useContext(RecipeDetailsContext);

  const list = useMemo(() => {
    if (!detailsContext?.ingredientCollection) {
      return null;
    }

    return detailsContext.ingredientCollection.getIngredientList();
  }, [detailsContext?.ingredientCollection]);

  if (!list || list.length < 1) {
    return null;
  }

  return (
    <>
      <Accordion>
        <AccordionSummary>
          <Typography variant="h5">Ingredients</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Unit</TableCell>
                  <TableCell>Amount</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {list.map((listEntry) => {
                  return (
                    <TableRow key={listEntry.key}>
                      <TableCell>{listEntry.title}</TableCell>
                      <TableCell>
                        {listEntry.unit ? listEntry.unit : "-"}
                      </TableCell>
                      <TableCell>{listEntry.amount}</TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>
        </AccordionDetails>
      </Accordion>
    </>
  );
}
