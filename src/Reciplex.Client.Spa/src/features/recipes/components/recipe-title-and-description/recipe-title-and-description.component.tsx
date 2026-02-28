import { useLocation } from "react-router";
import styles from "./recipe-title-and-description.module.css";
import type { PropsWithChildren } from "preact/compat";

export function RecipeNameAndDescription({
  name,
  shortDescription,
  children,
}: PropsWithChildren<RecipeNameAndDescriptionProps>) {
  const location = useLocation();
  const { recipeName, recipeShortDescription } = location?.state ?? {};
  const finalName = name ?? recipeName;
  const finalShortDescription = shortDescription ?? recipeShortDescription;

  if (!finalName && !finalShortDescription) {
    return null;
  }

  return (
    <div className={styles.header}>
      <h1>{finalName ?? ""}</h1>
      <p>{finalShortDescription ?? ""}</p>
      {children}
    </div>
  );
}

export interface RecipeNameAndDescriptionProps {
  name?: string;
  shortDescription?: string;
}

export function makeRecipeNameAndDescriptionState(
  recipeName?: string,
  recipeShortDescription?: string,
) {
  return {
    recipeName,
    recipeShortDescription,
  };
}
