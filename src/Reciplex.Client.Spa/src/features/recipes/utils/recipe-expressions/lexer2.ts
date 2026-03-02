import {
  createToken,
  CstParser,
  EmbeddedActionsParser,
  Lexer,
  type CstNode,
  type ILexingResult,
  type ParserMethod,
} from "chevrotain";

const ModeExpressionBody = "expression-body";
const ModeOutsideRecipeExpression = "outside-recipe-expression";

const EndRecipeExpression = createToken({
  name: "EndRecipeExpression",
  pattern: /\s\)\)/,
  pop_mode: true,
});
const StartRecipeExpression = createToken({
  name: "StartRecipeExpression",
  pattern: /\(\(\s/,
  push_mode: ModeExpressionBody,
});
const OutsideRecipeExpression = createToken({
  name: "OutsideRecipeExpression",
  pattern: /(?:[^(]|\(\(\S)+/,
  longer_alt: [StartRecipeExpression],
});

/** matches whitespace */
const WhiteSpace = createToken({
  name: "WhiteSpace",
  pattern: /\s+/,
});

/** Matches unquoted literals */
const UnquotedLiteral = createToken({
  name: "UnquotedLiteral",
  pattern: /\S+/,
});
/** matches decimals `0.1`, `0.2`, .. `1.1`, `1.2`, ... */
const Decimal = createToken({
  name: "Decimal",
  pattern: /([0-9]*)\.[0-9][0-9]*/,
  longer_alt: [UnquotedLiteral],
});

/** matches integers: `0`, `1`, `2`, ... */
const Integer = createToken({
  name: "Integer",
  pattern: /[0-9]([0-9]*)/,
  longer_alt: [UnquotedLiteral],
});

const Fraction = createToken({
  name: "Fraction",
  pattern: /([0-9][0-9]*)\/([0-9][0-9]*)/,
  longer_alt: [UnquotedLiteral],
});

const UnquotedIdentifier = createToken({
  name: "UnquotedIdentifier",
  pattern: /[a-zA-Z][a-zA-Z0-9]*/,
  longer_alt: [UnquotedLiteral],
});

const UnquotedIdentifierWithLink = createToken({
  name: "UnquotedIdentifierWithLink",
  pattern: /[a-zA-Z][a-zA-Z0-9]*\$\S+/,
  longer_alt: [UnquotedLiteral],
});

// /** matches `((` */
// const BeginRecipeExpression = createToken({
//   name: "BeginRecipeExpression",
//   pattern: /\(\(/,
//   longer_alt: [NotWhitespace],
// });
// /** matches `))` */
// const EndRecipeExpression = createToken({
//   name: "EndRecipeExpression",
//   pattern: /\)\)/,
//   longer_alt: [NotWhitespace],
// });

const QuotedLiteral = createToken({
  name: "QuotedLiteral",
  pattern: /"(?:[^"()]|""|"\)|"\()*"/,
});

//\(\(\s(?:[^\(\)]|"\(|"\))*?\s\)\)

// const allTokens = [
//   QuotedLiteral,
//   WhiteSpace,
//   BeginRecipeExpression,
//   EndRecipeExpression,
//   Decimal,
//   Fraction,
//   Integer,
//   NotWhitespace,
// ];

export const RecipeExpressionLexer = new Lexer({
  modes: {
    [ModeOutsideRecipeExpression]: [
      StartRecipeExpression,
      OutsideRecipeExpression,
    ],
    [ModeExpressionBody]: [
      EndRecipeExpression,
      QuotedLiteral,
      WhiteSpace,
      UnquotedIdentifierWithLink,
      UnquotedIdentifier,
      Decimal,
      Fraction,
      Integer,
      UnquotedLiteral,
    ],
  },
  defaultMode: ModeOutsideRecipeExpression,
});

export function ParseLexerSequence(lexerResult: ILexingResult) {}
/* eslint-disable @typescript-eslint/no-unsafe-declaration-merging */
export interface RecipeParser extends CstParser {
  anyValues: ParserMethod<unknown[], CstNode>;
  commandLink: ParserMethod<unknown[], CstNode>;
  fullCommand: ParserMethod<unknown[], CstNode>;
  recipeExpression: ParserMethod<unknown[], CstNode>;

  beforeExpression: ParserMethod<unknown[], CstNode>;

  afterExpression: ParserMethod<unknown[], CstNode>;
}

export class RecipeParser extends CstParser {
  /* eslint-enable @typescript-eslint/no-unsafe-declaration-merging */
  constructor() {
    super(
      [
        StartRecipeExpression,
        OutsideRecipeExpression,
        EndRecipeExpression,
        QuotedLiteral,
        WhiteSpace,
        UnquotedIdentifierWithLink,
        UnquotedIdentifier,
        Decimal,
        Fraction,
        Integer,
        UnquotedLiteral,
      ],
      {
        nodeLocationTracking: "full",
      },
    );

    this.RULE("recipeExpression", () => {
      this.SUBRULE(this.beforeExpression);
      this.OR([
        { ALT: () => this.SUBRULE(this.commandLink) },
        { ALT: () => this.SUBRULE(this.fullCommand) },
      ]);
      this.SUBRULE(this.afterExpression);
      //this.MANY(() => this.CONSUME(OutsideRecipeExpression));

      //   this.OR([
      //     {
      //       ALT: () => {
      //         this.MANY(() => this.CONSUME(OutsideRecipeExpression));
      //         this.CONSUME(OutsideRecipeExpression);
      //         this.CONSUME(StartRecipeExpression);
      //       },
      //     },
      //     { ALT: () => this.CONSUME(StartRecipeExpression) },
      //   ]);
    });

    this.RULE("anyValues", () => {
      this.MANY(() => {
        this.SUBRULE(this.anyValue);
      });
    });

    this.RULE("commandLink", () => {
      this.CONSUME(UnquotedIdentifierWithLink);
      this.SUBRULE(this.anyValues);
    });

    this.RULE("fullCommand", () => {
      this.CONSUME(UnquotedIdentifier);
      //this.SUBRULE(this.anyValues);
      this.MANY(() => {
        this.SUBRULE(this.anyValue);
      });
    });

    this.RULE("afterExpression", () => {
      this.CONSUME(EndRecipeExpression);
      this.MANY(() => this.CONSUME(OutsideRecipeExpression));
    });

    this.RULE("beforeExpression", () => {
      this.MANY(() => this.CONSUME(OutsideRecipeExpression));
      this.CONSUME(StartRecipeExpression);
    });

    this.RULE("anyValue", () => {
      this.OR([
        { ALT: () => this.CONSUME(QuotedLiteral) },
        { ALT: () => this.CONSUME(WhiteSpace) },
        { ALT: () => this.CONSUME(UnquotedIdentifierWithLink) },
        { ALT: () => this.CONSUME(UnquotedIdentifier) },
        { ALT: () => this.CONSUME(Decimal) },
        { ALT: () => this.CONSUME(Fraction) },
        { ALT: () => this.CONSUME(Integer) },
        { ALT: () => this.CONSUME(UnquotedLiteral) },
      ]);
    });

    this.performSelfAnalysis();
  }
}

const parserInstance = new RecipeParser();

const BaseCstVisitor = parserInstance.getBaseCstVisitorConstructor();

export class RecipeCstVisitor extends BaseCstVisitor {
  constructor() {
    super();
    // This helper will detect any missing or redundant methods on this visitor
    this.validateVisitor();
  }

  anyValue(ctx) {
    console.log(ctx);
    if (ctx.children) {
      console.log(ctx.children[Object.keys(ctx.children)[0]].image);
    }
    //console.log(ctx.children[Object.keys(ctx.children)[0]].image);
  }
  recipeExpression(ctx) {
    this.visit(ctx.beforeExpression);
    this.visit(ctx.fullCommand);
    this.visit(ctx.commandLink);
    this.visit(ctx.afterExpression);

    //console.log(ctx);
  }
  anyValues(ctx) {
    ctx.anyValue.map((n) => this.visit(n));
    // console.log(ctx);
  }
  commandLink(ctx) {
    // console.log(ctx);
  }
  fullCommand(ctx) {
    this.visit(ctx.anyValues);
    ctx.anyValue.map((n) => this.visit(n));
    //console.log(ctx);
  }
  afterExpression(ctx) {
    // console.log(ctx);
    console.log(" )) ");
  }
  beforeExpression(ctx) {
    // console.log(ctx);
    console.log("(( ");
  }
}
