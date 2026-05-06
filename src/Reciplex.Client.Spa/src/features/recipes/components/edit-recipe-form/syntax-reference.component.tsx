import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import Accordion from "@mui/material/Accordion";
import AccordionDetails from "@mui/material/AccordionDetails";
import AccordionSummary from "@mui/material/AccordionSummary";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";

/**
 * A collapsible syntax guide.
 * Uses an Accordion to keep the form clean while providing instruction.
 */
export function SyntaxReference() {
  return (
    <Accordion
      disableGutters
      sx={{ mt: 2, border: "1px solid", borderColor: "divider" }}
    >
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
          Syntax Help
        </Typography>
      </AccordionSummary>

      <AccordionDetails sx={{ pt: 0 }}>
        <Stack spacing={2}>
          <Box>
            <Typography
              variant="caption"
              sx={{ fontWeight: "bold", display: "block" }}
            >
              Ingredients (i)
            </Typography>
            <Typography
              variant="body2"
              component="code"
              sx={{ display: "block", mb: 0.5 }}
            >
              (( i BUTTER 1 tbsp ))
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Format: [type] [name] [amount] [unit] [&quot;optional custom text
              to render instead&quot;]
            </Typography>
          </Box>

          <Box>
            <Typography
              variant="caption"
              sx={{ fontWeight: "bold", display: "block" }}
            >
              Tools (t)
            </Typography>
            <Typography
              variant="body2"
              component="code"
              sx={{ display: "block", mb: 0.5 }}
            >
              (( t 12 cup &quot;pot&quot;))
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Format: [type] [size] [name] [&quot;optional custom text to render
              instead&quot;]
            </Typography>
          </Box>

          <Box>
            <Typography
              variant="caption"
              sx={{ fontWeight: "bold", display: "block" }}
            >
              Tools with quantity (tQ)
            </Typography>
            <Typography
              variant="body2"
              component="code"
              sx={{ display: "block", mb: 0.5 }}
            >
              (( tQ 3 &quot;spoons&quot; ))
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Format: [type] [quantity] [name] [&quot;optional custom text to
              render instead&quot;]
            </Typography>
          </Box>
          <Box sx={{ pt: 1, borderTop: "1px dashed", borderColor: "divider" }}>
            <Typography
              variant="caption"
              sx={{ fontWeight: "bold", display: "block", mb: 1 }}
            >
              Referencing (Linking)
            </Typography>
            <Typography
              variant="body2"
              component="code"
              sx={{ display: "block", mb: 0.5 }}
            >
              Define: (( i salt 1/2 tbsp )) &rarr; Use: (( i salt ))
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Use a text identifier to reuse previously defined items.
            </Typography>
          </Box>
        </Stack>
      </AccordionDetails>
    </Accordion>
  );
}
