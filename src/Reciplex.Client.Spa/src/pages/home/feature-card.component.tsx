import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Typography from "@mui/material/Typography";
import type { ComponentChild } from "preact";

// Remark: This component is auto-generated with some minor hand edits.
//         Do not generalize it outside of the home page.
//         Source llm is gemma-4-26b-a4b.

export const FeatureCard = ({
  icon,
  title,
  description,
}: IFeatureCardProps) => (
  <Card
    sx={{
      height: "100%",
      width: "100%",
      display: "flex",
      flexDirection: "column",
      boxShadow: "0 2px 10px rgba(0,0,0,0.05)",
      transition: "transform 0.2s",
      "&:hover": { transform: "translateY(-4px)" },
    }}
  >
    <CardContent sx={{ flexGrow: 1 }}>
      <Box sx={{ color: "primary.main", mb: 2 }}>
        {icon}
        <Typography
          variant="h6"
          component="div"
          sx={{ mt: 1, fontWeight: 700 }}
        >
          {title}
        </Typography>
      </Box>
      <Typography variant="body2" color="text.secondary" lineHeight={1.6}>
        {description}
      </Typography>
    </CardContent>
  </Card>
);

export interface IFeatureCardProps {
  icon: ComponentChild;
  title: ComponentChild;
  description: ComponentChild;
}
