import {
  createToken,
  Lexer,
  tokenMatcher,
  type CustomPatternMatcherReturn,
  type IToken,
} from "chevrotain";

const S_SquaredExpressionMode = "s-squared-expression-mode";
const FreeTextMode = "free-text-mode";

export const S_SquaredOpeningToken = "s-squared-opening-tag";
/**
 * Matches an opening S-squared expression
 */
const S_SquaredOpeningTokenDef = createToken({
  name: S_SquaredOpeningToken,
  pattern: /\(\(/,
  push_mode: S_SquaredExpressionMode,
});

export const S_SquaredClosingToken = "s-squared-closing-tag";
/**
 * Matches a closing S-squared expression
 */
const S_SquaredClosingTokenDef = createToken({
  name: S_SquaredClosingToken,
  pattern: /\)\)/,
  pop_mode: true,
});

export const FreeTextLiteralToken = "free-text-literal";
const FreeTextLiteralRegex = /([^\s(]|(\([^\s(]))+/y;
/**
 * Matches free text outside of a S-squared expression
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

export interface S_SquaredQuotedStringPayload {
  textValue: string;
}
export const S_SquaredQuotedStringToken = "s-squared-quoted-string";
const S_SquaredQuotedStringRegex = /"(?:[^"]|"")*"/y;
/**
 * Matches a string expression
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

export interface S_SquaredTextValuePayload {
  text: string;
}
export interface S_SquaredRealValuePayload extends S_SquaredTextValuePayload {
  realValue: number;
}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface S_SquaredIntegerPayload extends S_SquaredRealValuePayload {}
export const S_SquaredIntegerToken = "s-squared-integer";
const S_SquaredIntegerRegex = /[0-9]([0-9]*)/y;
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

export interface S_SquaredFractionPayload extends S_SquaredRealValuePayload {
  numerator: number;
  denominator: number;
}
export const S_SquaredFractionToken = "s-squared-fraction";
const S_SquaredFractionRegex = /([0-9][0-9]*)\/([0-9][0-9]*)/y;
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

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface S_SquaredDecimalTokenPayload extends S_SquaredRealValuePayload {}
export const S_SquaredDecimalToken = "s-squared-decimal";
const S_SquaredDecimalRegex = /([0-9]*)\.[0-9][0-9]*/y;
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

export const FreeTextWhitespaceToken = "free-text-whitespace";
const FreeTextWhitespaceTokenDef = createToken({
  name: FreeTextWhitespaceToken,
  pattern: /\s+/,
  line_breaks: true,
});

const S_SquaredWhitespaceRegex = /\s+/y;
export const S_SquaredWhitespaceToken = "s-squared-whitespace";
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

export const S_SquaredTextLiteralToken = "s-squared-text-literal";
const S_SquaredTextLiteralRegex = /([^\s"]|(\)[^\s)]))+/y;
const S_SquaredTextLiteralTokenDef = createToken({
  name: FreeTextLiteralToken,
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

export const S_SquaredAtomToken = "s-squared-atom";
const S_SquaredAtomRegex = /[a-zA-Z][a-zA-Z0-9]*/y;
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

const isCharacterWhitespaceRegex = /\s/;
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
