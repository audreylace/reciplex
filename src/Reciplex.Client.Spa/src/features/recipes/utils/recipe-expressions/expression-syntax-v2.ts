import {
  echoCommandParser,
  type IRExpEchoCommand,
} from "./echo-command-syntax";
import {
  isAtomToken,
  isClosingToken,
  isNumberLikeToken,
  isOpeningToken,
  isOutsideLikeToken,
  isTextLikeToken,
  recipeExpressionLexer,
  type ReciplexExpressionToken,
} from "./expression-lexer";
import type { CommandParserRExpTokens } from "./expression-syntax-types";
import {
  ingredientCommandParser,
  type IRExpIngredientCommand,
} from "./ingredient-command-syntax";
import {
  toolCommandParser,
  type IRExpToolCommand,
} from "./tool-command-syntax";

export type RExpCommand =
  | IRExpEchoCommand
  | IRExpIngredientCommand
  | IRExpToolCommand;

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

  for (const parser of [
    echoCommandParser,
    ingredientCommandParser,
    toolCommandParser,
  ]) {
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
): args is CommandParserRExpTokens[] {
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
