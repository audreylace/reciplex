import { StringIterator } from "./string-iterator";

/**
 * common interface for all lexer symbols.
 * Parsers can derive new interfaces from this interface
 * to hold additional data as desired.
 */
export interface LexerSymbol {
  /** string representing the type of the lexer symbol */
  type: string;
  /** the raw matched value */
  rawValue: string;
  /** the start index of the symbol in the source string */
  index: number;
}

/**
 * the matcher outcome
 */
type MatcherOutcome = "continue" | "match" | "abort";
/**
 * A matcher closure that is executed on a sequence of characters
 * to determine if they are a match. Matchers are low level primitives
 * for building more complex grammar.
 * @param character the current character or null if the end of the string has been hit
 * @returns the match outcome. All characters up to but not including `character` will be considered
 * matched when `match` is returned. This means
 * when matching a given sequence of N characters, the method should run N+1
 * times.
 */
export interface Matcher {
  (character: string | null): MatcherOutcome;
}

/**
 * uses a pure delegate to match a non-zero length sequence of characters
 * @param test the delegate that is assumed to be pure with no side effects
 * @returns the matcher to execute
 */
export function pureMatch(test: (character: string) => boolean): Matcher {
  let isFirst = true;
  return (character: string | null) => {
    if (character === null) {
      return isFirst ? "abort" : "match";
    }
    if (test(character)) {
      isFirst = false;
      return "continue";
    }

    if (isFirst) {
      return "abort";
    }

    return "match";
  };
}

/**
 * matches a sequence of whitespace characters
 * @returns the matcher to execute
 */
export function matchAnyWhitespace(): Matcher {
  return pureMatch((c: string) => /\\s/.test(c));
}

/**
 * matches a sequence of non-whitespace characters
 * @returns the matcher to execute
 */
export function matchAnyNonWhitespace(): Matcher {
  return pureMatch((c: string) => /\\S/.test(c));
}

/**
 * matches a constant
 * @param constant the const to match
 * @returns the matcher to execute
 */
export function matchConstant(constant: string): Matcher {
  if (constant.length === 0) {
    throw new Error("can not match a 0 length sequence");
  }

  const iterator = new StringIterator(constant);
  return (character: string | null) => {
    if (!iterator.hasValue) {
      return "match";
    }

    if (character === null || iterator.current !== character) {
      return "abort";
    }

    iterator.moveNext();
    return "continue";
  };
}

/**
 * A lexer rule
 */
export interface LexerRule {
  /**
   * Executes the rule against the iterator.
   * @param iterator the string iterator
   * @returns the stream of lexer symbols if any. If the rule
   * did not match then the iterator position should be advanced. If it did
   * match, then the iterator should be moved by the N consumed characters.
   */
  execute(iterator: StringIterator): LexerSymbol[];
}

// const ing = [
//   matchConstant("{{i"),
//   matchAnyWhitespace(),
//   consumeUntil(matchConstant("}}")),
// ];

function consumeUntil(type: string, matcherFactory: () => Matcher): LexerRule {
  return {
    execute: (iterator: StringIterator) => {
      const unmatchedIterator = iterator.clone();
      do {
        const matcher = matcherFactory();
        const matcherIterator = unmatchedIterator.clone();
        do {
          /** todo - run matcher loop */
        } while (matcherIterator.moveNext());
      } while (unmatchedIterator.moveNext());

      return [];
    },
  };
}
