import type { RecipeAstNodes } from "../../utils/recipe-expressions/md-to-expressions-v2";

/**
 * Type of delegate invoked by `IConcept` to notify owning parent that
 * its concept key has changed.
 * @param oldKey the previous key before the change
 * @param newKey the new key after the change
 */
export type KeyChangeListenerDelegate = (
  oldKey: string,
  newKey: string,
) => void;

/**
 * A collection type that handles recipe ast nodes
 */
export interface IHandleRecipeAstNode {
  /**
   * invoked by a runtime to attempt to add a recipe ast node to this type
   * @param node the node trying to be added
   * @returns true if this type handles this ast node and the walker should stop parsing.
   */
  tryAddNode(node: RecipeAstNodes): boolean;
}

/**
 * Concept inside of a `ConceptCollection`
 * @type TNode the type of the ast node stored in the collection
 */
export interface IConcept<TNode, TSelf> {
  /**
   * sets the callback that is invoked when the concepts `conceptKey` changes
   * @param delegate the callback
   */
  setKeyChangeListener(delegate: KeyChangeListenerDelegate): void;
  /**
   * invoked to determine if this instance and `other` represent
   * the same concept.
   * @param other concept to compare against
   * @returns true if `other` and this represent the same concept
   * @remark the same concept can be split over multiple `IConcept`
   * because of various reasons like user supplied ids. This API
   * is used to group all related concepts into
   * generated lists.
   */
  isSameConceptInList(other: TSelf): boolean;
  /**
   * Attempts to add `node` to this collection
   * @param node the ast node to add to this collection
   * @returns `true` if the node was added to the collection
   */
  tryAddAstNode(node: TNode): boolean;
  /**
   * Attempts to get an ast node by its parse position
   * @param position the position of the node
   * @returns the node at `position` or undefined if a node is not stored
   * at that position in this concept.
   */
  getAstNodeByPosition(position: number): TNode | undefined;
  /**
   * the user supplied id of this concept. undefined if
   * no id is found.s
   */
  get userSuppliedId(): string | undefined;
  /**
   * the concepts key for fast lookup. Multiple
   * instances of this collection can have the same key.
   * Any indexing structure should store multiple concepts
   * under a single key via an array.
   */
  get conceptKey(): string;
  /**
   * the nodes stored in this collection
   */
  get astNodes(): TNode[];
}

/**
 * information derived from a node
 * in a generalized format
 */
export interface INodeMetaData {
  /**
   * the user assigned id to the node
   */
  userId?: string;
  /**
   * the nodes parse position for corelation
   * when rendering with hast and other syntax trees
   */
  position: number;
  /**
   * the concept key of the node.
   */
  conceptKey: string;
}

/**
 * Manages a set of concepts of type `TConcept` for `TNode`.
 * @type TNode the ast nodes managed by the concepts of this collection
 * @type TConcept the concepts managed by this collection
 */
export class ConceptCollection<
  TNode,
  TConcept extends IConcept<TNode, TConcept>,
> {
  /**
   * class constructor
   * @param getNodeMetaData delegate invoked to map node to node meta data
   * @param factory factory for making a new concept using a node
   */
  constructor(
    getNodeMetaData: (node: TNode) => INodeMetaData,
    factory: (node: TNode) => TConcept,
  ) {
    this._factory = factory;
    this._getNodeMetaData = getNodeMetaData;
  }

  /**
   * Mapping of node position to owning concept
   */
  private _conceptByPosition: Record<number, TConcept> = {};
  /**
   * Mapping of concept key to concept list
   */
  private _conceptByKey: Record<string, TConcept[]> = {};
  /**
   * Concepts by user id
   */
  private _conceptByUserSuppliedId: Record<string, TConcept[]> = {};
  /**
   * delegate invoked to map node to node meta data
   */
  private _getNodeMetaData: (node: TNode) => INodeMetaData;
  /**
   * factory for making a new concept using a node
   */
  private _factory: (node: TNode) => TConcept;

  /**
   * adds an ast node to this concept collection
   * @param node the ast node that will be added to this collection
   */
  public addAstNode(node: TNode): void {
    let conceptArr;
    const { userId, conceptKey, position } = this._getNodeMetaData(node);
    if (typeof userId === "string") {
      conceptArr = this._conceptByUserSuppliedId[userId];
    } else {
      conceptArr = this._conceptByKey[conceptKey];
    }

    let concept = conceptArr?.find((c) => c.tryAddAstNode(node));
    if (!concept) {
      concept = this._factory(node);
      this.addConcept(concept);
    }
    this._conceptByPosition[position] = concept;
  }

  /**
   * gets the concept holding the node identified by `position`
   * @param position the position
   * @returns the concept holding the node at `position` or undefined
   */
  public getConceptByPosition(position: number): TConcept | undefined {
    return this._conceptByPosition[position];
  }

  /**
   * computes a list from groups of concepts
   * @param delegate the delegate to build list entries. Handled
   * arrays holding sets of concepts representing the same thing
   * in a list. The delegate should return the list entry representing
   * that concept set.
   * @returns list of `TListEntry`
   * @type TListEntry the type of each list array entry
   */
  public computeList<TListEntry>(
    delegate: (conceptArr: TConcept[], index: number) => TListEntry,
  ): TListEntry[] {
    const listEntries: TListEntry[] = [];
    for (const conceptKey in this._conceptByKey) {
      const localIngredientList: TConcept[][] = [];
      const conceptArr = this._conceptByKey[conceptKey];

      for (const concept of conceptArr) {
        const group = localIngredientList.find((arr) =>
          arr[0].isSameConceptInList(concept),
        );
        if (group) {
          group.push(concept);
        } else {
          localIngredientList.push([concept]);
        }
      }

      let index = 0;
      for (const groupArr of localIngredientList) {
        listEntries.push(delegate(groupArr, index++));
      }
    }

    return listEntries;
  }

  private addConcept(concept: TConcept) {
    const key = concept.conceptKey;
    const userId = concept.userSuppliedId;

    concept.setKeyChangeListener((before, after) =>
      this.onConceptKeyChange(concept, before, after),
    );

    let conceptArr = this._conceptByKey[key];
    if (!conceptArr) {
      conceptArr = [];
      this._conceptByKey[key] = conceptArr;
    }
    conceptArr.push(concept);

    if (typeof userId === "string") {
      let userIdArr = this._conceptByUserSuppliedId[userId];
      if (!userIdArr) {
        userIdArr = [];
        this._conceptByUserSuppliedId[userId] = userIdArr;
      }
      userIdArr.push(concept);
    }
  }

  private onConceptKeyChange(src: TConcept, before: string, after: string) {
    if (before === after) {
      return;
    }

    const beforeArr = this._conceptByKey[before];
    if (beforeArr) {
      const index = beforeArr.indexOf(src);
      if (index !== -1) {
        beforeArr.splice(index, 1);
      }
    }

    let afterArr = this._conceptByKey[after];
    if (!afterArr) {
      afterArr = [];
      this._conceptByKey[after] = afterArr;
    }

    afterArr.push(src);
  }
}
