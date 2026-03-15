import Markdown, { type Options as ReactMarkdownOptions } from "react-markdown";
import rehypeSanitize, { defaultSchema } from "rehype-sanitize";
import detailsRenderStyleModule from "./details-render.module.css";
import { useContext, useEffect, useMemo, useState } from "preact/hooks";
import { unified } from "unified";
import remarkParse from "remark-parse";
import type { Root } from "mdast";
import { createContext } from "preact";
import {
  RecipeAstElementAttributes,
  RecipeAstElements,
  RecipeAstToHastHandlers,
  recipeExpressionSyntaxPlugin,
  recipeSyntaxMarkDownTreeVisiter,
  type RecipeExpressionAstNode,
} from "../../utils/recipe-expressions/md-to-expressions-v2";

interface IIngredientListEntry {
  title: string;
  amount?: number;
  unit?: string;
  key: string;
}

/**
 * The model for the recipe details context
 */
interface RecipeDetailsContextModel {
  /**
   * The raw markdown text
   */
  markdownText: string;
  /**
   * the markdown AST tree root
   */
  mdAstRoot: Root;
  ingredientCollection: RecipeIngredientConceptCollection;
}

/**
 * The recipe details context
 */
const RecipeDetailsContext = createContext<RecipeDetailsContextModel | null>(
  null,
);

class RecipeIngredientConceptCollection {
  private _ingredientConceptByPosition: Record<
    number,
    RecipeIngredientConcept
  > = {};
  private _ingredientConceptByKey: Record<string, RecipeIngredientConcept[]> =
    {};
  private _ingredientConceptByUserSuppliedId: Record<
    string,
    RecipeIngredientConcept[]
  > = {};

  public addAstNode(node: RecipeExpressionAstNode) {
    const command = node.data.command;
    let conceptArr;
    if (typeof command.userSuppliedId === "string") {
      conceptArr =
        this._ingredientConceptByUserSuppliedId[command.userSuppliedId];
    } else {
      const ingredientKey =
        RecipeIngredientConcept.astNodesIngredientCollectionKey(node);
      conceptArr = this._ingredientConceptByKey[ingredientKey];
    }

    let concept = conceptArr?.find((c) => c.tryAddAstNode(node));
    if (!concept) {
      concept = new RecipeIngredientConcept(node);
      this.addConcept(concept);
    }
    this._ingredientConceptByPosition[node.data.position] = concept;
    return;
  }

  public getConceptByPosition(
    position: number,
  ): RecipeIngredientConcept | undefined {
    return this._ingredientConceptByPosition[position];
  }

  public getIngredientList(): IIngredientListEntry[] {
    const ingredientList: IIngredientListEntry[] = [];
    for (const conceptKey in this._ingredientConceptByKey) {
      const localIngredientList: RecipeIngredientConcept[][] = [];
      const conceptArr = this._ingredientConceptByKey[conceptKey];

      for (const concept of conceptArr) {
        const group = localIngredientList.find((arr) =>
          arr[0].isSameIngredient(concept),
        );
        if (group) {
          group.push(concept);
        } else {
          localIngredientList.push([concept]);
        }
      }

      let index = 0;
      for (const groupArr of localIngredientList) {
        let quantity: number | undefined;
        if (typeof groupArr[0].totalQuantity === "number") {
          quantity = groupArr.reduce(
            (prev, c) => prev + (c.totalQuantity ?? 0),
            0,
          );
        }

        ingredientList.push({
          title: groupArr[0].name,
          unit: groupArr[0].unitText,
          amount: quantity,
          key: `${index++}\n${groupArr[0].ingredientCollectionKey}`,
        });
      }
    }

    return ingredientList;
  }

  private addConcept(concept: RecipeIngredientConcept) {
    const key = concept.ingredientCollectionKey;
    const userId = concept.useSuppliedId;

    concept.setKeyChangeListener(this.onIngredientKeyChange.bind(this));

    let conceptArr = this._ingredientConceptByKey[key];
    if (!conceptArr) {
      conceptArr = [];
      this._ingredientConceptByKey[key] = conceptArr;
    }
    conceptArr.push(concept);

    if (typeof userId === "string") {
      let userIdArr = this._ingredientConceptByUserSuppliedId[userId];
      if (!userIdArr) {
        userIdArr = [];
        this._ingredientConceptByUserSuppliedId[userId] = userIdArr;
      }
      userIdArr.push(concept);
    }
  }

  private onIngredientKeyChange(
    src: RecipeIngredientConcept,
    before: string,
    after: string,
  ) {
    if (before === after) {
      return;
    }

    const beforeArr = this._ingredientConceptByKey[before];
    if (beforeArr) {
      const index = beforeArr.indexOf(src);
      if (index !== -1) {
        beforeArr.splice(index, 1);
      }
    }

    let afterArr = this._ingredientConceptByKey[after];
    if (!afterArr) {
      afterArr = [];
      this._ingredientConceptByKey[after] = afterArr;
    }

    afterArr.push(src);
  }
}

class RecipeIngredientConcept {
  /**
   * All AST nodes representing this ingredient
   */
  private _astNodes: Record<number, RecipeExpressionAstNode> = {};

  /**
   * the name of the ingredient. Updated
   * as new nodes are added if `_userSuppliedId` is
   * set. Otherwise does not change. Last node
   * with a name wins.
   */
  private _name: string = "";

  /**
   * the user supplied ID. Set only from the constructor.
   */
  private _userSuppliedId: string | undefined;

  /**
   * If this node has the do not combine flag set.
   * When set, only nodes with the same `_userSuppliedId`
   * can be joined. If `_userSuppliedId` is undefined
   * then the node is a singleton.
   */
  private _doNotCombine: boolean = false;

  /**
   * The unit of the node. Updated
   * as new nodes are added if `_userSuppliedId` is
   * set. Otherwise does not change. Last node
   * with a name wins. `undefined` means
   * this ingredient does not have a quantity.
   */
  private _unit?: string;

  /**
   * the total quantity of this ingredient
   */
  private _quantity?: number;

  private _onKeyChange?: (
    src: RecipeIngredientConcept,
    oldKey: string,
    newKey: string,
  ) => void;

  constructor(node: RecipeExpressionAstNode) {
    const command = node.data.command;

    this._userSuppliedId = command.userSuppliedId;
    this._doNotCombine = command.doNotCombine ?? false;
    this._astNodes[node.data.position] = node;
    this._quantity = command.quantity?.amount;
    this._name =
      node.data.command.ingredientName ?? node.data.command.inlineText ?? "";
    this._unit = node.data.command.quantity?.unit;
  }

  /**
   * The name of the ingredient
   */
  public get name(): string {
    return this._name;
  }

  public get unitText(): string | undefined {
    if (this._unit === "-") {
      return "";
    }

    return this._unit;
  }

  public get totalQuantity(): number | undefined {
    return this._quantity;
  }

  public getAmountByPosition(position: number): number | undefined {
    if (typeof this._userSuppliedId === "string") {
      return this.totalQuantity;
    }

    return this._astNodes[position]?.data?.command?.quantity?.amount;
  }

  /**
   * Calculates the text to render for a node based on its position
   * @param position the node position
   * @returns the text to render
   */
  public getRenderText(position: number): string {
    return this._astNodes[position]?.data?.command?.inlineText ?? this.name;
  }

  public setKeyChangeListener(
    delegate: (
      src: RecipeIngredientConcept,
      oldKey: string,
      newKey: string,
    ) => void,
  ) {
    this._onKeyChange = delegate;
  }

  public clearKeyChangeListener() {
    this._onKeyChange = undefined;
  }

  /**
   * Computes the key to use in a collection. This
   * does not uniquely identify the ingredient but is
   * useful for quickly reducing the name of comparisons that
   * must be done.
   */
  public get ingredientCollectionKey(): string {
    return RecipeIngredientConcept.makeCollectionKey(
      this._doNotCombine,
      this._name,
      this._unit,
    );
  }

  public static astNodesIngredientCollectionKey(
    node: RecipeExpressionAstNode,
  ): string {
    const command = node.data.command;
    return RecipeIngredientConcept.makeCollectionKey(
      command.doNotCombine,
      command.ingredientName,
      command.quantity?.unit,
    );
  }

  private static makeCollectionKey(
    doNotCombine: boolean | undefined,
    name: string | undefined,
    unit: string | undefined,
  ): string {
    return `${doNotCombine ?? false}\n${name?.replaceAll("\n", "\\n") ?? ""}\n${unit?.replaceAll("\n", "\\n") ?? ""}`;
  }

  public get useSuppliedId(): string | undefined {
    return this._userSuppliedId;
  }

  /**
   * Checks if two ingredient facades represent the same ingredient.
   * The same ingredient can be split over multiple facades because
   * of user supplied ids. This method allows interested parties
   * to roll up these ingredients together.
   * @param other the other ingredient to check against
   * @returns true if they are the same. False otherwise.
   */
  public isSameIngredient(other: RecipeIngredientConcept) {
    if (this._doNotCombine) {
      return false;
    }
    if (other._doNotCombine) {
      return false;
    }

    if (this._name !== other._name) {
      return false;
    }

    if (this._unit !== other._unit) {
      return false;
    }

    return true;
  }

  /**
   * Attempts to add a ingredient ast node to this facade. The
   * facade will reject the node if the node does not belong to the same set.
   * @param node ast node to attempt to add
   */
  public tryAddAstNode(node: RecipeExpressionAstNode): boolean {
    const command = node.data.command;
    const nodeDoNotCombine = command.doNotCombine ?? false;
    // if this collection as an ID then new nodes must match with the same id
    if (command.userSuppliedId !== this._userSuppliedId) {
      return false;
    }
    // in the even an ID is set, then the two nodes
    // must belong to the same space as
    // controlled by the `doNotCombine` flags.
    else if (typeof command.userSuppliedId === "string") {
      if (nodeDoNotCombine === this._doNotCombine) {
        const oldKey = this.ingredientCollectionKey;

        // next named node overrides all previous
        if (!this._name) {
          this._name =
            command.ingredientName ?? command.inlineText ?? this._name;
        } else {
          this._name = command.ingredientName ?? this._name;
        }

        this._astNodes[node.data.position] = node;
        this._unit = command.quantity?.unit ?? this._unit;
        this._quantity = command.quantity?.amount ?? this._quantity;

        const newKey = this.ingredientCollectionKey;

        if (oldKey !== newKey) {
          this._onKeyChange?.(this, oldKey, newKey);
        }

        return true;
      }
      return false;
    }

    // node is in the do not combine space. Reject its inclusion.
    if (this._doNotCombine || command.doNotCombine) {
      return false;
    }

    // to combine a node they must have the same name
    if (command.ingredientName !== this._name) {
      return false;
    }

    // The nodes must have the same unit or no units
    if (command.quantity?.unit !== this._unit) {
      return false;
    }

    this._astNodes[node.data.position] = node;
    if (command.quantity) {
      this._quantity = (this._quantity ?? 0) + command.quantity.amount;
    }

    return true;
  }
}

/** renders the recipe details */
export function DetailsRender(props: DetailsRenderProps) {
  const [detailsContextModel, setDetailsContextModel] =
    useState<RecipeDetailsContextModel | null>(null);

  useEffect(() => {
    if (props.detailsMd) {
      const ingredientCollection = new RecipeIngredientConceptCollection();

      const processor = unified().use(remarkParse);
      const ast = processor.parse(props.detailsMd);
      recipeSyntaxMarkDownTreeVisiter(ast, (node) => {
        ingredientCollection.addAstNode(node);
      });

      setDetailsContextModel({
        markdownText: props.detailsMd,
        mdAstRoot: ast,
        ingredientCollection: ingredientCollection,
      });
      return;
    }
    setDetailsContextModel(null);
  }, [props.detailsMd]);

  return (
    <>
      <RecipeDetailsContext.Provider value={detailsContextModel}>
        <h4>Recipe Details</h4>
        <IngredientList />
        <div className={detailsRenderStyleModule.detailsWrapper}>
          <DetailsMdRender {...props} />
        </div>
      </RecipeDetailsContext.Provider>
    </>
  );
}

const customSchema = {
  ...defaultSchema,
  tagNames: [
    // @ts-expect-error expanding tagNames is the expected pattern for rehype-remark per their docs
    ...defaultSchema.tagNames,
    ...RecipeAstElements,
  ],
  attributes: {
    ...defaultSchema.attributes,
    ...RecipeAstElementAttributes,
  },
};

/** props for  `DetailsRender` */
export interface DetailsRenderProps {
  detailsMd?: string;
  mayEdit: boolean;
  goToEditAction: () => void;
}

function IngredientList() {
  const detailsContext = useContext(RecipeDetailsContext);

  const list = useMemo(() => {
    if (!detailsContext?.ingredientCollection) {
      return null;
    }

    return detailsContext.ingredientCollection.getIngredientList();
  }, [detailsContext?.ingredientCollection]);

  if (!list) {
    return null;
  }

  return (
    <>
      <ul>
        {list.map((listEntry) => {
          let unitString = "";
          if (listEntry.unit && listEntry.amount) {
            unitString = ` - ${listEntry.amount} ${listEntry.unit}`;
          } else if (listEntry.amount) {
            unitString = ` - ${listEntry.amount}`;
          }
          return (
            <li key={listEntry.key}>
              {listEntry.title}
              {unitString}
            </li>
          );
        })}
      </ul>
    </>
  );
}

const markdownComponents = {
  recipeExpression: RecipeIngredientRender,
} as ReactMarkdownOptions["components"];
const remarkRehypeHandlers = {
  handlers: RecipeAstToHastHandlers,
} as ReactMarkdownOptions["remarkRehypeOptions"];

/** inner md render */
function DetailsMdRender({
  detailsMd,
  mayEdit,
  goToEditAction,
}: DetailsRenderProps) {
  if (!detailsMd) {
    if (!mayEdit) {
      return (
        <p>
          <i>No details</i>
        </p>
      );
    }
    return (
      <p
        className={detailsRenderStyleModule.emptyDetails}
        onClick={goToEditAction}
      >
        <i>Click to edit and add details</i>
      </p>
    );
  }

  return (
    <Markdown
      rehypePlugins={[[rehypeSanitize, customSchema]]}
      remarkPlugins={[recipeExpressionSyntaxPlugin]}
      allowedElements={customSchema.tagNames}
      remarkRehypeOptions={remarkRehypeHandlers}
      components={markdownComponents}
    >
      {detailsMd}
    </Markdown>
  );
}

function RecipeIngredientRender({ position }: { position: number }) {
  const detailsContext = useContext(RecipeDetailsContext);

  if (!detailsContext) {
    return null;
  }

  const concept =
    detailsContext.ingredientCollection.getConceptByPosition(position);
  if (!concept) {
    return null;
  }

  const inlineText = concept.getRenderText(position);
  const unit = concept.unitText;
  const amount = concept.getAmountByPosition(position);

  if (amount) {
    return (
      <span className={detailsRenderStyleModule.ingredientText}>
        {amount} {unit}
        {" of "}
        {inlineText}
      </span>
    );
  }

  return (
    <span className={detailsRenderStyleModule.ingredientText}>
      {inlineText}
    </span>
  );
}
