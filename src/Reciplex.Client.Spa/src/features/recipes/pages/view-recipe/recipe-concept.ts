import type {
  RecipeAstNodes,
  RecipeIngredientExpressionAstNode,
  RecipeToolExpressionAstNode,
} from "../../utils/recipe-expressions/md-to-expressions-v2";

// export interface IListEntry {
//   title: string;
//   amount?: number;
//   unit?: string;
//   key: string;
// }

// export class RecipeConceptCollection<
//   TNode extends RecipeIngredientExpressionAstNode | RecipeToolExpressionAstNode,
// > {
//   private _conceptByPosition: Record<number, RecipeConcept<TNode>> = {};
//   private _conceptByKey: Record<string, RecipeConcept<TNode>[]> = {};
//   private _conceptByUserSuppliedId: Record<string, RecipeConcept<TNode>[]> = {};

//   public addAstNode(node: TNode) {
//     const command = node.data.command;
//     let conceptArr;
//     if (typeof command.userSuppliedId === "string") {
//       conceptArr = this._conceptByUserSuppliedId[command.userSuppliedId];
//     } else {
//       const ingredientKey = RecipeConcept.astNodesIngredientCollectionKey(node);
//       conceptArr = this._conceptByKey[ingredientKey];
//     }

//     let concept = conceptArr?.find((c) => c.tryAddAstNode(node));
//     if (!concept) {
//       concept = new RecipeConcept(node);
//       this.addConcept(concept);
//     }
//     this._conceptByPosition[node.data.position] = concept;
//     return;
//   }

//   public getConceptByPosition(
//     position: number,
//   ): RecipeConcept<TNode> | undefined {
//     return this._conceptByPosition[position];
//   }

//   public getList(): IListEntry[] {
//     const ingredientList: IListEntry[] = [];
//     for (const conceptKey in this._conceptByKey) {
//       const localIngredientList: RecipeConcept<TNode>[][] = [];
//       const conceptArr = this._conceptByKey[conceptKey];

//       for (const concept of conceptArr) {
//         const group = localIngredientList.find((arr) =>
//           arr[0].isSameIngredient(concept),
//         );
//         if (group) {
//           group.push(concept);
//         } else {
//           localIngredientList.push([concept]);
//         }
//       }

//       let index = 0;
//       for (const groupArr of localIngredientList) {
//         let quantity: number | undefined;
//         if (typeof groupArr[0].totalQuantity === "number") {
//           quantity = groupArr.reduce(
//             (prev, c) => prev + (c.totalQuantity ?? 0),
//             0,
//           );
//         }

//         ingredientList.push({
//           title: groupArr[0].name,
//           unit: groupArr[0].unitText,
//           amount: quantity,
//           key: `${index++}\n${groupArr[0].ingredientCollectionKey}`,
//         });
//       }
//     }

//     return ingredientList;
//   }

//   private addConcept(concept: RecipeConcept<TNode>) {
//     const key = concept.ingredientCollectionKey;
//     const userId = concept.useSuppliedId;

//     concept.setKeyChangeListener(this.onIngredientKeyChange.bind(this));

//     let conceptArr = this._conceptByKey[key];
//     if (!conceptArr) {
//       conceptArr = [];
//       this._conceptByKey[key] = conceptArr;
//     }
//     conceptArr.push(concept);

//     if (typeof userId === "string") {
//       let userIdArr = this._conceptByUserSuppliedId[userId];
//       if (!userIdArr) {
//         userIdArr = [];
//         this._conceptByUserSuppliedId[userId] = userIdArr;
//       }
//       userIdArr.push(concept);
//     }
//   }

//   private onIngredientKeyChange(
//     src: RecipeConcept<TNode>,
//     before: string,
//     after: string,
//   ) {
//     if (before === after) {
//       return;
//     }

//     const beforeArr = this._conceptByKey[before];
//     if (beforeArr) {
//       const index = beforeArr.indexOf(src);
//       if (index !== -1) {
//         beforeArr.splice(index, 1);
//       }
//     }

//     let afterArr = this._conceptByKey[after];
//     if (!afterArr) {
//       afterArr = [];
//       this._conceptByKey[after] = afterArr;
//     }

//     afterArr.push(src);
//   }
// }

// export class RecipeIngredientConcept implements IConceptCollection<RecipeIngredientExpressionAstNode> {
//   private _onKeyChange?: (oldKey: string, newKey: string) => void;
//   /**
//    * All AST nodes representing this ingredient
//    */
//   private _astNodes: Record<number, RecipeIngredientExpressionAstNode> = {};

//   /**
//    * the user supplied ID. Set only from the constructor.
//    */
//   private _userSuppliedId: string | undefined;

//   /**
//    * the name of the ingredient. Updated
//    * as new nodes are added if `_userSuppliedId` is
//    * set. Otherwise does not change. Last node
//    * with a name wins.
//    */
//   private _name: string = "";

//   /**
//    * The unit of the node. Updated
//    * as new nodes are added if `_userSuppliedId` is
//    * set. Otherwise does not change. Last node
//    * with a name wins. `undefined` means
//    * this ingredient does not have a quantity.
//    */
//   private _unit?: string;

//   /**
//    * the quantity of this ingredient
//    */
//   private _quantity?: number;

//   constructor(node: RecipeIngredientExpressionAstNode) {
//     const command = node.data.command;

//     this._userSuppliedId = command.userSuppliedId;
//     this._astNodes[node.data.position] = node;
//     this._quantity = command.quantity?.amount;
//     this._name =
//       node.data.command.ingredientName ?? node.data.command.inlineText ?? "";
//     this._unit = node.data.command.quantity?.unit;
//   }

//   static factory(
//     node: RecipeIngredientExpressionAstNode,
//   ): RecipeIngredientConcept {
//     return new RecipeIngredientConcept(node);
//   }

//   static getNodeMetaData(
//     node: RecipeIngredientExpressionAstNode,
//   ): INodeMetaData {
//     return {
//       conceptKey: this.makeCollectionKey(
//         node.data.command.ingredientName,
//         node.data.command.quantity?.unit,
//       ),
//       position: node.data.position,
//       userId: node.data.command.userSuppliedId,
//     };
//   }

//   setKeyChangeListener(
//     delegate: (oldKey: string, newKey: string) => void,
//   ): void {
//     this._onKeyChange = delegate;
//   }

//   sameConceptInList(other: RecipeIngredientConcept): boolean {
//     if (this._name !== other._name) {
//       return false;
//     }

//     if (this._unit !== other._unit) {
//       return false;
//     }

//     return true;
//   }

//   tryAddAstNode(node: RecipeIngredientExpressionAstNode): boolean {
//     const command = node.data.command;
//     // if this collection as an ID then new nodes must match with the same id
//     if (command.userSuppliedId !== this._userSuppliedId) {
//       return false;
//     }

//     // in the even an ID is set, then the two nodes
//     // must belong to the same space as
//     // controlled by the `doNotCombine` flags.
//     else if (typeof command.userSuppliedId === "string") {
//       const oldKey = this.conceptKey;

//       // next named node overrides all previous
//       if (!this._name) {
//         this._name = command.ingredientName ?? command.inlineText ?? this._name;
//       } else {
//         this._name = command.ingredientName ?? this._name;
//       }

//       this._astNodes[node.data.position] = node;
//       this._unit = command.quantity?.unit ?? this._unit;
//       this._quantity = command.quantity?.amount ?? this._quantity;

//       const newKey = this.conceptKey;

//       if (oldKey !== newKey) {
//         this._onKeyChange?.(oldKey, newKey);
//       }

//       return true;
//     }

//     // to combine a node they must have the same name
//     if (command.ingredientName !== this._name) {
//       return false;
//     }

//     // The nodes must have the same unit or no units
//     if (command.quantity?.unit !== this._unit) {
//       return false;
//     }

//     this._astNodes[node.data.position] = node;
//     if (command.quantity) {
//       this._quantity = (this._quantity ?? 0) + command.quantity.amount;
//     }

//     return true;
//   }

//   getAstNodeByPosition(position: number): RecipeIngredientExpressionAstNode {
//     return this._astNodes[position];
//   }
//   get userSuppliedId(): string | undefined {
//     return this._userSuppliedId;
//   }
//   get conceptKey(): string {
//     return RecipeIngredientConcept.makeCollectionKey(this._name, this._unit);
//   }

//   private static makeCollectionKey(
//     name: string | undefined,
//     unit: string | undefined,
//   ): string {
//     return `${name?.replaceAll("\n", "\\n") ?? ""}\n${unit?.replaceAll("\n", "\\n") ?? ""}`;
//   }

//   /**
//    * The name of the ingredient
//    */
//   public get name(): string {
//     return this._name;
//   }

//   public get unitText(): string | undefined {
//     if (this._unit === "-") {
//       return "";
//     }

//     return this._unit;
//   }

//   public get totalQuantity(): number | undefined {
//     return this._quantity;
//   }

//   public getAmountByPosition(position: number): number | undefined {
//     if (typeof this._userSuppliedId === "string") {
//       return this.totalQuantity;
//     }

//     return this._astNodes[position]?.data?.command?.quantity?.amount;
//   }

//   /**
//    * Calculates the text to render for a node based on its position
//    * @param position the node position
//    * @returns the text to render
//    */
//   public getRenderText(position: number): string {
//     return this._astNodes[position]?.data?.command?.inlineText ?? this.name;
//   }
// }

// export class RecipeConcept<
//   TNode extends RecipeIngredientExpressionAstNode | RecipeToolExpressionAstNode,
// > {
//   /**
//    * All AST nodes representing this ingredient
//    */
//   private _astNodes: Record<number, TNode> = {};

//   /**
//    * the name of the ingredient. Updated
//    * as new nodes are added if `_userSuppliedId` is
//    * set. Otherwise does not change. Last node
//    * with a name wins.
//    */
//   private _name: string = "";

//   /**
//    * the user supplied ID. Set only from the constructor.
//    */
//   private _userSuppliedId: string | undefined;

//   /**
//    * If this node has the do not combine flag set.
//    * When set, only nodes with the same `_userSuppliedId`
//    * can be joined. If `_userSuppliedId` is undefined
//    * then the node is a singleton.
//    */
//   private _doNotCombine: boolean = false;

//   /**
//    * The unit of the node. Updated
//    * as new nodes are added if `_userSuppliedId` is
//    * set. Otherwise does not change. Last node
//    * with a name wins. `undefined` means
//    * this ingredient does not have a quantity.
//    */
//   private _unit?: string;

//   /**
//    * the total quantity of this ingredient
//    */
//   private _quantity?: number;

//   private _onKeyChange?: (
//     src: RecipeConcept<TNode>,
//     oldKey: string,
//     newKey: string,
//   ) => void;

//   constructor(node: TNode) {
//     const command = node.data.command;

//     this._userSuppliedId = command.userSuppliedId;
//     this._doNotCombine = command.doNotCombine ?? false;
//     this._astNodes[node.data.position] = node;
//     this._quantity = command.quantity?.amount;
//     this._name =
//       node.data.command.ingredientName ?? node.data.command.inlineText ?? "";
//     this._unit = node.data.command.quantity?.unit;
//   }

//   /**
//    * The name of the ingredient
//    */
//   public get name(): string {
//     return this._name;
//   }

//   public get unitText(): string | undefined {
//     if (this._unit === "-") {
//       return "";
//     }

//     return this._unit;
//   }

//   public get totalQuantity(): number | undefined {
//     return this._quantity;
//   }

//   public getAmountByPosition(position: number): number | undefined {
//     if (typeof this._userSuppliedId === "string") {
//       return this.totalQuantity;
//     }

//     return this._astNodes[position]?.data?.command?.quantity?.amount;
//   }

//   /**
//    * Calculates the text to render for a node based on its position
//    * @param position the node position
//    * @returns the text to render
//    */
//   public getRenderText(position: number): string {
//     return this._astNodes[position]?.data?.command?.inlineText ?? this.name;
//   }

//   public setKeyChangeListener(
//     delegate: (
//       src: RecipeConcept<TNode>,
//       oldKey: string,
//       newKey: string,
//     ) => void,
//   ) {
//     this._onKeyChange = delegate;
//   }

//   public clearKeyChangeListener() {
//     this._onKeyChange = undefined;
//   }

//   /**
//    * Computes the key to use in a collection. This
//    * does not uniquely identify the ingredient but is
//    * useful for quickly reducing the name of comparisons that
//    * must be done.
//    */
//   public get ingredientCollectionKey(): string {
//     return RecipeConcept.makeCollectionKey(
//       this._doNotCombine,
//       this._name,
//       this._unit,
//     );
//   }

//   public static astNodesIngredientCollectionKey<
//     TNode extends
//       | RecipeIngredientExpressionAstNode
//       | RecipeToolExpressionAstNode,
//   >(node: TNode): string {
//     const command = node.data.command;
//     return RecipeConcept.makeCollectionKey(
//       command.doNotCombine,
//       command.ingredientName,
//       command.quantity?.unit,
//     );
//   }

//   private static makeCollectionKey(
//     doNotCombine: boolean | undefined,
//     name: string | undefined,
//     unit: string | undefined,
//   ): string {
//     return `${doNotCombine ?? false}\n${name?.replaceAll("\n", "\\n") ?? ""}\n${unit?.replaceAll("\n", "\\n") ?? ""}`;
//   }

//   public get useSuppliedId(): string | undefined {
//     return this._userSuppliedId;
//   }

//   /**
//    * Checks if two ingredient facades represent the same ingredient.
//    * The same ingredient can be split over multiple facades because
//    * of user supplied ids. This method allows interested parties
//    * to roll up these ingredients together.
//    * @param other the other ingredient to check against
//    * @returns true if they are the same. False otherwise.
//    */
//   public isSameIngredient(other: RecipeConcept<TNode>) {
//     if (this._doNotCombine) {
//       return false;
//     }
//     if (other._doNotCombine) {
//       return false;
//     }

//     if (this._name !== other._name) {
//       return false;
//     }

//     if (this._unit !== other._unit) {
//       return false;
//     }

//     return true;
//   }

//   /**
//    * Attempts to add a ingredient ast node to this facade. The
//    * facade will reject the node if the node does not belong to the same set.
//    * @param node ast node to attempt to add
//    */
//   public tryAddAstNode(node: TNode): boolean {
//     const command = node.data.command;
//     const nodeDoNotCombine = command.doNotCombine ?? false;
//     // if this collection as an ID then new nodes must match with the same id
//     if (command.userSuppliedId !== this._userSuppliedId) {
//       return false;
//     }
//     // in the even an ID is set, then the two nodes
//     // must belong to the same space as
//     // controlled by the `doNotCombine` flags.
//     else if (typeof command.userSuppliedId === "string") {
//       if (nodeDoNotCombine === this._doNotCombine) {
//         const oldKey = this.ingredientCollectionKey;

//         // next named node overrides all previous
//         if (!this._name) {
//           this._name =
//             command.ingredientName ?? command.inlineText ?? this._name;
//         } else {
//           this._name = command.ingredientName ?? this._name;
//         }

//         this._astNodes[node.data.position] = node;
//         this._unit = command.quantity?.unit ?? this._unit;
//         this._quantity = command.quantity?.amount ?? this._quantity;

//         const newKey = this.ingredientCollectionKey;

//         if (oldKey !== newKey) {
//           this._onKeyChange?.(this, oldKey, newKey);
//         }

//         return true;
//       }
//       return false;
//     }

//     // node is in the do not combine space. Reject its inclusion.
//     if (this._doNotCombine || command.doNotCombine) {
//       return false;
//     }

//     // to combine a node they must have the same name
//     if (command.ingredientName !== this._name) {
//       return false;
//     }

//     // The nodes must have the same unit or no units
//     if (command.quantity?.unit !== this._unit) {
//       return false;
//     }

//     this._astNodes[node.data.position] = node;
//     if (command.quantity) {
//       this._quantity = (this._quantity ?? 0) + command.quantity.amount;
//     }

//     return true;
//   }
// }

// export abstract class AbstractRecipeConcept<
//   TNode extends RecipeAstNodes,
//   TSelf extends AbstractRecipeConcept<TNode, TSelf>,
// > {
//   /**
//    * All AST nodes representing this concept
//    */
//   protected _astNodes: Record<number, TNode> = {};

//   /**
//    * the name of the concept. Updated
//    * as new nodes are added if `_userSuppliedId` is
//    * set. Otherwise does not change. Last node
//    * with a name wins.
//    */
//   protected _name: string = "";

//   /**
//    * the user supplied ID. Set only from the constructor.
//    */
//   protected _userSuppliedId: string | undefined;

//   /**
//    * If this node has the do not combine flag set.
//    * When set, only nodes with the same `_userSuppliedId`
//    * can be joined. If `_userSuppliedId` is undefined
//    * then the node is a singleton.
//    */
//   protected _doNotCombine: boolean = false;

//   protected _onKeyChange?: (src: TSelf, oldKey: string, newKey: string) => void;

//   constructor(node: TNode) {
//     const command = node.data.command;

//     this._userSuppliedId = command.userSuppliedId;
//     this._doNotCombine = command.doNotCombine ?? false;
//     this._astNodes[node.data.position] = node;
//     this._name =
//       this.onGetNameFromNode(node) ?? this.onGetInlineTextFromNode(node) ?? "";
//   }

//   protected abstract onGetNameFromNode(node: TNode): string | undefined;
//   protected abstract onGetInlineTextFromNode(node: TNode): string | undefined;

//   /**
//    * The name of the concept
//    */
//   public get name(): string {
//     return this._name;
//   }

//   /**
//    * Calculates the text to render for a node based on its position
//    * @param position the node position
//    * @returns the text to render
//    */
//   public getRenderText(position: number): string {
//     const node = this._astNodes[position];
//     if (!node) {
//       return this._name;
//     }

//     return this.onGetInlineTextFromNode(node) ?? this.name;
//   }

//   public setKeyChangeListener(
//     delegate: (src: TSelf, oldKey: string, newKey: string) => void,
//   ) {
//     this._onKeyChange = delegate;
//   }

//   public clearKeyChangeListener() {
//     this._onKeyChange = undefined;
//   }

//   /**
//    * Computes the key to use in a collection. This
//    * does not uniquely identify the concept but is
//    * useful for quickly reducing the number of comparisons that
//    * must be done.
//    */
//   public abstract get collectionKey(): string;

//   public get useSuppliedId(): string | undefined {
//     return this._userSuppliedId;
//   }

//   /**
//    * Checks if two ingredient facades represent the same ingredient.
//    * The same ingredient can be split over multiple facades because
//    * of user supplied ids. This method allows interested parties
//    * to roll up these ingredients together.
//    * @param other the other ingredient to check against
//    * @returns true if they are the same. False otherwise.
//    */
//   public isSameConcept(other: TSelf) {
//     if (this._doNotCombine) {
//       return false;
//     }
//     if (other._doNotCombine) {
//       return false;
//     }

//     if (this._name !== other._name) {
//       return false;
//     }

//     return this.onIsSameConcept(other);
//   }

//   protected abstract onIsSameConcept(other: TSelf): boolean;
//   protected abstract onAddNode(node: TNode): void;

//   /**
//    * Attempts to add a ingredient ast node to this facade. The
//    * facade will reject the node if the node does not belong to the same set.
//    * @param node ast node to attempt to add
//    */
//   public tryAddAstNode(node: TNode): boolean {
//     const command = node.data.command;
//     const nodeDoNotCombine = command.doNotCombine ?? false;
//     // if this collection as an ID then new nodes must match with the same id
//     if (command.userSuppliedId !== this._userSuppliedId) {
//       return false;
//     }
//     // in the even an ID is set, then the two nodes
//     // must belong to the same space as
//     // controlled by the `doNotCombine` flags.
//     else if (typeof command.userSuppliedId === "string") {
//       if (nodeDoNotCombine === this._doNotCombine) {
//         const oldKey = this.collectionKey;

//         // next named node overrides all previous
//         if (!this._name) {
//           this._name =
//             this.onGetNameFromNode(node) ??
//             this.onGetInlineTextFromNode(node) ??
//             this._name;
//         } else {
//           this._name = this.onGetNameFromNode(node) ?? this._name;
//         }
//         this._astNodes[node.data.position] = node;
//         this.onAddNode(node);

//         const newKey = this.collectionKey;
//         if (oldKey !== newKey) {
//           // todo - figure out how to make TS realize that this === TSelf
//           this._onKeyChange?.(this as unknown as TSelf, oldKey, newKey);
//         }

//         return true;
//       }
//       return false;
//     }

//     // node is in the do not combine space. Reject its inclusion.
//     if (this._doNotCombine || command.doNotCombine) {
//       return false;
//     }

//     // to combine a node they must have the same name
//     if (this.onGetNameFromNode(node) !== this._name) {
//       return false;
//     }

//     if (!this.onDoesNodeBelongsToConcept(node)) {
//       return false;
//     }

//     const oldKey = this.collectionKey;
//     this._astNodes[node.data.position] = node;
//     this.onAddNode(node);
//     const newKey = this.collectionKey;

//     if (oldKey !== newKey) {
//       // todo - figure out how to make TS realize that this === TSelf
//       this._onKeyChange?.(this as unknown as TSelf, oldKey, newKey);
//     }
//     return true;
//   }

//   protected abstract onDoesNodeBelongsToConcept(node: TNode): boolean;
// }

// export class RecipeIngredientConcept extends AbstractRecipeConcept<
//   RecipeIngredientExpressionAstNode,
//   RecipeIngredientConcept
// > {
//   /**
//    * The unit of the node. Updated
//    * as new nodes are added if `_userSuppliedId` is
//    * set. Otherwise does not change. Last node
//    * with a name wins. `undefined` means
//    * this ingredient does not have a quantity.
//    */
//   private _unit?: string;

//   /**
//    * the total quantity of this ingredient
//    */
//   private _quantity?: number;

//   constructor(node: RecipeIngredientExpressionAstNode) {
//     super(node);
//     const command = node.data.command;
//     this._quantity = command.quantity?.amount;
//     this._unit = node.data.command.quantity?.unit;
//   }

//   protected onGetNameFromNode(
//     node: RecipeIngredientExpressionAstNode,
//   ): string | undefined {
//     return node.data.command.ingredientName;
//   }
//   protected onGetInlineTextFromNode(
//     node: RecipeIngredientExpressionAstNode,
//   ): string | undefined {
//     return node.data.command.inlineText;
//   }
//   protected onIsSameConcept(other: RecipeIngredientConcept): boolean {
//     if (this._unit !== other._unit) {
//       return false;
//     }
//     return true;
//   }

//   protected onAddNode(node: RecipeIngredientExpressionAstNode): void {
//     if (typeof this._userSuppliedId === "string") {
//       this._unit = node.data.command.quantity?.unit ?? this._unit;
//       this._quantity = node.data.command.quantity?.amount ?? this._quantity;
//     } else {
//       if (node.data.command.quantity) {
//         this._quantity =
//           (this._quantity ?? 0) + node.data.command.quantity.amount;
//       }
//     }
//   }

//   protected onDoesNodeBelongsToConcept(
//     node: RecipeIngredientExpressionAstNode,
//   ) {
//     // The nodes must have the same unit or no units
//     if (node.data.command.quantity?.unit !== this._unit) {
//       return false;
//     }
//     return true;
//   }

//   public get unitText(): string | undefined {
//     if (this._unit === "-") {
//       return "";
//     }

//     return this._unit;
//   }

//   public get totalQuantity(): number | undefined {
//     return this._quantity;
//   }

//   public getAmountByPosition(position: number): number | undefined {
//     if (typeof this.useSuppliedId === "string") {
//       return this.totalQuantity;
//     }

//     return this._astNodes[position]?.data?.command?.quantity?.amount;
//   }

//   /**
//    * Computes the key to use in a collection. This
//    * does not uniquely identify the ingredient but is
//    * useful for quickly reducing the name of comparisons that
//    * must be done.
//    */
//   public get collectionKey(): string {
//     return RecipeIngredientConcept.makeCollectionKey(
//       this._doNotCombine,
//       this._name,
//       this._unit,
//     );
//   }

//   public static astNodesIngredientCollectionKey(
//     node: RecipeIngredientExpressionAstNode,
//   ): string {
//     const command = node.data.command;
//     return RecipeIngredientConcept.makeCollectionKey(
//       command.doNotCombine,
//       command.ingredientName,
//       command.quantity?.unit,
//     );
//   }

//   private static makeCollectionKey(
//     doNotCombine: boolean | undefined,
//     name: string | undefined,
//     unit: string | undefined,
//   ): string {
//     return `${doNotCombine ?? false}\n${name?.replaceAll("\n", "\\n") ?? ""}\n${unit?.replaceAll("\n", "\\n") ?? ""}`;
//   }
// }
