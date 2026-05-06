import type {
  RecipeAstNodes,
  RecipeToolExpressionAstNode,
} from "../../utils/recipe-expressions/md-to-expressions-v2";
import {
  ConceptCollection,
  type IConcept,
  type IHandleRecipeAstNode,
  type INodeMetaData,
  type KeyChangeListenerDelegate,
} from "./concept-collection";

export class RecipeToolConcept implements IConcept<
  RecipeToolExpressionAstNode,
  RecipeToolConcept
> {
  constructor(node: RecipeToolExpressionAstNode) {
    this._userSuppliedId = node.data.command.userSuppliedId;
    this._toolName =
      node.data.command.toolName ?? node.data.command.inlineText ?? "";
    this._toolUnit = node.data.command.size?.unit;
    this._toolSize = node.data.command.size?.amount;
    this._totalQuantity = node.data.command.amount?.amount ?? 1;
    this._nodeByPosition[node.data.position] = node;
    this._astNodesList.push(node);
  }

  private _userSuppliedId?: string;
  private _nodeByPosition: Record<number, RecipeToolExpressionAstNode> = {};
  private _astNodesList: RecipeToolExpressionAstNode[] = [];
  private _onKeyChange?: (oldKey: string, newKey: string) => void;
  private _toolName: string;
  private _toolUnit?: string;
  private _toolSize?: number;
  private _totalQuantity: number;

  public get toolName(): string {
    return this._toolName;
  }

  public get toolSizeUnit(): string | undefined {
    return this._toolUnit;
  }

  public get toolSize(): number | undefined {
    return this._toolSize;
  }

  public get totalQuantity(): number {
    return this._totalQuantity;
  }

  public getToolQuantityByPosition(position: number): number | undefined {
    if (this._userSuppliedId) {
      return this._totalQuantity;
    }

    const node = this._nodeByPosition[position];
    if (!node) {
      return undefined;
    }

    return node.data.command.amount?.amount ?? 1;
  }

  setKeyChangeListener(delegate: KeyChangeListenerDelegate): void {
    this._onKeyChange = delegate;
  }
  isSameConceptInList(other: RecipeToolConcept): boolean {
    return other.conceptKey === this.conceptKey;
  }
  tryAddAstNode(node: RecipeToolExpressionAstNode): boolean {
    const command = node.data.command;

    if (
      typeof command.userSuppliedId === "string" &&
      typeof this._userSuppliedId === "string"
    ) {
      if (command.userSuppliedId !== this._userSuppliedId) {
        return false;
      }

      this.addNode(node);
      return true;
    }

    const nodeData = RecipeToolConcept.getNodeMetaData(node);
    if (nodeData.conceptKey !== this.conceptKey) {
      return false;
    }
    this.addNode(node);
    return true;
  }
  getAstNodeByPosition(
    position: number,
  ): RecipeToolExpressionAstNode | undefined {
    return this._nodeByPosition[position];
  }
  get userSuppliedId(): string | undefined {
    return this._userSuppliedId;
  }
  get conceptKey(): string {
    return RecipeToolConcept.computeConceptKey(
      this._toolName,
      this._toolUnit,
      this._toolSize,
    );
  }
  get astNodes(): RecipeToolExpressionAstNode[] {
    return this._astNodesList;
  }
  public getInlineText(position: number): string | undefined {
    return this._nodeByPosition[position]?.data?.command?.inlineText;
  }
  static factory(node: RecipeToolExpressionAstNode): RecipeToolConcept {
    return new RecipeToolConcept(node);
  }
  static getNodeMetaData(node: RecipeToolExpressionAstNode): INodeMetaData {
    return {
      conceptKey: RecipeToolConcept.computeConceptKey(
        node.data.command.toolName,
        node.data.command.size?.unit,
        node.data.command.size?.amount,
      ),
      position: node.data.position,
      userId: node.data.command.userSuppliedId,
    };
  }

  private addNode(node: RecipeToolExpressionAstNode): void {
    this._nodeByPosition[node.data.position] = node;
    this._astNodesList.push(node);

    if (this._userSuppliedId) {
      const oldKey = this.conceptKey;

      const command = node.data.command;
      // next named node overrides all previous
      if (!this._toolName) {
        this._toolName =
          command.toolName ?? command.inlineText ?? this._toolName;
      } else {
        this._toolName = command.toolName ?? this._toolName;
      }

      this._totalQuantity =
        node.data.command.amount?.amount ?? this._totalQuantity;
      this._toolSize = node.data.command.size?.amount ?? this._toolSize;
      this._toolUnit = node.data.command.size?.unit ?? this._toolUnit;

      if (oldKey !== this.conceptKey) {
        this._onKeyChange?.(oldKey, this.conceptKey);
      }
    } else {
      this._totalQuantity += node.data.command.amount?.amount ?? 0;
    }
  }

  private static computeConceptKey(
    toolName?: string,
    unit?: string,
    size?: number,
  ) {
    return `${toolName?.replaceAll("\n", "\\n") ?? ""}\n${unit?.replaceAll("\n", "\\n") ?? ""}\n${size}`;
  }
}

export interface IToolListEntry {
  toolName: string;
  quantity: number;
  size?: number;
  sizeUnit?: string;
  key: string;
}

/**
 * Collection holding all ingredient concepts generated from AST nodes
 */
export class RecipeToolConceptCollection
  extends ConceptCollection<RecipeToolExpressionAstNode, RecipeToolConcept>
  implements IHandleRecipeAstNode
{
  /**
   * constructor of this type
   */
  constructor() {
    super(RecipeToolConcept.getNodeMetaData, RecipeToolConcept.factory);
  }

  /**
   * @inheritdoc
   */
  tryAddNode(node: RecipeAstNodes): boolean {
    if (node.type !== "recipeToolExpression") {
      return false;
    }

    this.addAstNode(node);
    return true;
  }

  getToolList(): IToolListEntry[] {
    return this.computeList((arr, pos) => {
      return {
        key: `${pos}${arr[0].conceptKey}`,
        toolName: arr[0].toolName,
        size: arr[0].toolSize,
        sizeUnit: arr[0].toolSizeUnit,
        quantity: arr.reduce((prev, concept) => {
          return prev + concept.totalQuantity;
        }, 0),
      };
    });
  }
}
