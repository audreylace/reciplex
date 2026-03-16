/**
 * @fileoverview Syntax for recipe expression
 */

import {
  isAtomToken,
  isClosingToken,
  isFractionToken,
  isNumberLikeToken,
  isOpeningToken,
  isOutsideLikeToken,
  isQuotedStringToken,
  isTextLikeToken,
  isTextLiteralToken,
  recipeExpressionLexer,
  type ReciplexExpressionToken,
  type RExpAtomToken,
  type RExpFractionToken,
  type RExpNumberToken,
  type RExpQuotedStringToken,
  type RExpTextLiteralToken,
} from "./expression-lexer";

/**
 * A expression command
 */
export interface IRExpCommand<TCommandTag extends string> {
  /**
   * the tag
   */
  tag: TCommandTag;
}

/**
 * An echo command
 */
export interface IRExpEchoCommand extends IRExpCommand<"echo"> {
  /**
   * the text to write
   */
  text: string;
}

export type RExpCommand =
  | IRExpEchoCommand
  | IRExpIngredientCommand
  | IRExpToolCommand;

type ArgumentTokens =
  | RExpQuotedStringToken
  | RExpNumberToken
  | RExpFractionToken
  | RExpTextLiteralToken;

/**
 * Command handlers take the raw atom expression and convert them into
 * models.
 */
interface ICommandParser<
  TCommandTag extends string,
  TCommand extends IRExpCommand<TCommandTag>,
> {
  /**
   * Selector function matches against the atom token.
   * @param atom the atom
   * @returns true if this command handler wants to try to handle the atom
   */
  selector: (atom: RExpAtomToken) => boolean;

  /**
   * Extracts arguments from a atom
   * @param atom the atom
   * @param args the args
   * @returns Returns `null` or `TCommand`. `false` if the next handler should be tried.
   */
  argumentExtractor: (
    atom: RExpAtomToken,
    args: ArgumentTokens[],
  ) => null | TCommand | false;
}

/**
 * Handles an echo command
 */
const echoCommandParser: ICommandParser<"echo", IRExpEchoCommand> = {
  selector: (atom) => {
    const atomValue = atom.payload.text;
    switch (atomValue) {
      case "e":
      case "echo":
        return true;
      default:
        return false;
    }
  },
  argumentExtractor: (_atom, args) => {
    if (args.length !== 1 || !isQuotedStringToken(args[0])) {
      return null;
    }

    return { text: args[0].payload.text, tag: "echo" };
  },
};

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
  quantity?: {
    /**
     * the raw amount
     */
    amount: number;
    /**
     * the unit
     */
    unit: string;
    /**
     * The fraction used to compute `amount` if applicable
     */
    fraction?: { numerator: number; denominator: number };
  };
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
  /**
   * Set to true to disable combining. Otherwise this should be left at undefined.
   */
  doNotCombine: boolean;
}

/**
 * Tool command. Has the same syntax as the ingredient command.
 * @see IRExpIngredientCommand
 */
export interface IRExpToolCommand
  extends Omit<IRExpIngredientCommand, "tag">, IRExpCommand<"tool"> {}

/**
 * Handles a recipe and tool command
 */
const ingredientCommandParser: ICommandParser<
  "ingredient" | "tool",
  IRExpIngredientCommand | IRExpToolCommand
> = {
  selector: (atom) => {
    const atomValue = atom.payload.text;
    switch (atomValue) {
      case "i":
      case "ingredient":
      case "ingredientUnique":
      case "iUnique":
        return true;

      case "t":
      case "tool":
      case "tUnique":
      case "toolUnique":
        return true;

      default:
        return false;
    }
  },
  argumentExtractor: (atom, args) => {
    if (args.length < 1) {
      return null;
    }

    const doNotCombine =
      atom.payload.text === "ingredientUnique" ||
      atom.payload.text === "iUnique" ||
      atom.payload.text === "tUnique" ||
      atom.payload.text === "toolUnique";

    let expressionTokenPosition = 0;

    let userSuppliedIdProperty: IRExpIngredientCommand["userSuppliedId"];
    let nextToken = args[expressionTokenPosition];
    if (isTextLiteralToken(nextToken)) {
      userSuppliedIdProperty = nextToken.payload.text;
      expressionTokenPosition++;
    }

    let quantityProperty: IRExpIngredientCommand["quantity"] = undefined;
    nextToken = args[expressionTokenPosition];
    if (isNumberLikeToken(nextToken)) {
      const unitToken = args[expressionTokenPosition + 1];
      if (!isTextLikeToken(unitToken)) {
        return null;
      }
      if (isFractionToken(nextToken)) {
        quantityProperty = {
          amount: nextToken.payload.realValue,
          unit: unitToken.payload.text,
          fraction: {
            numerator: nextToken.payload.numerator,
            denominator: nextToken.payload.denominator,
          },
        };
      } else {
        quantityProperty = {
          amount: nextToken.payload.realValue,
          unit: unitToken.payload.text,
        };
      }

      expressionTokenPosition += 2;
    }

    let ingredientNameProperty: IRExpIngredientCommand["ingredientName"];
    let inlineTextProperty: IRExpIngredientCommand["inlineText"];
    const remainingTokens = args.slice(expressionTokenPosition);
    if (
      remainingTokens.length === 2 &&
      isQuotedStringToken(remainingTokens[0]) &&
      isQuotedStringToken(remainingTokens[1])
    ) {
      ingredientNameProperty = remainingTokens[0].payload.text;
      inlineTextProperty = remainingTokens[1].payload.text;
    } else if (
      remainingTokens.length === 1 &&
      isQuotedStringToken(remainingTokens[0])
    ) {
      if (userSuppliedIdProperty) {
        inlineTextProperty = remainingTokens[0].payload.text;
      } else {
        ingredientNameProperty = remainingTokens[0].payload.text;
      }
    } else if (remainingTokens.length === 0 && userSuppliedIdProperty) {
      // do nothing
    } else {
      return null;
    }

    let tag: "ingredient" | "tool";
    switch (atom.payload.text) {
      case "t":
      case "tUnique":
      case "toolUnique":
      case "tool":
        tag = "tool";
        break;
      case "i":
      case "ingredient":
      case "ingredientUnique":
      case "iUnique":
        tag = "ingredient";
        break;
      default:
        return null;
    }

    return {
      tag: tag,
      quantity: quantityProperty,
      ingredientName: ingredientNameProperty,
      inlineText: inlineTextProperty,
      userSuppliedId: userSuppliedIdProperty,
      doNotCombine: doNotCombine,
    };
  },
};

export interface IOutsideTextTokens {
  type: "outside-text";
  tokens: [ReciplexExpressionToken, ...ReciplexExpressionToken[]];
}

export interface ICommandTokens {
  type: "command";
  tokens: [ReciplexExpressionToken, ...ReciplexExpressionToken[]];
  command?: RExpCommand; // undefined if the expression did not map to any commands or is invalid
}

export function extractExpressionFromTokens(
  tokens: ReciplexExpressionToken[],
): (IOutsideTextTokens | ICommandTokens)[] {
  if (tokens.length === 0) {
    return [];
  }

  const result: ReturnType<typeof extractExpressionFromTokens> = [];
  let position = 0;
  while (position < tokens.length) {
    const outsideTokens = extractOutsideTextSegment(tokens, position);
    if (outsideTokens) {
      result.push(outsideTokens);
      position += outsideTokens.tokens.length;
    }

    const commandTokens = extractCommandSegment(tokens, position);
    if (commandTokens === null) {
      break;
    }

    result.push(commandTokens);
    position += commandTokens.tokens.length;
  }

  return result;
}

function extractOutsideTextSegment(
  tokens: ReciplexExpressionToken[],
  startOffset: number,
): IOutsideTextTokens | null {
  let endOffset = startOffset;
  while (isOutsideLikeToken(tokens[endOffset])) {
    endOffset++;
  }

  if (endOffset !== startOffset) {
    return {
      type: "outside-text",
      tokens: tokens.slice(startOffset, endOffset) as [
        ReciplexExpressionToken,
        ...ReciplexExpressionToken[],
      ],
    };
  }

  return null;
}

function extractCommandSegment(
  tokens: ReciplexExpressionToken[],
  startOffset: number,
): ICommandTokens | null {
  let endOffset = startOffset;
  if (!isOpeningToken(tokens[startOffset])) {
    return null;
  }

  while (!isClosingToken(tokens[endOffset]) && endOffset < tokens.length) {
    endOffset++;
  }

  if (!isClosingToken(tokens[endOffset])) {
    return null;
  }

  endOffset++; // have to increment by 1 to pull in end token

  const expressionTokens = tokens.slice(startOffset, endOffset) as [
    ReciplexExpressionToken,
    ...ReciplexExpressionToken[],
  ];
  const tokensOfInterest = expressionTokens.filter(
    (t) => isAtomToken(t) || isNumberLikeToken(t) || isTextLikeToken(t),
  );

  if (tokensOfInterest.length < 1 || !isAtomToken(tokensOfInterest[0])) {
    return {
      type: "command",
      tokens: expressionTokens,
    };
  }

  const atomToken = tokensOfInterest[0];
  const atomArgs = tokensOfInterest.slice(1);

  if (!isArrayExpressionArgs(atomArgs)) {
    return {
      type: "command",
      tokens: expressionTokens,
    };
  }

  for (const parser of [echoCommandParser, ingredientCommandParser]) {
    if (parser.selector(atomToken)) {
      const result = parser.argumentExtractor(atomToken, atomArgs);
      if (result !== false) {
        return {
          type: "command",
          tokens: expressionTokens,
          command: result ?? undefined,
        };
      }
    }
  }

  return {
    type: "command",
    tokens: expressionTokens,
  };
}

function isArrayExpressionArgs(
  args: ReciplexExpressionToken[],
): args is ArgumentTokens[] {
  return args.every((t) => {
    if (isNumberLikeToken(t) || isTextLikeToken(t)) {
      return true;
    }

    return false;
  });
}

/**
 * Parses text into a set of recipe expressions
 * @param freeText the free text to lex and parse
 * @returns the set of expression models extracted from the text
 */
export function parseTextForRecipeExpression(
  freeText: string,
): ReturnType<typeof extractExpressionFromTokens> | null {
  const lexerTokens = recipeExpressionLexer.tokenize(freeText);
  if (lexerTokens.errors.length > 0) {
    return null;
  }
  return extractExpressionFromTokens(
    lexerTokens.tokens as ReciplexExpressionToken[],
  );
}
