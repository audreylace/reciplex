import {
  createToken,
  Lexer,
  tokenMatcher,
  type CustomPatternMatcherReturn,
  type IToken,
} from "chevrotain";

const sQuaredExpressionMode = "sQuaredExpression";
const freeTextMode = "freeText";

const OpeningSQuaredExpression = "OpeningSQuaredExpression";
/**
 * Matches an opening S-squared expression
 */
const OpeningSQuaredExpressionToken = createToken({
  name: OpeningSQuaredExpression,
  pattern: /\(\(/,
  push_mode: sQuaredExpressionMode,
});

const ClosingSQuaredExpression = "ClosingSQuaredExpression";
/**
 * Matches a closing S-squared expression
 */
const ClosingSQuaredExpressionToken = createToken({
  name: ClosingSQuaredExpression,
  pattern: /\)\)/,
  pop_mode: true,
});

const FreeTextExpression = "FreeTextExpression";
const freeTextRegex = /([^\s(]|(\([^\s(]))+/y;
/**
 * Matches free text outside of a S-squared expression
 */
const FreeTextExpressionToken = createToken({
  name: FreeTextExpression,
  pattern: {
    exec: (text, startOffset) => {
      freeTextRegex.lastIndex = startOffset;
      const match = freeTextRegex.exec(text);
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

const StringExpression = "StringExpression";
const stringExpressionRegex = /"(?:[^"]|"")*"/y;
/**
 * Matches a string expression
 */
const StringExpressionToken = createToken({
  name: StringExpression,
  start_chars_hint: ['"'],
  pattern: {
    exec: matchRegexWithStructure(stringExpressionRegex, (text) => {
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

      return processedValue;
    }),
  },
  line_breaks: false,
});

const IntegerExpression = "IntegerExpression";
const integerRegEx = /[0-9]([0-9]*)/y;
const IntegerExpressionToken = createToken({
  name: IntegerExpression,
  pattern: { exec: matchRegexWithStructure(integerRegEx, (s) => parseInt(s)) },
  start_chars_hint: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
  line_breaks: false,
});

const FractionExpression = "FractionExpression";
const fractionRegex = /([0-9][0-9]*)\/([0-9][0-9]*)/y;
const FractionExpressionToken = createToken({
  name: FractionExpression,
  pattern: {
    exec: matchRegexWithStructure(fractionRegex, (s) => {
      const pieces = s.split("/");
      return {
        numerator: parseInt(pieces[0]),
        denominator: parseInt(pieces[1]),
        realValue: parseInt(pieces[0]) / parseInt(pieces[1]),
      };
    }),
  },
  start_chars_hint: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
  line_breaks: false,
});

const DecimalExpression = "DecimalExpression";
const decimalRegex = /([0-9]*)\.[0-9][0-9]*/y;
const DecimalExpressionToken = createToken({
  name: DecimalExpression,
  pattern: {
    exec: matchRegexWithStructure(decimalRegex, (s) => {
      return parseFloat(s);
    }),
  },
  start_chars_hint: [".", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
  line_breaks: false,
});

const WhiteSpaceExpression = "WhiteSpaceExpression";
const WhiteSpaceToken = createToken({
  name: WhiteSpaceExpression,
  pattern: /\s+/,
});

const FreeTextLiteralExpression = "FreeTextLiteralExpression";
const freeTextLiteralRegex = /([^\s"]|(\)[^\s)]))+/y;
const FreeTextLiteralToken = createToken({
  name: FreeTextLiteralExpression,
  pattern: {
    exec: matchRegexWithStructure(freeTextLiteralRegex, undefined, (result) => {
      const matchedString = result[0];
      if (matchedString.length < 2) {
        return result;
      }

      const lastTwoChars = matchedString.substring(
        matchedString.length - 2,
        matchedString.length,
      );
      if (lastTwoChars === "))") {
        return [matchedString.substring(0, matchedString.length - 2)];
      }

      return result;
    }),
  },
  line_breaks: false,
});

const AtomExpression = "AtomExpression";
const atomRegex = /[a-zA-Z][a-zA-Z0-9]*/y;
const AtomExpressionToken = createToken({
  name: AtomExpression,
  pattern: {
    exec: (text, startOffset, matchedTokens) => {
      const isSExp = (offset: number) => {
        return tokenMatcher(
          matchedTokens[matchedTokens.length - offset],
          OpeningSQuaredExpressionToken,
        );
      };
      if (matchedTokens.length < 1) {
        return null;
      } else if (matchedTokens.length == 1 && !isSExp(1)) {
        return null;
      } else if (matchedTokens.length >= 2) {
        if (
          isSExp(2) &&
          tokenMatcher(matchedTokens[matchedTokens.length - 1], WhiteSpaceToken)
        ) {
          /* empty */
        } else if (isSExp(1)) {
          /* empty */
        } else {
          return null;
        }
      }
      atomRegex.lastIndex = startOffset;
      const match = atomRegex.exec(text);
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

  if (tokenMatcher(lastToken, WhiteSpaceToken)) {
    return true;
  }

  if (tokenMatcher(lastToken, OpeningSQuaredExpressionToken)) {
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

export const RecipeExpressionLexer3 = new Lexer({
  modes: {
    [freeTextMode]: [
      WhiteSpaceToken,
      OpeningSQuaredExpressionToken,
      FreeTextExpressionToken,
    ],
    [sQuaredExpressionMode]: [
      StringExpressionToken,
      IntegerExpressionToken,
      FractionExpressionToken,
      DecimalExpressionToken,
      AtomExpressionToken,
      WhiteSpaceToken,
      ClosingSQuaredExpressionToken,
      FreeTextLiteralToken,
    ],
  },
  defaultMode: freeTextMode,
});
