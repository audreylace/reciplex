import { useContext, useMemo } from "react";
import { RecipeDetailsContext } from "./recipe-details-context.component";
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import Typography from "@mui/material/Typography";
import TableContainer from "@mui/material/TableContainer";
import AccordionDetails from "@mui/material/AccordionDetails";
import TableCell from "@mui/material/TableCell";
import Table from "@mui/material/Table";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TableBody from "@mui/material/TableBody";

export function ToolList() {
  const detailsContext = useContext(RecipeDetailsContext);

  const toolCollection = detailsContext?.toolCollection;
  const list = useMemo(() => {
    if (!toolCollection) {
      return null;
    }

    return toolCollection.getToolList();
  }, [toolCollection]);

  if (!list || list.length < 1) {
    return null;
  }

  return (
    <Accordion>
      <AccordionSummary>
        <Typography variant="h5">Tools</Typography>
      </AccordionSummary>
      <AccordionDetails>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Size</TableCell>
                <TableCell>Quantity</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {list.map((listEntry) => {
                let unitString = "-";
                if (listEntry.sizeUnit && listEntry.size) {
                  unitString = `${listEntry.size} ${listEntry.sizeUnit}`;
                }
                return (
                  <TableRow key={listEntry.key}>
                    <TableCell>{listEntry.toolName}</TableCell>
                    <TableCell>{unitString}</TableCell>
                    <TableCell>{listEntry.quantity}</TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </TableContainer>
      </AccordionDetails>
    </Accordion>
  );
}
