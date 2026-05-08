import type {
  RecipeAstNodes,
  RecipeIngredientExpressionAstNode,
} from "../../utils/recipe-expressions/md-to-expressions-v2";
import {
  ConceptCollection,
  type IConcept,
  type IHandleRecipeAstNode,
  type INodeMetaData,
} from "./concept-collection";

/**
 * Ingredient concept gathering up a set of ingredient ast nodes
 */
export class RecipeIngredientConcept implements IConcept<
  RecipeIngredientExpressionAstNode,
  RecipeIngredientConcept
> {
  private _onKeyChange?: (oldKey: string, newKey: string) => void;
  /**
   * All AST nodes representing this ingredient stored by position index
   */
  private _astNodes: Record<number, RecipeIngredientExpressionAstNode> = {};
  /**
   * All ast nodes representing this ingredient stored as a list
   */
  private _astNodeList: RecipeIngredientExpressionAstNode[] = [];

  /**
   * the user supplied ID. Set only from the constructor.
   */
  private _userSuppliedId: string | undefined;

  /**
   * the name of the ingredient. Updated
   * as new nodes are added if `_userSuppliedId` is
   * set. Otherwise does not change. Last node
   * with a name wins.
   */
  private _name: string = "";

  /**
   * The unit of the node. Updated
   * as new nodes are added if `_userSuppliedId` is
   * set. Otherwise does not change. Last node
   * with a name wins. `undefined` means
   * this ingredient does not have a quantity.
   */
  private _unit?: string;

  /**
   * the quantity of this ingredient
   */
  private _quantity?: number;

  constructor(node: RecipeIngredientExpressionAstNode) {
    const command = node.data.command;

    this._userSuppliedId = command.userSuppliedId;
    this._astNodes[node.data.position] = node;
    this._astNodeList.push(node);
    this._quantity = command.quantity?.amount;
    this._name =
      node.data.command.ingredientName ?? node.data.command.inlineText ?? "";
    this._unit = node.data.command.quantity?.unit;
  }
  get astNodes(): RecipeIngredientExpressionAstNode[] {
    return this._astNodeList;
  }

  static factory(
    node: RecipeIngredientExpressionAstNode,
  ): RecipeIngredientConcept {
    return new RecipeIngredientConcept(node);
  }

  static getNodeMetaData(
    node: RecipeIngredientExpressionAstNode,
  ): INodeMetaData {
    return {
      conceptKey: RecipeIngredientConcept.makeCollectionKey(
        node.data.command.ingredientName,
        node.data.command.quantity?.unit,
      ),
      position: node.data.position,
      userId: node.data.command.userSuppliedId,
    };
  }

  setKeyChangeListener(
    delegate: (oldKey: string, newKey: string) => void,
  ): void {
    this._onKeyChange = delegate;
  }

  isSameConceptInList(other: RecipeIngredientConcept): boolean {
    if (this._name !== other._name) {
      return false;
    }

    if (this._unit !== other._unit) {
      return false;
    }

    return true;
  }

  tryAddAstNode(node: RecipeIngredientExpressionAstNode): boolean {
    const command = node.data.command;
    // if this collection as an ID then new nodes must match with the same id
    if (command.userSuppliedId !== this._userSuppliedId) {
      return false;
    }

    // in the even an ID is set, then the two nodes
    // must belong to the same space as
    // controlled by the `doNotCombine` flags.
    else if (typeof command.userSuppliedId === "string") {
      const oldKey = this.conceptKey;

      // next named node overrides all previous
      if (!this._name) {
        this._name = command.ingredientName ?? command.inlineText ?? this._name;
      } else {
        this._name = command.ingredientName ?? this._name;
      }

      this._astNodes[node.data.position] = node;
      this._unit = command.quantity?.unit ?? this._unit;
      this._quantity = command.quantity?.amount ?? this._quantity;
      this._astNodeList.push(node);
      const newKey = this.conceptKey;

      if (oldKey !== newKey) {
        this._onKeyChange?.(oldKey, newKey);
      }

      return true;
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
    this._astNodeList.push(node);
    if (command.quantity) {
      this._quantity = (this._quantity ?? 0) + command.quantity.amount;
    }

    return true;
  }

  getAstNodeByPosition(position: number): RecipeIngredientExpressionAstNode {
    return this._astNodes[position];
  }
  get userSuppliedId(): string | undefined {
    return this._userSuppliedId;
  }
  get conceptKey(): string {
    return RecipeIngredientConcept.makeCollectionKey(this._name, this._unit);
  }

  private static makeCollectionKey(
    name: string | undefined,
    unit: string | undefined,
  ): string {
    return `${name?.replaceAll("\n", "\\n") ?? ""}\n${unit?.replaceAll("\n", "\\n") ?? ""}`;
  }

  /**
   * The name of the ingredient represented by this concept
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
}

/**
 * a single ingredient entry in the list
 */
export interface IIngredientListEntry {
  /**
   * the title of the ingredient
   */
  title: string;
  /**
   * the amount of the ingredient
   */
  amount?: number;
  /**
   * the units of the ingredient
   */
  unit?: string;
  /**
   * A unique key of the entry in the list. Intended for rendering purposes.
   */
  key: string;
}

/**
 * Collection holding all ingredient concepts generated from AST nodes
 */
export class RecipeIngredientConceptCollection
  extends ConceptCollection<
    RecipeIngredientExpressionAstNode,
    RecipeIngredientConcept
  >
  implements IHandleRecipeAstNode
{
  /**
   * constructor of this type
   */
  constructor() {
    super(
      RecipeIngredientConcept.getNodeMetaData,
      RecipeIngredientConcept.factory,
    );
  }

  /**
   * @inheritdoc
   */
  tryAddNode(node: RecipeAstNodes): boolean {
    if (node.type !== "recipeIngredientExpression") {
      return false;
    }

    this.addAstNode(node);
    return true;
  }

  /**
   * computes the list of ingredients deduplicated
   * and ready for display to an end user.
   * @returns the ingredient list
   */
  public getIngredientList(): IIngredientListEntry[] {
    return this.computeList((groupArr, index) => {
      let quantity: number | undefined;
      if (typeof groupArr[0].totalQuantity === "number") {
        quantity = groupArr.reduce(
          (prev, c) => prev + (c.totalQuantity ?? 0),
          0,
        );
      }

      let unitText = groupArr[0].unitText;
      if (unitText === "-") {
        unitText = "";
      }
      return {
        title: groupArr[0].name,
        unit: unitText,
        amount: quantity,
        key: `${index++}\n${groupArr[0].conceptKey}`,
      };
    });
  }
}
