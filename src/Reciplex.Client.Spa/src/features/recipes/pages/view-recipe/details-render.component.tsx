import { useContext, useMemo } from "preact/hooks";
import { RecipeDetailsContext } from "./recipe-details-context.component";
import { usePipeline } from "./usePipeline.hook";
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";

const componentMap = {
  recipeIngredientExpression: RecipeIngredientRender,
  recipeToolExpression: RecipeToolRender,
};

/** renders the recipe details */
export function DetailsRender({
  detailsMd,
  mayEdit,
  goToEditAction,
}: IDetailsRenderProps) {
  const detailsContextModel = usePipeline(detailsMd, componentMap);

  return (
    <>
      <RecipeDetailsContext.Provider value={detailsContextModel}>
        <ToolList />
        <IngredientList />

        <Paper sx={{ p: 2, mt: 2 }}>
          <Typography variant="h5">Details</Typography>

          <DetailsMdRender mayEdit={mayEdit} goToEditAction={goToEditAction} />
        </Paper>
      </RecipeDetailsContext.Provider>
    </>
  );
}

/** props for  `DetailsRender` */
export interface IDetailsRenderProps {
  detailsMd?: string;
  mayEdit: boolean;
  goToEditAction: () => void;
}

function IngredientList() {
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

function ToolList() {
  const detailsContext = useContext(RecipeDetailsContext);

  const list = useMemo(() => {
    if (!detailsContext?.toolCollection) {
      return null;
    }

    return detailsContext.toolCollection.getToolList();
  }, [detailsContext?.toolCollection]);

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

/** inner md render */
function DetailsMdRender({
  mayEdit,
  goToEditAction,
}: {
  mayEdit: IDetailsRenderProps["mayEdit"];
  goToEditAction: IDetailsRenderProps["goToEditAction"];
}) {
  const detailsContext = useContext(RecipeDetailsContext);
  if (!detailsContext?.component) {
    if (!mayEdit) {
      return (
        <Typography variant="body2">
          <i>No details</i>
        </Typography>
      );
    }
    return (
      <Typography
        variant="body2"
        onClick={goToEditAction}
        sx={{
          cursor: "pointer",
        }}
      >
        <i>Click to edit and add details</i>
      </Typography>
    );
  }

  return detailsContext?.component;
}

function RecipeIngredientRender({ position }: { position: number }) {
  const detailsContext = useContext(RecipeDetailsContext);

  if (!detailsContext) {
    return null;
  }

  const concept =
    detailsContext.ingredientCollection.getConceptByPosition(position);
  if (!concept) {
    return null;
  }

  const inlineText = concept.getRenderText(position);
  const unit = concept.unitText;
  const amount = concept.getAmountByPosition(position);

  if (amount) {
    return (
      <span>
        {amount} {unit} {inlineText}
      </span>
    );
  }

  return <span>{inlineText}</span>;
}

function RecipeToolRender({ position }: { position: number }) {
  const detailsContext = useContext(RecipeDetailsContext);

  if (!detailsContext) {
    return null;
  }

  const concept = detailsContext.toolCollection.getConceptByPosition(position);
  if (!concept) {
    return null;
  }

  const toolName = concept.toolName;
  const inlineText = concept.getInlineText(position);
  const toolSizeUnit = concept.toolSizeUnit;
  const toolSize = concept.toolSize;
  const toolQuantity = concept.getToolQuantityByPosition(position);

  if (inlineText) {
    return <span>{inlineText}</span>;
  }

  return (
    <span>
      {toolSize} {toolSizeUnit} {toolQuantity !== 1 ? "x" + toolQuantity : ""}
      {toolName}
    </span>
  );
}
