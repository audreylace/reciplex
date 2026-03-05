import { Input } from "@headlessui/react";
import {
  RecipeCstVisitor,
  RecipeExpressionLexer,
  RecipeParser,
} from "../../features/recipes/utils/recipe-expressions/lexer2";
import {
  RecipeExpressionLexer3,
  RecipeExpressionParser,
} from "../../features/recipes/utils/recipe-expressions/lexer3";

/**
 * Entry point for home page component
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function HomePage() {
  return (
    <>
      <h1>Welcome to Reciplex!</h1>
      <p>Version: Alpha A</p>
      <p>
        <Input
          style={{ width: "100%" }}
          onChange={(e) => {
            if (e.target && e.currentTarget.value) {
              const lexingResult = RecipeExpressionLexer3.tokenize(
                e.currentTarget.value,
              );
              console.log(lexingResult);

              const parser = new RecipeExpressionParser();
              parser.input = lexingResult.tokens;

              const cst = parser.recipeTextWithExpressions();

              console.log(cst);

              // const ast = new RecipeCstVisitor().visit(cst);
              // console.log(ast);
            }
          }}
        />
      </p>
    </>
  );
}
