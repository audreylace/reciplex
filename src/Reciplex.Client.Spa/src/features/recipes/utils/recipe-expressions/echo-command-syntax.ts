import { isQuotedStringToken } from "./expression-lexer";
import type { ICommandParser, IRExpCommand } from "./expression-syntax-types";

/**
 * An echo command
 */
export interface IRExpEchoCommand extends IRExpCommand<"echo"> {
  /**
   * the text to write
   */
  text: string;
}

/**
 * Handles an echo command
 */
export const echoCommandParser: ICommandParser<"echo", IRExpEchoCommand> = {
  selector: (atom) => {
    const atomValue = atom.payload.text;
    switch (atomValue) {
      case "e":
      case "echo":
        return true;
      default:
        return false;
    }
  },
  argumentExtractor: (_atom, args) => {
    if (args.length !== 1 || !isQuotedStringToken(args[0])) {
      return null;
    }

    return { text: args[0].payload.text, tag: "echo" };
  },
};
