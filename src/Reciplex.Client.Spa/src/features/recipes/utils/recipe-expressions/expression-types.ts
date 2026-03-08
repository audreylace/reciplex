import type { IToken } from "chevrotain";
import {
  RecipeExpressionLexer,
  S_SquaredAtomToken,
  S_SquaredClosingToken,
  S_SquaredDecimalToken,
  S_SquaredFractionToken,
  S_SquaredIntegerToken,
  S_SquaredOpeningToken,
  S_SquaredQuotedStringToken,
  S_SquaredTextLiteralToken,
  S_SquaredWhitespaceToken,
  type S_SquaredRealValuePayload,
  type S_SquaredTextValuePayload,
} from "./expression-lexer";

/**
 * Helper for changing `IToken.payload` from `any` to `T`.
 */
type ITokenWithPayload<T> = Omit<IToken, "payload"> & { payload: T };

/**
 * the set of tags identifying each recipe model
 */
export type RecipeExpressionModelTags =
  | "ingredient-freeform"
  | "ingredient-with-unit"
  | "expression-echo";

/**
 * the set of recipe expression models
 */
export type RecipeExpressionModels =
  | RecipeIngredientFreeformModel
  | RecipeIngredientWithUnitModel
  | RecipeEchoModel;

/**
 * An ingredient without units
 */
export interface RecipeIngredientFreeformModel extends RecipeExpressionModel {
  /**
   * the ingredient name
   */
  text: string;
  /**
   * @inheritdoc
   */
  tag: "ingredient-freeform";
}

/**
 * An ingredient with a unit
 */
export interface RecipeIngredientWithUnitModel extends RecipeExpressionModel {
  /**
   * the ingredient name
   */
  text: string;
  /**
   * the number of ingredient units
   */
  amount: number;
  /**
   * the unit of measurement
   */
  unit: string;
  /**
   * @inheritdoc
   */
  tag: "ingredient-with-unit";
}

/**
 * an expression that just writes its arguments back
 */
export interface RecipeEchoModel extends RecipeExpressionModel {
  /**
   * the text to write
   */
  text: string;
  /**
   * @inheritdoc
   */
  tag: "expression-echo";
}

/**
 * Base model of recipe expressions
 */
export interface RecipeExpressionModel {
  /** the model tag */
  tag: RecipeExpressionModelTags;
  /** the start index in the string of the expression */
  startIndex: number;
  /**
   * The end index in the string of the expression.
   * This is the index immediately after the last character of the extracted text.
   * For example, with the string "abcd" and extraction "bc", this value is 3.
   * Use with `string.slice()` to extract the text: `text.slice(startIndex, endIndex)`.
   */
  endIndex: number;
}

/**
 * Parses text into a set of recipe expressions
 * @param freeText the free text to lex and parse
 * @returns the set of expression models extracted from the text
 */
export function parseTextForRecipeExpression(
  freeText: string,
): RecipeExpressionModels[] | null {
  const lexerTokens = RecipeExpressionLexer.tokenize(freeText);
  if (lexerTokens.errors.length > 0) {
    return null;
  }
  return parseRecipeExpressionsFromStack(lexerTokens.tokens);
}

/**
 * Scans a token stack looking for recipe expressions. When it finds them, it converts them
 * into models.
 * @param tokenStack the token stack to scan for expressions
 * @returns the set of models found in `tokenStack`
 */
export function parseRecipeExpressionsFromStack(
  tokenStack: IToken[],
): RecipeExpressionModels[] {
  const expressions: RecipeExpressionModels[] = [];

  let startOffset = 0;
  while (true) {
    const position = parseRecipeExpressionFromStack(tokenStack, startOffset);
    if (!position) {
      break;
    }

    if (position[0]) {
      expressions.push(position[0]);
    }

    startOffset = position[1];
  }

  return expressions;
}

/**
 * Extracts the atom token from the token stack
 * @param tokenStack the token stack to extract the atom from
 * @param startOffset the offset from the start of the token stack
 * @returns null if the atom is not found. Otherwise an array where the first index is the
 * atom value. The second index is the token index after the atom in `tokenStack`.
 */
function extractAtomTokenFromStack(
  tokenStack: IToken[],
  startOffset: number,
): [string, number] | null {
  const firstToken = tokenStack[startOffset];
  if (firstToken?.tokenType.name === S_SquaredAtomToken) {
    return [firstToken.image, startOffset + 1];
  }
  if (firstToken?.tokenType.name !== S_SquaredWhitespaceToken) {
    return null;
  }

  const secondToken = tokenStack[startOffset + 1];
  if (secondToken?.tokenType.name === S_SquaredAtomToken) {
    return [secondToken.image, 2 + startOffset];
  }

  return null;
}

/**
 * extracts a set of arguments from the token stack searching for the end token
 * @param tokenStack the token stack
 * @param startOffset the offset to begin searching
 * @returns the set of tokens or null if the stack does not have an end token
 */
function extractExpressionArgumentsFromStack(
  tokenStack: IToken[],
  startOffset: number,
): [IToken[], number] | null {
  if (
    tokenStack.length === startOffset ||
    (tokenStack[startOffset].tokenType.name !== S_SquaredWhitespaceToken &&
      tokenStack[startOffset].tokenType.name !== S_SquaredClosingToken)
  ) {
    // bad spacer token. After atom only closing or whitespace should come.
    return null;
  }

  for (let endIndex = startOffset; endIndex < tokenStack.length; endIndex++) {
    if (tokenStack[endIndex].tokenType.name === S_SquaredClosingToken) {
      return [tokenStack.slice(startOffset, endIndex), endIndex + 1];
    }

    // look for bad tokens
    if (!isTextNumberOrWhitespaceToken(tokenStack[endIndex])) {
      break;
    }
  }

  return null;
}
/**
 * context for a expression extraction
 */
interface IExpressionExtractionContext {
  /**
   * the current atom
   */
  atom: string;
  /**
   * the start index in the raw string
   */
  rawStartIndex: number;
  /**
   * the end index in the raw string
   */
  rawEndIndex: number;
  /**
   * args for the operation if any
   */
  args: IToken[];
  /**
   * the raw token stack
   */
  tokenStack: IToken[];
  /**
   * the offset into the token stack where
   * the opening token is found
   */
  stackOffset: number;
  /**
   * the number of tokens in this expression.
   * `startOffset` + `stackOffset` passed
   * to `tokenStack.slice(startOffset, (startOffset + expressionTokenLength))`
   * will yield all tokens representing the expression.
   */
  expressionTokenLength: number;
}

/**
 * Extracts an echo command expression from the extraction context.
 * An echo command is identified by the atom "echo" followed by text arguments.
 * @param context the extraction context containing the atom and arguments
 * @returns a RecipeEchoModel if the atom is "echo" and arguments are valid text, null otherwise
 */
function echoCommandExtractor({
  atom,
  rawStartIndex,
  rawEndIndex,
  args,
}: IExpressionExtractionContext): RecipeEchoModel | null {
  if (atom !== "echo") {
    return null;
  }
  const text = mergeTextTokens(args);
  if (text === null) {
    return null;
  }
  return {
    startIndex: rawStartIndex,
    endIndex: rawEndIndex,
    text: text,
    tag: "expression-echo",
  };
}

/**
 * Extracts a recipe ingredient expression from the extraction context.
 * Handles both freeform ingredients (no units) and ingredients with units.
 * @param context the extraction context containing the atom and arguments
 * @returns a RecipeIngredientWithUnitModel or RecipeIngredientFreeformModel if valid, null otherwise
 */
function recipeIngredientExtractor({
  args,
  rawStartIndex,
  rawEndIndex,
  atom,
}: IExpressionExtractionContext):
  | RecipeIngredientWithUnitModel
  | RecipeIngredientFreeformModel
  | null {
  if (atom !== "ingredient") {
    return null;
  }

  // looks for pattern: Number WSp Unit Wsp text ...
  if (
    isNumberToken(args[0]) &&
    isWhitespaceToken(args[1]) &&
    isTextToken(args[2]) &&
    isWhitespaceToken(args[3]) &&
    isTextOrNumberToken(args[4])
  ) {
    const amount = args[0].payload.realValue;
    const unit = args[2].payload.text;
    const text = mergeTextTokens(args, 4);
    if (text === null) {
      // happens if the token stack has been values
      return null;
    }

    return {
      tag: "ingredient-with-unit",
      unit: unit,
      amount: amount,
      startIndex: rawStartIndex,
      endIndex: rawEndIndex,
      text: text,
    };
  }

  // default back to unit less
  const text = mergeTextTokens(args);
  if (text === null) {
    return null;
  }

  return {
    tag: "ingredient-freeform",
    text,
    startIndex: rawStartIndex,
    endIndex: rawEndIndex,
  };
}

/**
 * Checks if a token represents a numeric value (integer, decimal, or fraction).
 * @param token the token to check
 * @returns true if the token is a number type, false otherwise
 */
function isNumberToken(
  token: IToken | undefined | null,
): token is ITokenWithPayload<S_SquaredRealValuePayload> {
  switch (token?.tokenType.name) {
    case S_SquaredIntegerToken:
    case S_SquaredDecimalToken:
    case S_SquaredFractionToken:
      return true;
  }

  return false;
}

/**
 * Checks if a token represents a text value (quoted string or text literal).
 * @param token the token to check
 * @returns true if the token is a text type, false otherwise
 */
function isTextToken(
  token: IToken | undefined | null,
): token is ITokenWithPayload<S_SquaredTextValuePayload> {
  switch (token?.tokenType.name) {
    case S_SquaredQuotedStringToken:
    case S_SquaredTextLiteralToken:
      return true;
  }

  return false;
}

/**
 * Checks if a token represents either a text value or a numeric value.
 * @param token the token to check
 * @returns true if the token is text or number, false otherwise
 */
function isTextOrNumberToken(
  token: IToken | undefined | null,
): token is ITokenWithPayload<
  S_SquaredTextValuePayload | S_SquaredRealValuePayload
> {
  if (isTextToken(token)) {
    return true;
  }

  return isNumberToken(token);
}

/**
 * Checks if a token represents either text, number, or whitespace.
 * @param token the token to check
 * @returns true if the token is text, number, or whitespace, false otherwise
 */
function isTextNumberOrWhitespaceToken(
  token: IToken | undefined | null,
): token is
  | ITokenWithPayload<S_SquaredTextValuePayload>
  | ITokenWithPayload<S_SquaredRealValuePayload> {
  if (isTextOrNumberToken(token)) {
    return true;
  }

  return isWhitespaceToken(token);
}

/**
 * Checks if a token represents whitespace.
 * @param token the token to check
 * @returns true if the token is whitespace, false otherwise
 */
function isWhitespaceToken(
  token: IToken | undefined | null,
): token is ITokenWithPayload<S_SquaredTextValuePayload> {
  return token?.tokenType.name === S_SquaredWhitespaceToken;
}

/**
 * Merges multiple text tokens into a single string value.
 * Skips non-text tokens (numbers, whitespace) and concatenates text payloads.
 * @param tokens the array of tokens to merge
 * @param startOffset the optional start index (defaults to 0)
 * @param endOffset the optional end index (defaults to tokens.length)
 * @returns the concatenated text string, or null if any non-text token is encountered
 */
function mergeTextTokens(
  tokens: IToken[],
  startOffset?: number,
  endOffset?: number,
) {
  endOffset ??= tokens.length;
  startOffset ??= 0;
  let value = "";
  for (let i = startOffset; i < endOffset && i < tokens.length; i++) {
    const currentToken = tokens[i];
    if (!isTextNumberOrWhitespaceToken(currentToken)) {
      return null;
    }
    value += currentToken.payload.text;
  }

  return value;
}

/**
 * Set of extractors to run
 */
const modelExtractors = [echoCommandExtractor, recipeIngredientExtractor];

/**
 * Parses one recipe expression from the `tokenStack`
 * @param tokenStack the token stack to scan
 * @param startOffset the index to begin the search inside the token stack
 * @returns null if an expression was not found. An array where the first is the model
 * representing the token data. The second part of the array is
 * the index after the closing token of the expression.
 */
export function parseRecipeExpressionFromStack(
  tokenStack: IToken[],
  startOffset: number,
): [RecipeExpressionModels | null, number] | null {
  // scan forward looking for an opening token
  let foundOpening = false;
  for (; startOffset < tokenStack.length; startOffset++) {
    if (tokenStack[startOffset].tokenType.name === S_SquaredOpeningToken) {
      foundOpening = true;
      break;
    }
  }

  // exit if we hit the end of the stack without finding an opening token
  if (!foundOpening) {
    return null;
  }

  // extract the atom from the opening expression if there is one
  const extractedToken = extractAtomTokenFromStack(tokenStack, startOffset);
  if (!extractedToken) {
    //
    // `extractAtomTokenFromStack` returns null if there is not an
    // atom or it is not in the right spot
    return null;
  }
  const [atomValue, nextIndexAfterAtom] = extractedToken;

  // collect remaining arguments up till the end of the expression
  const extractedExpressionArguments = extractExpressionArgumentsFromStack(
    tokenStack,
    nextIndexAfterAtom,
  );

  if (!extractedExpressionArguments) {
    // `extractExpressionArguments` returns null if a closing token is not found
    return null;
  }
  const [argumentTokens, afterEndOfExpression] = extractedExpressionArguments;

  let totalSize = 0;
  for (let i = startOffset; i < afterEndOfExpression; i++) {
    totalSize += tokenStack[i].image.length;
  }

  const extractionContext: IExpressionExtractionContext = {
    atom: atomValue,
    rawStartIndex: tokenStack[startOffset].startOffset,
    rawEndIndex: tokenStack[startOffset].startOffset + totalSize,
    args: argumentTokens,
    tokenStack: tokenStack,
    stackOffset: startOffset,
    expressionTokenLength: afterEndOfExpression - startOffset,
  };

  for (const extractor of modelExtractors) {
    const result = extractor(extractionContext);
    if (result) {
      return [result, afterEndOfExpression];
    }
  }

  return [null, afterEndOfExpression];
}
