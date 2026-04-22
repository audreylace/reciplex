import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";

export function RecipeTableTitle() {
  return (
    <Box>
      <Typography
        variant="h5"
        component="div"
        sx={{
          pt: 2,
          pl: 2,
        }}
      >
        Recipes
      </Typography>
    </Box>
  );
}
