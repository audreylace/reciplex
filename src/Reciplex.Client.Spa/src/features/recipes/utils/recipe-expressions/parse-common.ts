import {
  isNumberLikeToken,
  isTextLikeToken,
  isFractionToken,
  isQuotedStringToken,
} from "./expression-lexer";
import type {
  CommandParserRExpTokens,
  IQuantityWithoutUnit,
  IQuantityWithUnit,
} from "./expression-syntax-types";

export function parseQuantityWithUnit(
  args: CommandParserRExpTokens[],
  offset: number,
): [number, IQuantityWithUnit] | undefined {
  if (!isNumberLikeToken(args[offset])) {
    return;
  }

  if (!isTextLikeToken(args[offset + 1])) {
    return;
  }

  const unit = args[offset + 1].payload.text;
  if (isFractionToken(args[offset])) {
    return [
      offset + 2,
      {
        amount: args[offset].payload.realValue,
        unit,
        fraction: {
          numerator: args[offset].payload.numerator,
          denominator: args[offset].payload.denominator,
        },
      },
    ];
  }

  return [
    offset + 2,
    {
      unit,
      amount: args[offset].payload.realValue,
    },
  ];
}

export function parseQuantityWithoutUnit(
  args: CommandParserRExpTokens[],
  offset: number,
): [number, IQuantityWithoutUnit] | undefined {
  if (!isNumberLikeToken(args[offset])) {
    return;
  }

  if (isFractionToken(args[offset])) {
    return [
      offset + 1,
      {
        amount: args[offset].payload.realValue,
        fraction: {
          numerator: args[offset].payload.numerator,
          denominator: args[offset].payload.denominator,
        },
      },
    ];
  }

  return [
    offset + 1,
    {
      amount: args[offset].payload.realValue,
    },
  ];
}

export function parseStringTuple(
  args: CommandParserRExpTokens[],
  offset: number,
): [number, string, string] | [number, string] | undefined {
  if (
    isQuotedStringToken(args[offset]) &&
    isQuotedStringToken(args[offset + 1])
  ) {
    return [
      offset + 2,
      args[offset].payload.text,
      args[offset + 1].payload.text,
    ];
  } else if (isQuotedStringToken(args[offset])) {
    return [offset + 1, args[offset].payload.text];
  }
}
