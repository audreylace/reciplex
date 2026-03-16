import {
  createToken,
  Lexer,
  tokenMatcher,
  type CustomPatternMatcherReturn,
  type IToken,
  type TokenType,
} from "chevrotain";

/**
 * Helper for changing `IToken.payload` from `any` to `T`.
 */
type TokenWithPayload<T> = Omit<IToken, "payload"> & { payload: T };

/**
 * Helper for stronger typing the name property on `TokenType`
 * @typeParam TName - the new type of for `name`. Must derive from `string`.
 */
type NamedTokenType<TName> = Omit<TokenType, "name"> & { name: TName };

/**
 * Base pattern of a recipe expression token
 * @typeParam TName - the type for token name
 * @typeParam TPayload - the payload of the token. `undefined` by default.
 */
export type RecipeExpressionToken<TName, TPayload = ITextValuePayload> = Omit<
  TokenWithPayload<TPayload>,
  "tokenType"
> & { tokenType: NamedTokenType<TName> };

/**
 * Helper type to grab the name from a `IToken`
 * @typeParam TExtendedTokenType - the type to extract name from
 */
type ExtractTokenName<TExtendedTokenType extends IToken> =
  TExtendedTokenType["tokenType"]["name"];

/**
 * Payload interface for S-squared text value tokens.
 * @typeParam TText - the type of `text`. Must extend from `string`.
 */
export interface ITextValuePayload<TText extends string = string> {
  /**
   * extracted text value
   */
  text: TText;
}

/**
 * Payload used by tokens that produce number values
 * @see IFractionTokenPayload
 */
export interface INumberTokenPayload extends ITextValuePayload {
  /**
   * computed real value of the number token
   */
  realValue: number;
}

/**
 * Payload from the fraction token.
 * @see INumberTokenPayload
 */
export interface IFractionTokenPayload extends INumberTokenPayload {
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
 * Word outside of the recipe expression
 */
export type RExpOutsideWordToken = RecipeExpressionToken<"outside-word">;

/**
 * Whitespace between words outside of the recipe expression.
 * Also includes the whitespace before a start recipe expression
 * and the whitespace trailing an end recipe expression.
 * Matched by the regular expression `\s`
 */
export type RExpOutsideWhitespaceToken =
  RecipeExpressionToken<"outside-whitespace">;

/**
 * Marks the start of a recipe expression: `((`.
 */
export type RExpStartToken = RecipeExpressionToken<"start-expression">;

/**
 * Marks the end of a recipe expression: `))`.
 */
export type RExpEndToken = RecipeExpressionToken<"end-expression">;

/**
 * A quoted string: `"values inside of the quotes including ""escaped"" quotes"`
 */
export type RExpQuotedStringToken = RecipeExpressionToken<
  "quoted-string",
  ITextValuePayload
>;

/**
 * An integer value: `1`, `2`, .. `101`, `102`, etc
 * A decimal: `.25`, `0.25`, `3.75`, etc
 */
export type RExpNumberToken = RecipeExpressionToken<
  "number",
  INumberTokenPayload
>;

/**
 * A fraction: `1/2`
 */
export type RExpFractionToken = RecipeExpressionToken<
  "fraction",
  IFractionTokenPayload
>;

/**
 * Whitespace between tokens as matched by the regular expression `\s`
 */
export type RExpWhitespaceToken = RecipeExpressionToken<
  "whitespace",
  ITextValuePayload
>;

/**
 * Any text value not in quotes not matched as an integer, fraction, or decimal.
 * Examples include `12ab1`, `word`, `11//22//abcd`, etc
 */
export type RExpTextLiteralToken = RecipeExpressionToken<
  "text-literal",
  ITextValuePayload
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
export type RExpAtomToken<T extends string = string> = RecipeExpressionToken<
  "atom",
  ITextValuePayload<T>
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
  | RExpAtomToken;

/**
 * Parsing mode for the top level expression
 */
const expressionLexerMode = "expression-mode";

/**
 * Parsing mode outside the recipe expression
 */
const outsideLexerMode = "outside-mode";

/**
 * The token that identifies the start of a recipe expression
 */
export const rExpStartName: ExtractTokenName<RExpStartToken> =
  "start-expression";

/**
 * The token definition for matching an opening S-squared expression tag.
 * Matches the pattern `((` to enter S-squared expression parsing mode.
 */
const rExpStartTokenDef = createToken({
  name: rExpStartName,
  pattern: /\(\(/,
  push_mode: expressionLexerMode,
});

/**
 * A token that ends a recipe expression
 */
export const rExpEndName: ExtractTokenName<RExpEndToken> = "end-expression";

/**
 * The token definition for matching a closing S-squared expression tag.
 * Matches the pattern `))` to exit S-squared expression parsing mode.
 */
const rExpEndTokenDef = createToken({
  name: rExpEndName,
  pattern: /\)\)/,
  pop_mode: true,
});

/**
 * Token that matches a set of non-whitespace characters outside of an expression
 */
export const rExpOutsideWordName: ExtractTokenName<RExpOutsideWordToken> =
  "outside-word";

/**
 * Regex pattern for matching free text literals outside of S-squared expressions.
 */
const rExpOutsideWordRegex = /[^\s]+/y;

/**
 * The token definition for matching free text literals outside of S-squared expressions.
 * Matches text that is not whitespace or opening `((` tags.
 */
const rExpOutsideWordTokenDef = createToken({
  name: rExpOutsideWordName,
  pattern: {
    exec: (text, startOffset) => {
      rExpOutsideWordRegex.lastIndex = startOffset;
      const match = rExpOutsideWordRegex.exec(text);
      if (match === null) {
        return null;
      }
      const matchedString = match[0];
      if (matchedString.length < 2) {
        return transformToTextPayload(match);
      }

      const index = matchedString.indexOf("((");
      if (index !== -1) {
        if (index === 0) {
          return null;
        }
        return transformToTextPayload([matchedString.substring(0, index)]);
      }

      return transformToTextPayload(match);
    },
  },
  line_breaks: false,
});

/**
 * Token holding a string value wrapped by quotes
 */
export const rExpQuotedStringName: ExtractTokenName<RExpQuotedStringToken> =
  "quoted-string";

/**
 * Regex pattern for matching quoted string literals within S-squared expressions.
 */
const rExpQuotedStringRegex = /"(?:[^"]|"")*"/y;

/**
 * The token definition for matching quoted string literals within S-squared expressions.
 * Handles escaped quotes by consuming the character after `"`.
 */
const rExpQuotedStringTokenDef = createToken({
  name: rExpQuotedStringName,
  start_chars_hint: ['"'],
  pattern: {
    exec: matchRegexWithStructure(rExpQuotedStringRegex, (text) => {
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

      return { text: processedValue } as ITextValuePayload;
    }),
  },
  line_breaks: true,
});

/**
 * Token matching an integer value in the text
 */
export const rExpNumberName: ExtractTokenName<RExpNumberToken> = "number";

/**
 * Regex pattern for matching integer and number literals within S-squared expressions.
 */
const rExpNumberRegex = /([0-9]([0-9]*))|(([0-9]*)\.[0-9][0-9]*)/y;

/**
 * The token definition for matching integer and number literals within S-squared expressions.
 * Parses the matched string into an integer value.
 */
const rExpNumberTokenDef = createToken({
  name: rExpNumberName,
  pattern: {
    exec: matchRegexWithStructure(rExpNumberRegex, (s) => {
      return {
        realValue: parseFloat(s),
        text: s,
      } as INumberTokenPayload;
    }),
  },
  start_chars_hint: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "."],
  line_breaks: false,
});

/**
 * Token holding a fraction
 */
export const rExpFractionName: ExtractTokenName<RExpFractionToken> = "fraction";

/**
 * Regex pattern for matching fraction literals within S-squared expressions.
 */
const rExpFractionRegex = /([0-9][0-9]*)\/([0-9][0-9]*)/y;

/**
 * The token definition for matching fraction literals within S-squared expressions.
 * Parses the matched string into numerator, denominator, and real value.
 */
const rExpFractionTokenDef = createToken({
  name: rExpFractionName,
  pattern: {
    exec: matchRegexWithStructure(rExpFractionRegex, (s) => {
      const pieces = s.split("/");
      return {
        numerator: parseInt(pieces[0]),
        denominator: parseInt(pieces[1]),
        realValue: parseInt(pieces[0]) / parseInt(pieces[1]),
        text: s,
      } as IFractionTokenPayload;
    }),
  },
  start_chars_hint: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
  line_breaks: false,
});

/**
 * Free text token outside of a recipe expression
 */
export const rExpOutsideWhitespaceName: ExtractTokenName<RExpOutsideWhitespaceToken> =
  "outside-whitespace";

/**
 * The token definition for matching whitespace characters in free text mode.
 * Matches one or more whitespace characters including newlines.
 */
const rExpOutsideWhitespaceTokenDef = createToken({
  name: rExpOutsideWhitespaceName,
  pattern: /\s+/,
  line_breaks: true,
});

/**
 * Regex matching whitespace sequence inside a recipe expression
 */
const rExpWhitespaceRegex = /\s+/y;

/**
 * Token holding a sequence of whitespace characters inside of an expression
 */
export const rExpWhitespaceName: ExtractTokenName<RExpWhitespaceToken> =
  "whitespace";

/**
 * The token definition for matching whitespace characters within S-squared expressions.
 * Matches one or more whitespace characters including newlines.
 */
const rExpWhitespaceTokenDef = createToken({
  name: rExpWhitespaceName,
  pattern: (text, startOffset) => {
    rExpWhitespaceRegex.lastIndex = startOffset;
    const match = rExpWhitespaceRegex.exec(text);
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
export const rExpTextLiteralName: ExtractTokenName<RExpTextLiteralToken> =
  "text-literal";

/**
 * Regex pattern for matching text literals within S-squared expressions.
 */
const rExpTextLiteralRegex = /[^\s")(]+/y;

/**
 * The token definition for matching text literals within S-squared expressions.
 * Matches text that is not whitespace, quoted, or closing `))` tags.
 */
const rExpTextLiteralTokenDef = createToken({
  name: rExpTextLiteralName,
  pattern: {
    exec: matchRegexWithStructure(
      rExpTextLiteralRegex,
      undefined,
      transformToTextPayload,
    ),
  },
  line_breaks: false,
});

/**
 * Token matching an expression atom. The atom is the expression command. See
 * s-expression syntax for further reading.
 */
export const rExpAtomName: ExtractTokenName<RExpAtomToken> = "atom";

/**
 * Regex pattern for matching atom (identifier) literals within S-squared expressions.
 */
const rExpAtomRegex = /[a-zA-Z][a-zA-Z0-9]*/y;

/**
 * The token definition for matching atom identifiers within S-squared expressions.
 * Matches identifiers that start with a letter and contain only letters or digits.
 * Validates that the token is followed by a terminal separator (whitespace or `))`).
 */
const rExpAtomTokenDef = createToken({
  name: rExpAtomName,
  pattern: {
    exec: (text, startOffset, matchedTokens) => {
      const isSExp = (offset: number) => {
        const token = matchedTokens[matchedTokens.length + offset];
        if (!token) {
          return false;
        }

        return tokenMatcher(token, rExpStartTokenDef);
      };

      if (
        !isSExp(-1) &&
        !(
          isSExp(-2) &&
          isOneOfTokenSet(
            matchedTokens[matchedTokens.length - 1],
            rExpWhitespaceTokenDef,
          )
        )
      ) {
        return null;
      }

      rExpAtomRegex.lastIndex = startOffset;
      const match = rExpAtomRegex.exec(text);
      if (match === null) {
        return null;
      }

      if (!isTerminalSeparator(text, startOffset + match[0].length)) {
        return null;
      }
      return transformToTextPayload(match);
    },
  },
  line_breaks: false,
});

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

  return isOneOfTokenSet(lastToken, rExpWhitespaceTokenDef, rExpStartTokenDef);
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
function isTerminalSeparator(text: string, startOffset: number) {
  if (startOffset === text.length) {
    return true;
  }

  const currentCharacter = text[startOffset];
  if (isCharacterWhitespaceRegex.test(currentCharacter)) {
    return true;
  }

  if (text.substring(startOffset, startOffset + 2) === "))") {
    return true;
  }

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
function matchRegexWithStructure<T>(
  regEx: RegExp,
  payloadCreator?: (text: string) => T,
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

    if (!isTerminalSeparator(text, startOffset + match[0].length)) {
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
  } as ITextValuePayload;
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

export function isOpeningToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpStartToken {
  if (token?.tokenType.name !== rExpStartName) {
    return false;
  }

  return true;
}

export function isClosingToken(
  token: ReciplexExpressionToken | undefined | null,
): token is RExpEndToken {
  if (token?.tokenType.name !== rExpEndName) {
    return false;
  }

  return true;
}

function isOneOfTokenSet(
  tokenToTest: IToken | undefined | null,
  ...possibleTokens: TokenType[]
) {
  if (!tokenToTest) {
    return false;
  }
  return possibleTokens.some((t) => tokenMatcher(tokenToTest, t));
}

/**
 * The lexer for parsing recipe expressions with S-squared syntax.
 * Supports parsing free text and S-squared expressions delimited by `((` and `))`.
 * Handles atoms, numbers (integers, decimals, fractions), quoted strings, and text literals.
 */
export const recipeExpressionLexer = new Lexer({
  modes: {
    [outsideLexerMode]: [
      rExpOutsideWhitespaceTokenDef,
      rExpStartTokenDef,
      rExpOutsideWordTokenDef,
    ],
    [expressionLexerMode]: [
      rExpQuotedStringTokenDef,
      rExpNumberTokenDef,
      rExpFractionTokenDef,
      rExpAtomTokenDef,
      rExpWhitespaceTokenDef,
      rExpEndTokenDef,
      rExpTextLiteralTokenDef,
    ],
  },
  defaultMode: outsideLexerMode,
});
