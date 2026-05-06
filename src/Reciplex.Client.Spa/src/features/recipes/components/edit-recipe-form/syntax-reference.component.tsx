import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import Accordion from "@mui/material/Accordion";
import AccordionDetails from "@mui/material/AccordionDetails";
import AccordionSummary from "@mui/material/AccordionSummary";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import Divider from "@mui/material/Divider";
import { SyntaxBlock } from "./syntax-block.component";
import { SyntaxItem } from "./syntax-item.component";

/**
 * A highly consistent, user-friendly syntax guide.
 * Uses a unified pattern for both Ingredients and Tools.
 */
export function SyntaxReference() {
  return (
    <Accordion
      disableGutters
      sx={{
        mt: 2,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 1,
      }}
    >
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
          Reciplex Syntax Guide
        </Typography>
      </AccordionSummary>

      <AccordionDetails sx={{ pt: 0 }}>
        <Stack spacing={3}>
          {/* SECTION 1: THE LEXER (The Foundation) */}
          <Box>
            <Typography variant="body1" gutterBottom>
              1. Core Grammar
            </Typography>
            <Typography variant="body2" sx={{ mb: 1 }}>
              All expressions must be wrapped in{" "}
              <code style={{ fontWeight: "bold" }}>((</code> and{" "}
              <code style={{ fontWeight: "bold" }}>))</code>.
            </Typography>
            <Stack spacing={1}>
              <SyntaxItem
                label="Strings"
                code='"quoted text"'
                desc="Used to define Names/Labels."
              />
              <SyntaxItem
                label="Atoms"
                code="BUTTER"
                desc="Unquoted identifiers used for Referencing."
              />
              <SyntaxItem
                label="Numbers"
                code="1, 0.5, 1/2"
                desc="Supports integers, decimals, and fractions."
              />
            </Stack>
          </Box>

          <Divider />

          {/* SECTION 2: INGREDIENTS */}
          <Box>
            <Typography variant="body1" gutterBottom>
              2. Ingredient Command (i)
            </Typography>
            <Typography variant="body2" sx={{ mb: 2, fontStyle: "italic" }}>
              Use for items that appear in your final ingredient list.
            </Typography>

            <Stack spacing={2}>
              {/* DEFINITION BLOCK */}
              <SyntaxBlock title="Defining an Ingredient">
                <code style={{ fontSize: "0.9rem" }}>
                  (( i 1 tbsp &quot;butter&quot; ))
                </code>
                <Typography
                  variant="caption"
                  display="block"
                  sx={{ mt: 0.5, opacity: 0.8 }}
                >
                  Uses quotes to set the name and quantity.
                </Typography>
              </SyntaxBlock>

              {/* BEHAVIOR CALLOUT */}
              <Box>
                <Typography variant="body2" display="block">
                  Auto-Merging
                </Typography>
                <Typography variant="caption" display="block">
                  Ingredients with the same name and unit auto-combine. Amounts
                  are summed up.
                </Typography>
              </Box>

              <Box>
                <Typography variant="body2" display="block">
                  Linkage (ID Referencing)
                </Typography>
                <Typography variant="caption" display="block">
                  Assign an ID to an ingredient to reuse it later without
                  re-defining it. <br />
                  <code style={{ fontSize: "0.8rem" }}>
                    (( i MyIngredient 1/2 &quot;cup&quot; &quot;ingredient&quot;
                    ))
                  </code>{" "}
                  &rarr;{" "}
                  <code style={{ fontSize: "0.8rem" }}>
                    {" "}
                    (( i MyIngredient &quot;optional override render text&quot;
                    ))
                  </code>
                </Typography>
              </Box>
            </Stack>
          </Box>

          <Divider />

          {/* SECTION 3: TOOLS */}
          <Box>
            <Typography variant="body1" gutterBottom>
              3. Tool Commands (t / tQ)
            </Typography>
            <Typography variant="body2" sx={{ mb: 2, fontStyle: "italic" }}>
              Use for physical equipment needed for the recipe.
            </Typography>

            <Stack spacing={2}>
              {/* TOOL TYPE T */}
              <SyntaxBlock title="Standard Tool (t) - Size-based">
                <code style={{ fontSize: "0.9rem" }}>
                  (( t 12 cup &quot;stock pot&quot; ))
                </code>
                <Typography
                  variant="caption"
                  display="block"
                  sx={{ mt: 0.5, opacity: 0.8 }}
                >
                  Format: [ID] [Size/Volume] [Name in Quotes] [Inline Text]
                </Typography>
              </SyntaxBlock>

              {/* TOOL TYPE TQ */}
              <SyntaxBlock title="Quantity Tool (tQ) - Count-based">
                <code style={{ fontSize: "0.9rem" }}>
                  (( tQ 3 &quot;spoons&quot; ))
                </code>
                <Typography
                  variant="caption"
                  display="block"
                  sx={{ mt: 0.5, opacity: 0.8 }}
                >
                  Format: [ID] [Amount] [Unit] [Name in Quotes] [Inline Text]
                </Typography>
              </SyntaxBlock>

              <Box>
                <Typography variant="body2" display="block">
                  Auto-Merging
                </Typography>
                <Typography variant="caption" display="block">
                  Tools with the same name, size, and unit are auto-combined.
                  The quantity is summed up.
                </Typography>
              </Box>

              {/* BEHAVIOR CALLOUT */}
              <Box>
                <Typography variant="body2" display="block">
                  Linkage (ID Referencing)
                </Typography>
                <Typography variant="caption" display="block">
                  Assign an ID to a tool to reuse it later without re-defining
                  it. <br />
                  <code style={{ fontSize: "0.8rem" }}>
                    (( t EGG_TIMER &quot;timer&quot; ))
                  </code>{" "}
                  &rarr;{" "}
                  <code style={{ fontSize: "0.8rem" }}>(( t EGG_TIMER ))</code>
                </Typography>
              </Box>
            </Stack>
          </Box>
        </Stack>
      </AccordionDetails>
    </Accordion>
  );
}
