import type {
  RExpQuotedStringToken,
  RExpNumberToken,
  RExpFractionToken,
  RExpTextLiteralToken,
  RExpAtomToken,
} from "./expression-lexer";

/**
 * A quantity with a unit
 */
export interface IQuantityWithUnit {
  /**
   * the raw amount. Either user supplied or computed from the fraction.
   */
  amount: number;
  /**
   * the unit of the amount
   */
  unit: string;
  /**
   * The fraction used to compute `amount` if applicable
   */
  fraction?: { numerator: number; denominator: number };
}

/**
 * A quantity that never has a unit
 */
export interface IQuantityWithoutUnit {
  /**
   * the raw amount. Either user supplied or computed from the fraction.
   */
  amount: number;
  /**
   * The fraction used to compute `amount` if applicable
   */
  fraction?: { numerator: number; denominator: number };
}

/**
 * A expression command
 */
export interface IRExpCommand<TCommandTag extends string> {
  /**
   * the tag
   */
  tag: TCommandTag;
}

export type CommandParserRExpTokens =
  | RExpQuotedStringToken
  | RExpNumberToken
  | RExpFractionToken
  | RExpTextLiteralToken;

/**
 * Command handlers take the raw atom expression and convert them into
 * models.
 */
export interface ICommandParser<
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
    args: CommandParserRExpTokens[],
  ) => null | TCommand | false;
}
