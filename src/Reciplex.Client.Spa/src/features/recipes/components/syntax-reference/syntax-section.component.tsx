import Accordion from "@mui/material/Accordion";
import AccordionDetails from "@mui/material/AccordionDetails";
import AccordionSummary from "@mui/material/AccordionSummary";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import type { ComponentChildren } from "preact";

export function SyntaxSection({ children, title }: ISyntaxSectionProps) {
  return (
    <Accordion>
      <AccordionSummary>
        <Typography variant="h5">{title}</Typography>
      </AccordionSummary>
      <AccordionDetails>
        <Stack>{children}</Stack>
      </AccordionDetails>
    </Accordion>
  );
}

export interface ISyntaxSectionProps {
  children?: ComponentChildren;
  title: string;
}
