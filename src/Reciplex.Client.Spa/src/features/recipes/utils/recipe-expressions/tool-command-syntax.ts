import { isTextLiteralToken, type RExpAtomToken } from "./expression-lexer";
import type {
  CommandParserRExpTokens,
  ICommandParser,
  IQuantityWithoutUnit,
  IQuantityWithUnit,
  IRExpCommand,
} from "./expression-syntax-types";
import {
  parseQuantityWithoutUnit,
  parseQuantityWithUnit,
  parseStringTuple,
} from "./parse-common";

/**
 * Tool command.
 *
 * Atoms:
 *  - tool - normal tool command
 *  - t - short hand for tool
 *
 * Syntax:
 *  - A tool with size
 *    `(( t 12 cup "stock pot" "large stock pot"))`
 *  - A tool without size but quantity:
 *    `(( tQ 3 "spoons" "do not reuse spoons between the 3 batches" ))`
 *  - A tool without size or quantity
 *    `(( t "timer" "set a timer to 60 minutes" ))`
 *  - A tool with both size and quantity
 *    `(( tQ 3 1/2 tbsp "spoons" "do not reuse spoons between the 3 batches" ))`
 *  - All tools support linkage. Use a text literal after the atom.
 *    `(( t BREAD_TIMER "timer" "set the timer to 60 minutes" )) followed by
 *    `(( t BREAD_TIMER "make sure to set the timer!"))
 *
 * Tools are added together. So writing (( t "spoon" )) (( t "spoon" )) will resolve to the tool
 * saying that the recipe calls for two spoons. To prevent this, assign them the same linkage.
 * Eg, `(( t SPOON "spoon" )) (( t SPOON "spoon" ))` or more concisely written
 * as `(( t SPOON "spoon" )) (( t SPOON ))`.
 *
 * Adding groups by name and the size if set.
 *
 */
export interface IRExpToolCommand extends IRExpCommand<"tool"> {
  /**
   * The name of the tool
   */
  toolName?: string;
  /**
   * Local text to render inline
   */
  inlineText?: string;
  /**
   * The size of this tool. Example, 12 quart pot.
   */
  size?: IQuantityWithUnit;
  /**
   * The number of unique instances. For example, 3 spoons
   */
  amount?: IQuantityWithoutUnit;
  /**
   * The unique user supplied id of this tool
   */
  userSuppliedId?: string;
}

/**
 * Handles a tool command
 */
export const toolCommandParser: ICommandParser<"tool", IRExpToolCommand> = {
  selector: (atom) => {
    const atomValue = atom.payload.text.toLowerCase();
    switch (atomValue) {
      case "t":
      case "tool":
        return true;
      case "tq":
      case "tquantity":
      case "toolquantity":
        return true;
      default:
        return false;
    }
  },
  argumentExtractor: (atomToken, args) => {
    let argIndex = 0;

    // UserId argument
    let userSuppliedIdProperty: IRExpToolCommand["userSuppliedId"] | undefined;
    if (isTextLiteralToken(args[argIndex])) {
      userSuppliedIdProperty = args[argIndex].payload.text;
      argIndex++;
    }

    // Quantity argument if this is a tool with quantity command
    let amountProperty: IRExpToolCommand["amount"];
    const amountParseResult = parseToolQuantityIfApplicable(
      atomToken,
      args,
      argIndex,
    );
    if (!amountParseResult) {
      if (amountParseResult === false) {
        return false;
      }
    } else {
      amountProperty = amountParseResult[1];
      argIndex = amountParseResult[0];
    }

    // extract size if it has one
    let sizeProperty: IRExpToolCommand["size"];
    const parseAmountResult = parseQuantityWithUnit(args, argIndex);
    if (parseAmountResult) {
      argIndex = parseAmountResult[0];
      sizeProperty = parseAmountResult[1];
    }

    // get text and inline text
    const stringTuple = parseStringTuple(args, argIndex);
    let inlineTextProperty: IRExpToolCommand["inlineText"];
    let toolNameProperty: IRExpToolCommand["toolName"];
    if (!stringTuple) {
      if (!userSuppliedIdProperty) {
        return false;
      }
    } else {
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      const [_, firstString, secondString] = stringTuple;
      argIndex = stringTuple[0];
      if (userSuppliedIdProperty) {
        if (typeof secondString === "string") {
          toolNameProperty = firstString;
          inlineTextProperty = secondString;
        } else {
          inlineTextProperty = firstString;
        }
      } else {
        toolNameProperty = firstString;
        inlineTextProperty = secondString;
      }
    }

    if (argIndex !== args.length) {
      // EXPECT: End on last token
      return false;
    }

    return {
      tag: "tool",
      userSuppliedId: userSuppliedIdProperty,
      amount: amountProperty,
      quantity: amountProperty,
      toolName: toolNameProperty,
      inlineText: inlineTextProperty,
      size: sizeProperty,
    } as IRExpToolCommand;
  },
};

/**
 * parses a `IQuantityWithoutUnit` with extra rules specific
 * to a tool command
 * @param atom the atom token since that changes the behavior of the parser
 * @param args arguments for the command
 * @param offset offset into args
 * @returns a tuple. First is the new index, and the second is the parsed model. Undefined is returned
 * if this expression does not have a quantity. `false` if the expression should have a quantity but it is malformed
 * or lacking.
 */
function parseToolQuantityIfApplicable(
  atom: RExpAtomToken,
  args: CommandParserRExpTokens[],
  offset: number,
): [number, IQuantityWithoutUnit] | undefined | false {
  const atomName = atom.payload.text.toLowerCase();

  if (
    atomName !== "tq" &&
    atomName !== "toolquantity" &&
    atomName !== "tquantity"
  ) {
    return;
  }

  return (
    parseQuantityWithoutUnit(args, offset) ??
    // EXPECT: defined model so undefined becomes false to signal error
    false
  );
}
