import LibraryBooks from "@mui/icons-material/LibraryBooks";
import AutoAwesome from "@mui/icons-material/AutoAwesome";
import Restaurant from "@mui/icons-material/Restaurant";
import { useNavigate } from "react-router";
import { makeBookListPath } from "../../features/recipes/route-utils";
import { FeatureCard } from "./feature-card.component";
import Container from "@mui/material/Container";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";
import Button from "@mui/material/Button";
import Divider from "@mui/material/Divider";
import Grid from "@mui/material/Grid";

// Remark: This component is auto-generated with some minor hand edits.
//         Do not generalize it outside of the home page.
//         Source llm is gemma-4-26b-a4b.

export function HomePage() {
  const navigate = useNavigate();
  return (
    <Container maxWidth="lg" sx={{ py: 10 }}>
      <Box sx={{ textAlign: "center", mb: 8 }}>
        <Typography
          variant="h2"
          sx={{
            fontWeight: 800,
            mb: 2,
            fontSize: { xs: "2.5rem", md: "3.5rem" },
            letterSpacing: "-0.03em",
          }}
        >
          Reciplex
        </Typography>
        <Typography
          variant="h5"
          color="text.secondary"
          sx={{ maxWidth: "650px", mx: "auto", fontWeight: 400 }}
        >
          The intelligent way to collect, organize, and master your favorite
          recipes.
        </Typography>
        <Stack
          direction="row"
          spacing={2}
          justifyContent="center"
          sx={{ mt: 5 }}
        >
          <Button
            variant="contained"
            size="large"
            onClick={() => navigate(makeBookListPath())}
            sx={{
              borderRadius: 3,
              px: 4,
              py: 1.5,
              textTransform: "none",
              fontSize: "1.1rem",
            }}
          >
            Start your library
          </Button>
        </Stack>
      </Box>

      <Divider sx={{ mb: 10, opacity: 0.6 }} />

      <Grid container spacing={4}>
        <FeatureCard
          icon={<AutoAwesome sx={{ fontSize: 35 }} />}
          title="Smart Lists"
          description="Our intelligent system automatically combines ingredients across your recipes, creating a perfectly organized shopping list for you."
        />

        <FeatureCard
          icon={<Restaurant sx={{ fontSize: 35 }} />}
          title="Kitchen Ready"
          description="Never forget a tool again. Reciplex keeps track of everything from timers to stock pots, so you are always prepared for the next step."
        />

        <FeatureCard
          icon={<LibraryBooks sx={{ fontSize: 35 }} />}
          title="Personalized Books"
          description="Group your recipes into beautiful, custom collections—from 'Quick Weeknight Dinners' to your secret family holiday traditions."
        />
      </Grid>
    </Container>
  );
}
