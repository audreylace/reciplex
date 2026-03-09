import {
  createToken,
  Lexer,
  tokenMatcher,
  type CustomPatternMatcherReturn,
  type IToken,
} from "chevrotain";

/**
 * The name of the mode used when parsing S-squared expressions.
 */
const S_SquaredExpressionMode = "s-squared-expression-mode";

/**
 * The name of the default mode used when parsing free text outside of S-squared expressions.
 */
const FreeTextMode = "free-text-mode";

/**
 * The token that identifies the start of a recipe expression
 */
export const S_SquaredOpeningToken = "s-squared-opening-tag";

/**
 * The token definition for matching an opening S-squared expression tag.
 * Matches the pattern `((` to enter S-squared expression parsing mode.
 */
const S_SquaredOpeningTokenDef = createToken({
  name: S_SquaredOpeningToken,
  pattern: /\(\(/,
  push_mode: S_SquaredExpressionMode,
});

/**
 * A token that ends a recipe expression
 */
export const S_SquaredClosingToken = "s-squared-closing-tag";

/**
 * The token definition for matching a closing S-squared expression tag.
 * Matches the pattern `))` to exit S-squared expression parsing mode.
 */
const S_SquaredClosingTokenDef = createToken({
  name: S_SquaredClosingToken,
  pattern: /\)\)/,
  pop_mode: true,
});

/**
 * Token that matches a set of non-whitespace characters outside of an expression
 */
export const FreeTextLiteralToken = "free-text-literal";

/**
 * Regex pattern for matching free text literals outside of S-squared expressions.
 */
const FreeTextLiteralRegex = /([^\s(]|(\([^\s(]))+/y;

/**
 * The token definition for matching free text literals outside of S-squared expressions.
 * Matches text that is not whitespace or opening `((` tags.
 */
const FreeTextLiteralTokenDef = createToken({
  name: FreeTextLiteralToken,
  pattern: {
    exec: (text, startOffset) => {
      FreeTextLiteralRegex.lastIndex = startOffset;
      const match = FreeTextLiteralRegex.exec(text);
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
 * Payload interface for S-squared quoted string tokens.
 */
export interface S_SquaredQuotedStringPayload {
  /**
   * the text value of the token
   */
  textValue: string;
}

/**
 * Token holding a string value wrapped by quotes
 */
export const S_SquaredQuotedStringToken = "s-squared-quoted-string";

/**
 * Regex pattern for matching quoted string literals within S-squared expressions.
 */
const S_SquaredQuotedStringRegex = /"(?:[^"]|"")*"/y;

/**
 * The token definition for matching quoted string literals within S-squared expressions.
 * Handles escaped quotes by consuming the character after `"`.
 */
const S_SquaredQuotedStringTokenDef = createToken({
  name: S_SquaredQuotedStringToken,
  start_chars_hint: ['"'],
  pattern: {
    exec: matchRegexWithStructure(S_SquaredQuotedStringRegex, (text) => {
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

      return { text: processedValue } as S_SquaredTextValuePayload;
    }),
  },
  line_breaks: true,
});

/**
 * Payload interface for S-squared text value tokens.
 */
export interface S_SquaredTextValuePayload {
  /**
   * extracted text value
   */
  text: string;
}

/**
 * Payload interface for S-squared real number value tokens.
 */
export interface S_SquaredRealValuePayload extends S_SquaredTextValuePayload {
  /**
   * computed real value of the number token
   */
  realValue: number;
}

/**
 * Payload interface for S-squared integer value tokens.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface S_SquaredIntegerPayload extends S_SquaredRealValuePayload {}

/**
 * Token matching an integer value in the text
 */
export const S_SquaredIntegerToken = "s-squared-integer";

/**
 * Regex pattern for matching integer literals within S-squared expressions.
 */
const S_SquaredIntegerRegex = /[0-9]([0-9]*)/y;

/**
 * The token definition for matching integer literals within S-squared expressions.
 * Parses the matched string into an integer value.
 */
const S_SquaredIntegerTokenDef = createToken({
  name: S_SquaredIntegerToken,
  pattern: {
    exec: matchRegexWithStructure(S_SquaredIntegerRegex, (s) => {
      return {
        realValue: parseInt(s),
        text: s,
      } as S_SquaredIntegerPayload;
    }),
  },
  start_chars_hint: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
  line_breaks: false,
});

/**
 * Payload interface for S-squared fraction value tokens.
 */
export interface S_SquaredFractionPayload extends S_SquaredRealValuePayload {
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
 * Token holding a fraction
 */
export const S_SquaredFractionToken = "s-squared-fraction";

/**
 * Regex pattern for matching fraction literals within S-squared expressions.
 */
const S_SquaredFractionRegex = /([0-9][0-9]*)\/([0-9][0-9]*)/y;

/**
 * The token definition for matching fraction literals within S-squared expressions.
 * Parses the matched string into numerator, denominator, and real value.
 */
const S_SquaredFractionDef = createToken({
  name: S_SquaredFractionToken,
  pattern: {
    exec: matchRegexWithStructure(S_SquaredFractionRegex, (s) => {
      const pieces = s.split("/");
      return {
        numerator: parseInt(pieces[0]),
        denominator: parseInt(pieces[1]),
        realValue: parseInt(pieces[0]) / parseInt(pieces[1]),
        text: s,
      } as S_SquaredFractionPayload;
    }),
  },
  start_chars_hint: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
  line_breaks: false,
});

/**
 * Payload interface for S-squared decimal value tokens.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface S_SquaredDecimalTokenPayload extends S_SquaredRealValuePayload {}

/**
 * Token holding a decimal
 */
export const S_SquaredDecimalToken = "s-squared-decimal";

/**
 * Regex pattern for matching decimal number literals within S-squared expressions.
 */
const S_SquaredDecimalRegex = /([0-9]*)\.[0-9][0-9]*/y;

/**
 * The token definition for matching decimal number literals within S-squared expressions.
 * Parses the matched string into a floating-point value.
 */
const S_SquaredDecimalTokenDef = createToken({
  name: S_SquaredDecimalToken,
  pattern: {
    exec: matchRegexWithStructure(S_SquaredDecimalRegex, (s) => {
      return {
        realValue: parseFloat(s),
        text: s,
      } as S_SquaredDecimalTokenPayload;
    }),
  },
  start_chars_hint: [".", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
  line_breaks: false,
});

/**
 * Free text token outside of a recipe expression
 */
export const FreeTextWhitespaceToken = "free-text-whitespace";

/**
 * The token definition for matching whitespace characters in free text mode.
 * Matches one or more whitespace characters including newlines.
 */
const FreeTextWhitespaceTokenDef = createToken({
  name: FreeTextWhitespaceToken,
  pattern: /\s+/,
  line_breaks: true,
});

/**
 * Regex matching whitespace sequence inside a recipe expression
 */
const S_SquaredWhitespaceRegex = /\s+/y;

/**
 * Token holding a sequence of whitespace characters inside of an expression
 */
export const S_SquaredWhitespaceToken = "s-squared-whitespace";

/**
 * The token definition for matching whitespace characters within S-squared expressions.
 * Matches one or more whitespace characters including newlines.
 */
const S_SquaredWhitespaceTokenDef = createToken({
  name: S_SquaredWhitespaceToken,
  pattern: (text, startOffset) => {
    S_SquaredWhitespaceRegex.lastIndex = startOffset;
    const match = S_SquaredWhitespaceRegex.exec(text);
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
export const S_SquaredTextLiteralToken = "s-squared-text-literal";

/**
 * Regex pattern for matching text literals within S-squared expressions.
 */
const S_SquaredTextLiteralRegex = /([^\s"]|(\)[^\s)]))+/y;

/**
 * The token definition for matching text literals within S-squared expressions.
 * Matches text that is not whitespace, quotes, or closing `))` tags.
 */
const S_SquaredTextLiteralTokenDef = createToken({
  name: S_SquaredTextLiteralToken,
  pattern: {
    exec: matchRegexWithStructure(
      S_SquaredTextLiteralRegex,
      undefined,
      (result) => {
        const matchedString = result[0];
        if (matchedString.length < 2) {
          return result;
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
      },
    ),
  },
  line_breaks: false,
});

/**
 * Token matching an expression atom. The atom is the expression command. See
 * s-expression syntax for further reading.
 */
export const S_SquaredAtomToken = "s-squared-atom";

/**
 * Regex pattern for matching atom (identifier) literals within S-squared expressions.
 */
const S_SquaredAtomRegex = /[a-zA-Z][a-zA-Z0-9]*/y;

/**
 * The token definition for matching atom identifiers within S-squared expressions.
 * Matches identifiers that start with a letter and contain only letters or digits.
 * Validates that the token is followed by a terminal separator (whitespace or `))`).
 */
const S_SquaredAtomTokenDef = createToken({
  name: S_SquaredAtomToken,
  pattern: {
    exec: (text, startOffset, matchedTokens) => {
      const isSExp = (offset: number) => {
        return tokenMatcher(
          matchedTokens[matchedTokens.length - offset],
          S_SquaredOpeningTokenDef,
        );
      };
      if (matchedTokens.length < 1) {
        return null;
      } else if (matchedTokens.length == 1 && !isSExp(1)) {
        return null;
      } else if (matchedTokens.length >= 2) {
        if (
          isSExp(2) &&
          tokenMatcher(
            matchedTokens[matchedTokens.length - 1],
            S_SquaredWhitespaceTokenDef,
          )
        ) {
          /* empty */
        } else if (isSExp(1)) {
          /* empty */
        } else {
          return null;
        }
      }
      S_SquaredAtomRegex.lastIndex = startOffset;
      const match = S_SquaredAtomRegex.exec(text);
      if (match === null) {
        return null;
      }

      if (!isTerminalSeparator(text, startOffset + match[0].length)) {
        return null;
      }
      return match;
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

  if (tokenMatcher(lastToken, S_SquaredWhitespaceTokenDef)) {
    return true;
  }

  if (tokenMatcher(lastToken, S_SquaredOpeningTokenDef)) {
    return true;
  }

  return false;
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
  } as S_SquaredTextValuePayload;
  return result;
}

/**
 * The lexer for parsing recipe expressions with S-squared syntax.
 * Supports parsing free text and S-squared expressions delimited by `((` and `))`.
 * Handles atoms, numbers (integers, decimals, fractions), quoted strings, and text literals.
 */
export const RecipeExpressionLexer = new Lexer({
  modes: {
    [FreeTextMode]: [
      FreeTextWhitespaceTokenDef,
      S_SquaredOpeningTokenDef,
      FreeTextLiteralTokenDef,
    ],
    [S_SquaredExpressionMode]: [
      S_SquaredQuotedStringTokenDef,
      S_SquaredIntegerTokenDef,
      S_SquaredFractionDef,
      S_SquaredDecimalTokenDef,
      S_SquaredAtomTokenDef,
      S_SquaredWhitespaceTokenDef,
      S_SquaredClosingTokenDef,
      S_SquaredTextLiteralTokenDef,
    ],
  },
  defaultMode: FreeTextMode,
});
