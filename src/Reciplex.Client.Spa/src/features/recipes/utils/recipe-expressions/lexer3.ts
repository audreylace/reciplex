import {
  Alternation,
  Alternative,
  createToken,
  CstParser,
  GAstVisitor,
  Lexer,
  NonTerminal,
  Option,
  Repetition,
  RepetitionMandatory,
  RepetitionMandatoryWithSeparator,
  RepetitionWithSeparator,
  Rule,
  Terminal,
  tokenMatcher,
  type CstNode,
  type CustomPatternMatcherReturn,
  type IToken,
  type TokenType,
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

const FreeTextWhitespaceExpression = "FreeTextWhitespaceExpression";
const FreeTextWhitespaceToken = createToken({
  name: FreeTextWhitespaceExpression,
  pattern: /\s+/,
});

const SQuaredWhitespaceExpression = "SQuaredWhitespaceExpression";
const SQuaredWhitespaceExpressionToken = createToken({
  name: SQuaredWhitespaceExpression,
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
          tokenMatcher(
            matchedTokens[matchedTokens.length - 1],
            SQuaredWhitespaceExpressionToken,
          )
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

  if (tokenMatcher(lastToken, SQuaredWhitespaceExpressionToken)) {
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

const lexerModes: Record<string, TokenType[]> = {
  [freeTextMode]: [
    FreeTextWhitespaceToken,
    OpeningSQuaredExpressionToken,
    FreeTextExpressionToken,
  ],
  [sQuaredExpressionMode]: [
    StringExpressionToken,
    IntegerExpressionToken,
    FractionExpressionToken,
    DecimalExpressionToken,
    AtomExpressionToken,
    SQuaredWhitespaceExpressionToken,
    ClosingSQuaredExpressionToken,
    FreeTextLiteralToken,
  ],
};

const lexerTokens = Object.keys(lexerModes).reduce((prev, k) => {
  const list = lexerModes[k];
  list.forEach((t) => prev.push(t));
  return prev;
}, [] as TokenType[]);

export const RecipeExpressionLexer3 = new Lexer({
  modes: lexerModes,
  defaultMode: freeTextMode,
});

export class RecipeExpressionParser extends CstParser {
  constructor() {
    super(lexerTokens);

    this.RULE("recipeTextWithExpressions", () => {
      this.MANY(() => {
        this.SUBRULE(this.embeddedExpressionRule);
      });
      //   this.OR([
      //     {
      //       ALT: () => {
      //         this.MANY(() => {
      //           this.SUBRULE(this.embeddedExpressionRule);
      //         });
      //       },
      //     },
      //     {
      //       ALT: () => {
      //         this.MANY1(() => {
      //           this.OR1([
      //             { ALT: () => this.CONSUME(FreeTextExpressionToken) },
      //             { ALT: () => this.CONSUME(FreeTextWhitespaceToken) },
      //           ]);
      //         });
      //       },
      //     },
      //   ]);
    });

    this.RULE("embeddedExpressionRule", () => {
      this.MANY(() => {
        this.OR([
          { ALT: () => this.CONSUME(FreeTextExpressionToken) },
          { ALT: () => this.CONSUME(FreeTextWhitespaceToken) },
        ]);
      });
      this.SUBRULE(this.commandExpressionRule);
      this.MANY1(() => {
        this.OR1([
          { ALT: () => this.CONSUME1(FreeTextExpressionToken) },
          { ALT: () => this.CONSUME1(FreeTextWhitespaceToken) },
        ]);
      });
    });

    this.RULE("commandExpressionRule", () => {
      this.CONSUME(OpeningSQuaredExpressionToken);
      this.OR([
        { ALT: () => this.CONSUME(AtomExpressionToken) },
        {
          ALT: () => {
            this.CONSUME(SQuaredWhitespaceExpressionToken);
            this.CONSUME1(AtomExpressionToken);
          },
        },
      ]);
      this.MANY(() => {
        this.CONSUME1(SQuaredWhitespaceExpressionToken);
        this.SUBRULE(this.valueTokenRule);
      });

      this.OR1([
        { ALT: () => this.CONSUME(ClosingSQuaredExpressionToken) },
        {
          ALT: () => {
            this.CONSUME2(SQuaredWhitespaceExpressionToken);
            this.CONSUME1(ClosingSQuaredExpressionToken);
          },
        },
      ]);
    });

    this.RULE("valueTokenRule", () => {
      this.OR([
        { ALT: () => this.CONSUME(StringExpressionToken) },
        { ALT: () => this.CONSUME(IntegerExpressionToken) },
        { ALT: () => this.CONSUME(FractionExpressionToken) },
        { ALT: () => this.CONSUME(DecimalExpressionToken) },
        { ALT: () => this.CONSUME(FreeTextLiteralToken) },
      ]);
    });

    this.performSelfAnalysis();
  }
}

export const RecipeExpressionParserInstance = new RecipeExpressionParser();
export class RecipeExpressionVisitor extends RecipeExpressionParserInstance.getBaseCstVisitorConstructorWithDefaults() {
  constructor() {
    super();
    this.validateVisitor();
  }

  recipeTextWithExpressions(cstNode: CstNode) {
    this.visit(cstNode.embeddedExpressionRule);
  }

  embeddedExpressionRule(cstNode: CstNode) {
    this.visit(cstNode.commandExpressionRule);
  }
  commandExpressionRule(cstNode: CstNode) {
    if (cstNode.AtomExpression) {
      console.log(cstNode.AtomExpression[0].image);
    }
    //this.visit(cstNode.AtomExpression);
  }
}
