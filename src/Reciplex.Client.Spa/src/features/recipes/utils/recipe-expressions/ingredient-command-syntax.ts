import { isTextLiteralToken } from "./expression-lexer";
import type {
  ICommandParser,
  IQuantityWithUnit,
  IRExpCommand,
} from "./expression-syntax-types";
import { parseQuantityWithUnit, parseStringTuple } from "./parse-common";

/**
 * Reciplex ingredient command.
 *
 * The following syntaxes map to this:
 * - inline short with quantity: `((i id?[ literal ] Amount[ Number | Integer | Fraction ] Unit[ literal | quoted ] listText?[ quoted ] inlineText [ quoted ]))`
 * - inline short no quantity: `((i id?[ literal ] listText?[ quoted ] inlineText[ quoted ] ))`
 *
 * Some examples:
 * - Defined inline with custom text: (( i BUTTER 1 tbsp "butter" "1/2 tsp of butter")) on each slice of bread
 * - Link to previously defined ingredient: Heat the (( i BUTTER "butter" )) for 10 seconds in the microwave to soften if cold
 * - Anonymous ingredient with no quantity: Goes well with (( i "milk" )) on the side. Make sure to pre-heat the (( i "milk" )).
 *
 * Note: If two ingredients have same `ingredientName` and unit then they are auto combined even if they have a different unique id.
 * The amounts will be auto-added together. So for example: Mix (( i 1 tsbp "soy sauce")) into the topping vat. Additionally, mix in (( i 1 tsbp "soy sauce" ))
 * into the noodle vat. Will result in an ingredient in the list named "soy sauce" with quantity 2 tsbp. To disable this behavior change the atom to ingredientUnique.
 *
 * So the same sentence written this way would result in two separate soy sauce entries: Mix (( ingredientUnique 1 tsbp "soy sauce"))
 * into the topping vat. Additionally, mix in (( ingredientUnique 1 tsbp "soy sauce" )) into the noodle vat.
 *
 * Unique respects the id property so if they both had the same ID then they would be collapsed down. `ingredientUnique` atom just disables
 * the auto-deduplication behavior.
 *
 * Ingredient commands with variables are merged with the last instance winning. So (( i SOY 1 tsbp "soy sauce" )) followed by (( i SOY 2 tsbp "green soy" )) would redefine
 * `SOY` to be 2 tsbp. However, the list text would be "soy sauce". To make it "green soy" then the second expression would need to include both local text and list text.
 */
export interface IRExpIngredientCommand extends IRExpCommand<"ingredient"> {
  /**
   * the ingredient quantity if applicable
   */
  quantity?: IQuantityWithUnit;
  /**
   * The name of the ingredient as displayed in the ingredient list
   */
  ingredientName?: string;
  /**
   * Custom text to print for this ingredient inline
   */
  inlineText?: string;
  /**
   * The unique supplied id of this ingredient
   */
  userSuppliedId?: string;
}

/**
 * Handles a recipe and tool command
 */
export const ingredientCommandParser: ICommandParser<
  "ingredient",
  IRExpIngredientCommand
> = {
  selector: (atom) => {
    const atomValue = atom.payload.text;
    switch (atomValue) {
      case "i":
      case "ingredient":
        return true;

      default:
        return false;
    }
  },
  argumentExtractor: (_atom, args) => {
    if (args.length < 1) {
      return null;
    }

    let expressionTokenPosition = 0;

    let userSuppliedIdProperty: IRExpIngredientCommand["userSuppliedId"];
    let nextToken = args[expressionTokenPosition];
    if (isTextLiteralToken(nextToken)) {
      userSuppliedIdProperty = nextToken.payload.text;
      expressionTokenPosition++;
    }

    let quantityProperty: IRExpIngredientCommand["quantity"] = undefined;
    nextToken = args[expressionTokenPosition];
    const parseQuantityResult = parseQuantityWithUnit(
      args,
      expressionTokenPosition,
    );
    if (parseQuantityResult) {
      expressionTokenPosition = parseQuantityResult[0];
      quantityProperty = parseQuantityResult[1];
    }

    const stringTuple = parseStringTuple(args, expressionTokenPosition);
    let ingredientNameProperty: IRExpIngredientCommand["ingredientName"];
    let inlineTextProperty: IRExpIngredientCommand["inlineText"];
    if (stringTuple?.length === 3) {
      ingredientNameProperty = stringTuple[1];
      inlineTextProperty = stringTuple[2];
    } else if (stringTuple?.length === 2) {
      if (userSuppliedIdProperty) {
        inlineTextProperty = stringTuple[1];
      } else {
        ingredientNameProperty = stringTuple[1];
      }
    }

    expressionTokenPosition = stringTuple?.[0] ?? expressionTokenPosition;
    if (expressionTokenPosition !== args.length) {
      // EXPECT: End on last token
      return false;
    }

    return {
      tag: "ingredient",
      quantity: quantityProperty,
      ingredientName: ingredientNameProperty,
      inlineText: inlineTextProperty,
      userSuppliedId: userSuppliedIdProperty,
    };
  },
};
