import {
  createToken,
  Lexer,
  tokenMatcher,
  type CustomPatternMatcherReturn,
  type IToken,
  type TokenType,
} from "chevrotain";
import type { INamedTokenType, ITokenWithPayload } from "./type-helpers";

/**
 * Base pattern of a recipe expression token
 * @type TName the value for token name
 * @type TPayload the payload of the token if any
 */
export type IRecipeExpressionToken<TName, TPayload = undefined> = Omit<
  ITokenWithPayload<TPayload>,
  "tokenType"
> & { tokenType: INamedTokenType<TName> };

/**
 * Helper type to grab the name from a `IToken`
 * @type T the type to extract name from
 */
type ExtractTokenName<T extends IToken> = T["tokenType"]["name"];

/**
 * Payload interface for S-squared text value tokens.
 */
export interface TextValuePayload<TText extends string = string> {
  /**
   * extracted text value
   */
  text: TText;
}

/**
 * Payload interface for S-squared real number value tokens.
 */
export interface NumberPayload extends TextValuePayload {
  /**
   * computed real value of the number token
   */
  realValue: number;
}

/**
 * Payload interface for S-squared fraction value tokens.
 */
export interface FractionPayload extends NumberPayload {
  /**
   * numerator of the fraction
   */
  numerator: number;
  /**
   * denominator of the fraction
   */
  denominator: number;
}

/**
 * word outside of the recipe expression
 */
export type RExpOutsideWordToken = IRecipeExpressionToken<"outside-word">;

/**
 * Whitespace between words outside of the recipe expression.
 * Also includes the whitespace before a start recipe expression
 * and the whitespace trailing an end recipe expression.
 * Matched by the regular expression `\s`
 */
export type RExpOutsideWhitespaceToken =
  IRecipeExpressionToken<"outside-whitespace">;

/**
 * Marks the start of a recipe expression: `((`.
 */
export type RExpStartToken = IRecipeExpressionToken<"start-expression">;

/**
 * Marks the end of a recipe expression: `))`.
 */
export type RExpEndToken = IRecipeExpressionToken<"end-expression">;

/**
 * A quoted string: `"values inside of the quotes including ""escaped"" quotes"`
 */
export type RExpQuotedStringToken = IRecipeExpressionToken<
  "quoted-string",
  TextValuePayload
>;

/**
 * An integer value: `1`, `2`, .. `101`, `102`, etc
 * A decimal: `.25`, `0.25`, `3.75`, etc
 */
export type RExpNumberToken = IRecipeExpressionToken<"number", NumberPayload>;

/**
 * A fraction: `1/2`
 */
export type RExpFractionToken = IRecipeExpressionToken<
  "fraction",
  FractionPayload
>;

/**
 * Whitespace between tokens as matched by the regular expression `\s`
 */
export type RExpWhitespaceToken = IRecipeExpressionToken<
  "whitespace",
  TextValuePayload
>;

/**
 * Any text value not in quotes not matched as an integer, fraction, or decimal.
 * Examples include `12ab1`, `word`, `11//22//abcd`, etc
 */
export type RExpTextLiteralToken = IRecipeExpressionToken<
  "text-literal",
  TextValuePayload
>;

/**
 * Matches the start of a sub expression: `(`
 */
export type RExpStartSubExpToken = IRecipeExpressionToken<
  "start-sub-expression",
  TextValuePayload
>;

/**
 * Matches the end of a sub expression: `)`
 */
export type RExpEndSubExpToken = IRecipeExpressionToken<
  "end-sub-expression",
  TextValuePayload
>;

/**
 * A command atom. Only allows ASCII letters [a-zA-Z] as the start character
 * followed by a [a-zA-Z0-9] sequence. Lexer restricts this to
 * only being allowed following a `RExpStart`, `RExpEnd`, `RExpStartSubExp`
 * and `RExpEndSubExp`.
 * @see RExpStartToken
 * @see RExpEndToken
 * @see RExpStartSubExpToken
 * @see RExpEndSubExpToken
 */
export type RExpAtomToken<T extends string = string> = IRecipeExpressionToken<
  "atom",
  TextValuePayload<T>
>;

/**
 * Union of token types from the Reciplex lexer
 */
export type ReciplexExpressionToken =
  | RExpOutsideWordToken
  | RExpOutsideWhitespaceToken
  | RExpStartToken
  | RExpEndToken
  | RExpQuotedStringToken
  | RExpNumberToken
  | RExpFractionToken
  | RExpWhitespaceToken
  | RExpTextLiteralToken
  | RExpAtomToken
  | RExpEndSubExpToken
  | RExpStartSubExpToken;

/**
 * Parsing mode for a nested sub expression
 */
//const SubExpressionMode = "sub-expression-mode";

/**
 * Parsing mode for the top level expression
 */
const ExpressionMode = "expression-mode";

/**
 * Parsing mode outside the recipe expression
 */
const OutsideMode = "outside-mode";

/**
 * The token that identifies the start of a recipe expression
 */
export const RExpStartName: ExtractTokenName<RExpStartToken> =
  "start-expression";

/**
 * The token definition for matching an opening S-squared expression tag.
 * Matches the pattern `((` to enter S-squared expression parsing mode.
 */
const RExpStartTokenDef = createToken({
  name: RExpStartName,
  pattern: /\(\(/,
  push_mode: ExpressionMode,
});

/**
 * A token that ends a recipe expression
 */
export const RExpEndName: ExtractTokenName<RExpEndToken> = "end-expression";

/**
 * The token definition for matching a closing S-squared expression tag.
 * Matches the pattern `))` to exit S-squared expression parsing mode.
 */
const RExpEndTokenDef = createToken({
  name: RExpEndName,
  pattern: /\)\)/,
  pop_mode: true,
});

/**
 * Token that matches a set of non-whitespace characters outside of an expression
 */
export const RExpOutsideWordName: ExtractTokenName<RExpOutsideWordToken> =
  "outside-word";

/**
 * Regex pattern for matching free text literals outside of S-squared expressions.
 */
const RExpOutsideWordRegex = /([^\s(]|(\([^\s(]))+/y;

/**
 * The token definition for matching free text literals outside of S-squared expressions.
 * Matches text that is not whitespace or opening `((` tags.
 */
const RExpOutsideWordTokenDef = createToken({
  name: RExpOutsideWordName,
  pattern: {
    exec: (text, startOffset) => {
      RExpOutsideWordRegex.lastIndex = startOffset;
      const match = RExpOutsideWordRegex.exec(text);
      if (match === null) {
        return null;
      }
      const matchedString = match[0];
      if (matchedString.length < 2) {
        return match;
      }

      const lastTwoChars = matchedString.substring(
        matchedString.length - 2,
        matchedString.length,
      );
      if (lastTwoChars === "((") {
        return [matchedString.substring(0, matchedString.length - 2)];
      }

      return match;
    },
  },
  line_breaks: false,
});

/**
 * Token holding a string value wrapped by quotes
 */
export const RExpQuotedStringName: ExtractTokenName<RExpQuotedStringToken> =
  "quoted-string";

/**
 * Regex pattern for matching quoted string literals within S-squared expressions.
 */
const RExpQuotedStringRegex = /"(?:[^"]|"")*"/y;

/**
 * The token definition for matching quoted string literals within S-squared expressions.
 * Handles escaped quotes by consuming the character after `"`.
 */
const RExpQuotedStringTokenDef = createToken({
  name: RExpQuotedStringName,
  start_chars_hint: ['"'],
  pattern: {
    exec: matchRegexWithStructure(RExpQuotedStringRegex, (text) => {
      const rawValue = text.substring(1, text.length - 1);

      let eatNextChar = false;
      let processedValue = "";
      for (const c of rawValue) {
        if (eatNextChar) {
          eatNextChar = false;
          continue;
        }
        if (c === '"') {
          eatNextChar = true;
        }
        processedValue += c;
      }

      return { text: processedValue } as TextValuePayload;
    }),
  },
  line_breaks: true,
});

/**
 * Token matching an integer value in the text
 */
export const RExpNumberName: ExtractTokenName<RExpNumberToken> = "number";

/**
 * Regex pattern for matching integer and number literals within S-squared expressions.
 */
const RExpNumberRegex = /([0-9]([0-9]*))|(([0-9]*)\.[0-9][0-9]*)/y;

/**
 * The token definition for matching integer and number literals within S-squared expressions.
 * Parses the matched string into an integer value.
 */
const RExpNumberTokenDef = createToken({
  name: RExpNumberName,
  pattern: {
    exec: matchRegexWithStructure(RExpNumberRegex, (s) => {
      return {
        realValue: parseFloat(s),
        text: s,
      } as NumberPayload;
    }),
  },
  start_chars_hint: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "."],
  line_breaks: false,
});

/**
 * Token holding a fraction
 */
export const RExpFractionName: ExtractTokenName<RExpFractionToken> = "fraction";

/**
 * Regex pattern for matching fraction literals within S-squared expressions.
 */
const RExpFractionRegex = /([0-9][0-9]*)\/([0-9][0-9]*)/y;

/**
 * The token definition for matching fraction literals within S-squared expressions.
 * Parses the matched string into numerator, denominator, and real value.
 */
const RExpFractionTokenDef = createToken({
  name: RExpFractionName,
  pattern: {
    exec: matchRegexWithStructure(RExpFractionRegex, (s) => {
      const pieces = s.split("/");
      return {
        numerator: parseInt(pieces[0]),
        denominator: parseInt(pieces[1]),
        realValue: parseInt(pieces[0]) / parseInt(pieces[1]),
        text: s,
      } as FractionPayload;
    }),
  },
  start_chars_hint: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
  line_breaks: false,
});

/**
 * Free text token outside of a recipe expression
 */
export const RExpOutsideWhitespaceName: ExtractTokenName<RExpOutsideWhitespaceToken> =
  "outside-whitespace";

/**
 * The token definition for matching whitespace characters in free text mode.
 * Matches one or more whitespace characters including newlines.
 */
const RExpOutsideWhitespaceTokenDef = createToken({
  name: RExpOutsideWhitespaceName,
  pattern: /\s+/,
  line_breaks: true,
});

/**
 * Regex matching whitespace sequence inside a recipe expression
 */
const RExpWhitespaceRegex = /\s+/y;

/**
 * Token holding a sequence of whitespace characters inside of an expression
 */
export const RExpWhitespaceName: ExtractTokenName<RExpWhitespaceToken> =
  "whitespace";

/**
 * The token definition for matching whitespace characters within S-squared expressions.
 * Matches one or more whitespace characters including newlines.
 */
const RExpWhitespaceTokenDef = createToken({
  name: RExpWhitespaceName,
  pattern: (text, startOffset) => {
    RExpWhitespaceRegex.lastIndex = startOffset;
    const match = RExpWhitespaceRegex.exec(text);
    if (match === null) {
      return null;
    }

    return transformToTextPayload(match);
  },
  line_breaks: true,
});
/**
 * Token holding a sequence of unquoted non-whitespace characters
 */
export const RExpTextLiteralName: ExtractTokenName<RExpTextLiteralToken> =
  "text-literal";

/**
 * Regex pattern for matching text literals within S-squared expressions.
 */
const RExpTextLiteralRegex = /[^\s")()]+/y;

/**
 * The token definition for matching text literals within S-squared expressions.
 * Matches text that is not whitespace, quoted, or closing `))` tags.
 */
const RExpTextLiteralTokenDef = createToken({
  name: RExpTextLiteralName,
  pattern: {
    exec: matchRegexWithStructure(RExpTextLiteralRegex, undefined, (result) => {
      const matchedString = result[0];
      if (matchedString.length < 2) {
        return transformToTextPayload(result);
      }

      const lastTwoChars = matchedString.substring(
        matchedString.length - 2,
        matchedString.length,
      );
      if (lastTwoChars === "))") {
        return transformToTextPayload([
          matchedString.substring(0, matchedString.length - 2),
        ]);
      }

      return transformToTextPayload(result);
    }),
  },
  line_breaks: false,
});

/**
 * Token matching an expression atom. The atom is the expression command. See
 * s-expression syntax for further reading.
 */
export const RExpAtomName: ExtractTokenName<RExpAtomToken> = "atom";

/**
 * Regex pattern for matching atom (identifier) literals within S-squared expressions.
 */
const RExpAtomRegex = /[a-zA-Z][a-zA-Z0-9]*/y;

function isOneOfTokenSet(tokenToTest: IToken, ...possibleTokens: TokenType[]) {
  return possibleTokens.some((t) => tokenMatcher(tokenToTest, t));
}

/**
 * The token definition for matching atom identifiers within S-squared expressions.
 * Matches identifiers that start with a letter and contain only letters or digits.
 * Validates that the token is followed by a terminal separator (whitespace or `))`).
 */
const RExpAtomTokenDef = createToken({
  name: RExpAtomName,
  pattern: {
    exec: (text, startOffset, matchedTokens) => {
      const isSExp = (offset: number) => {
        return isOneOfTokenSet(
          matchedTokens[matchedTokens.length - offset],
          RExpStartTokenDef,
          // RExpStartSubExpTokenDef,
        );
      };
      if (matchedTokens.length < 1) {
        return null;
      } else if (matchedTokens.length == 1 && !isSExp(1)) {
        return null;
      } else if (matchedTokens.length >= 2) {
        if (
          isSExp(2) &&
          isOneOfTokenSet(
            matchedTokens[matchedTokens.length - 1],
            RExpWhitespaceTokenDef,
          )
        ) {
          /* empty */
        } else if (isSExp(1)) {
          /* empty */
        } else {
          return null;
        }
      }
      RExpAtomRegex.lastIndex = startOffset;
      const match = RExpAtomRegex.exec(text);
      if (match === null) {
        return null;
      }

      if (
        !isTerminalSeparator(text, startOffset + match[0].length, matchedTokens)
      ) {
        return null;
      }
      return transformToTextPayload(match);
    },
  },
  line_breaks: false,
});

//const S_ExpressionOpeningTokenRegex = /\(/y;
// const RExpStartSubExpName: ExtractTokenName<RExpStartSubExpToken> =
//   "start-sub-expression";
// const RExpStartSubExpTokenDef = createToken({
//   name: RExpStartSubExpName,
//   pattern: /\(/,
//   push_mode: SubExpressionMode,

//   // pattern: {
//   //   exec: matchRegexWithStructure(S_ExpressionOpeningTokenRegex),
//   // },
//   // line_breaks: false,
//   // start_chars_hint: ["("],
// });

//const S_SquaredNestedExpClosingTokenRegex = /\)/y;
// const RExpEndSubExpName: ExtractTokenName<RExpEndSubExpToken> =
//   "end-sub-expression";
// const RExpEndSubExpTokenDef = createToken({
//   name: RExpEndSubExpName,
//   pattern: /\)/,
//   pop_mode: true,
//   // pattern: {
//   //   exec: matchRegexWithStructure(S_ExpressionClosingTokenRegex),
//   // },
//   // line_breaks: false,
//   // start_chars_hint: [")"],
// });

/**
 * Checks if the last matched token was a separator (whitespace or opening tag).
 * @param matchedTokens The array of tokens matched so far.
 * @returns `true` if the last token was a separator, `false` otherwise.
 */
function wasLastTokenSeparator(matchedTokens: IToken[]) {
  if (matchedTokens.length <= 0) {
    return false;
  }

  const lastToken = matchedTokens[matchedTokens.length - 1];

  return isOneOfTokenSet(
    lastToken,
    RExpWhitespaceTokenDef,
    RExpStartTokenDef,
    //  RExpStartSubExpTokenDef,
    //  RExpEndSubExpTokenDef,
  );
}

/**
 * Simple regex for checking if a character is whitespace
 */
const isCharacterWhitespaceRegex = /\s/;

/**
 * Checks if the character at the given offset is a terminal separator.
 * A terminal separator is either end of string, whitespace, or closing `))`.
 * @param text The text being parsed.
 * @param startOffset The offset to check.
 * @returns `true` if the character at the offset is a terminal separator.
 */
function isTerminalSeparator(
  text: string,
  startOffset: number,
  _matchedTokens: IToken[],
) {
  if (startOffset === text.length) {
    return true;
  }

  const currentCharacter = text[startOffset];
  if (isCharacterWhitespaceRegex.test(currentCharacter)) {
    return true;
  }

  // const [open, close] = matchedTokens.reduce(
  //   ([open, close], t) => {
  //     if (tokenMatcher(t, RExpStartSubExpTokenDef)) {
  //       return [open + 1, close];
  //     }

  //     if (tokenMatcher(t, RExpEndSubExpTokenDef)) {
  //       return [open, close + 1];
  //     }

  //     return [open, close];
  //   },
  //   [0, 0],
  // );

  // if (open !== close) {
  //   if (text.substring(startOffset, startOffset + 1) === ")") {
  //     return true;
  //   }
  // } else {
  if (text.substring(startOffset, startOffset + 2) === "))") {
    return true;
  }
  //}

  return false;
}

/**
 * Matches a regex pattern and optionally transforms the result into a payload.
 * Validates that the token is preceded by a separator and followed by a terminal separator.
 * @param regEx The regex pattern to match.
 * @param payloadCreator Optional function to transform the matched text into a payload.
 * @param postResultHook Optional function to further process the result.
 * @returns A CustomPatternMatcherReturn with the matched text and optional payload.
 */
function matchRegexWithStructure(
  regEx: RegExp,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  payloadCreator?: (text: string) => any,
  postResultHook?: (
    result: CustomPatternMatcherReturn,
  ) => CustomPatternMatcherReturn | null,
) {
  return (
    text: string,
    startOffset: number,
    matchedTokens: IToken[],
  ): CustomPatternMatcherReturn | null => {
    if (!wasLastTokenSeparator(matchedTokens)) {
      return null;
    }

    regEx.lastIndex = startOffset;
    const match = regEx.exec(text);
    if (match === null) {
      return null;
    }

    if (
      !isTerminalSeparator(text, startOffset + match[0].length, matchedTokens)
    ) {
      return null;
    }

    const result: CustomPatternMatcherReturn = [match[0]];
    result.payload = payloadCreator?.(match[0]);

    if (postResultHook) {
      return postResultHook(result);
    }
    return result;
  };
}

/**
 * Transforms matched data into a text payload with optional custom value.
 * @param dataToDecorate The matched data array.
 * @param value Optional custom value to use for the payload text.
 * @returns A CustomPatternMatcherReturn decorated with a text payload.
 */
function transformToTextPayload(
  dataToDecorate: [string] | RegExpMatchArray,
  value?: string,
): CustomPatternMatcherReturn {
  const result: CustomPatternMatcherReturn = [dataToDecorate[0]];
  result.payload = {
    text: value ?? dataToDecorate[0],
  } as TextValuePayload;
  return result;
}

export function isQuotedStringToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpQuotedStringToken {
  if (token?.tokenType.name !== "quoted-string") {
    return false;
  }

  return true;
}

export function isTextLiteralToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpTextLiteralToken {
  if (token?.tokenType.name !== "text-literal") {
    return false;
  }

  return true;
}

export function isFractionToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpFractionToken {
  if (token?.tokenType.name !== "fraction") {
    return false;
  }

  return true;
}

export function isNumberToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpNumberToken {
  if (token?.tokenType.name !== "number") {
    return false;
  }

  return true;
}

export function isNumberLikeToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpNumberToken | RExpFractionToken {
  if (!isNumberToken(token) && !isFractionToken(token)) {
    return false;
  }

  return true;
}

export function isTextLikeToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpTextLiteralToken | RExpQuotedStringToken {
  if (!isTextLiteralToken(token) && !isQuotedStringToken(token)) {
    return false;
  }

  return true;
}

export function isOutsideLikeToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpOutsideWhitespaceToken | RExpOutsideWordToken {
  if (
    token?.tokenType.name !== "outside-whitespace" &&
    token?.tokenType.name !== "outside-word"
  ) {
    return false;
  }

  return true;
}

export function isAtomToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpAtomToken {
  if (token?.tokenType.name !== "atom") {
    return false;
  }

  return true;
}

/**
 * Checks if a token represents a text value (quoted string or text literal).
 * @param token the token to check
 * @returns true if the token is a text type, false otherwise
 */
// export function isTextToken(
//   token: IToken | undefined | null,
// ): token is ITokenWithPayload<TextValuePayload> {
//   switch (token?.tokenType.name) {
//     case RExpQuotedStringName:
//     case RExpTextLiteralName:
//       return true;
//   }

//   return false;
// }

/**
 * Checks if a token represents either a text value or a numeric value.
 * @param token the token to check
 * @returns true if the token is text or number, false otherwise
 */
// export function isTextOrNumberToken(
//   token: IToken | undefined | null,
// ): token is ITokenWithPayload<TextValuePayload | NumberPayload> {
//   if (isTextToken(token)) {
//     return true;
//   }

//   return isNumberToken(token);
// }

/**
 * Checks if a token represents either text, number, or whitespace.
 * @param token the token to check
 * @returns true if the token is text, number, or whitespace, false otherwise
 */
// export function isTextNumberOrWhitespaceToken(
//   token: IToken | undefined | null,
// ): token is
//   | ITokenWithPayload<TextValuePayload>
//   | ITokenWithPayload<NumberPayload> {
//   if (isTextOrNumberToken(token)) {
//     return true;
//   }

//   return isWhitespaceToken(token);
// }

/**
 * Checks if a token represents whitespace.
 * @param token the token to check
 * @returns true if the token is whitespace, false otherwise
 */
// export function isWhitespaceToken(
//   token: IToken | undefined | null,
// ): token is ITokenWithPayload<TextValuePayload> {
//   return token?.tokenType.name === RExpWhitespaceName;
// }

/**
 * Checks if a token represents a numeric value (integer, decimal, or fraction).
 * @param token the token to check
 * @returns true if the token is a number type, false otherwise
 */
// export function isNumberToken(
//   token: IToken | undefined | null,
// ): token is ITokenWithPayload<NumberPayload> {
//   switch (token?.tokenType.name) {
//     case RExpNumberName:
//     case RExpFractionName:
//       return true;
//   }

//   return false;
// }

/**
 * Merges multiple text tokens into a single string value.
 * Skips non-text tokens (numbers, whitespace) and concatenates text payloads.
 * @param tokens the array of tokens to merge
 * @param startOffset the optional start index (defaults to 0)
 * @param endOffset the optional end index (defaults to tokens.length)
 * @returns the concatenated text string, or null if any non-text token is encountered
 */
// export function mergeTextTokens(
//   tokens: IToken[],
//   startOffset?: number,
//   endOffset?: number,
// ) {
//   endOffset ??= tokens.length;
//   startOffset ??= 0;
//   let value = "";
//   for (let i = startOffset; i < endOffset && i < tokens.length; i++) {
//     const currentToken = tokens[i];
//     if (!isTextNumberOrWhitespaceToken(currentToken)) {
//       return null;
//     }
//     value += currentToken.payload.text;
//   }

//   return value;
// }

/**
 * The lexer for parsing recipe expressions with S-squared syntax.
 * Supports parsing free text and S-squared expressions delimited by `((` and `))`.
 * Handles atoms, numbers (integers, decimals, fractions), quoted strings, and text literals.
 */
export const RecipeExpressionLexer = new Lexer({
  modes: {
    [OutsideMode]: [
      RExpOutsideWhitespaceTokenDef,
      RExpStartTokenDef,
      RExpOutsideWordTokenDef,
    ],
    [ExpressionMode]: [
      RExpQuotedStringTokenDef,
      RExpNumberTokenDef,
      RExpFractionTokenDef,
      // RExpDecimalTokenDef,
      RExpAtomTokenDef,
      RExpWhitespaceTokenDef,
      RExpEndTokenDef,
      // RExpStartSubExpTokenDef,
      RExpTextLiteralTokenDef,
    ],
    // [SubExpressionMode]: [
    //   RExpQuotedStringTokenDef,
    //   RExpIntegerTokenDef,
    //   RExpFractionTokenDef,
    //   RExpDecimalTokenDef,
    //   RExpAtomTokenDef,
    //   RExpWhitespaceTokenDef,
    //   RExpEndSubExpTokenDef,
    //   RExpStartSubExpTokenDef,
    //   RExpTextLiteralTokenDef,
    // ],
  },
  defaultMode: OutsideMode,
});
