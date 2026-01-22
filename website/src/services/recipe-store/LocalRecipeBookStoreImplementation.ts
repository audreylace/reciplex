import type {
  IRecipeBookStore,
  IUserModel,
  IRecipeBookModel,
  IGetRecipeBooksResult,
  IGetSessionInformationResult,
  IGetRecipeBooksArgs,
  IPageCursor,
  ICreateRecipeBookArgs,
  ICreateRecipeArgs,
  IRecipeModel,
  IUpdateRecipeArgs,
} from ".";

/**
 * Implements `IRecipeBookStore` using a browser only model that stores mock data. Useful for testing and demoing.
 */
export class LocalRecipeBookStoreImplementation implements IRecipeBookStore {
  private whoAmI = "gh67y";
  private _users: Record<string, IUserModel> = {
    gh67y: {
      displayName: "Ashy Poodle",
      id: "gh67y",
    },
    hh7878fdsf90: {
      displayName: "Punderson Poodle",
      id: "hh7878fdsf90",
    },
    "741890f914": {
      displayName: "Brittany",
      id: "741890f914",
    },
    fsdfsaf: {
      displayName: "Audrey",
      id: "fsdfsaf",
    },
  };

  private _books: Record<
    string,
    { book: IRecipeBookModel; recipes: Record<string, IRecipeModel> }
  > = {};
  /** Simulated network latency */
  private latency = 300;
  private _idCounter = 0;

  constructor() {
    for (let i = 0; i < 20; i++) {
      const key = `key_${String(i).padStart(10, "0")}`;
      this._books[key] = {
        book: {
          name: "my recipe book " + i,
          id: key,
          shortDescription: "fake recipe book that is owned by logged in user",
          ownerId: this.whoAmI,
          hasWriteAccess: true,
        },
        recipes: {},
      };
    }
    const userKeys = Object.keys(this._users);
    for (let i = 0; i < 20; i++) {
      const nextOwner = userKeys[i & userKeys.length];
      const key = `key_${String(i).padStart(10, "0")}_`;
      this._books[key] = {
        book: {
          name: "zz " + i,
          id: key,
          shortDescription:
            "fake recipe book that is may be owned by logged in user but maybe not",
          ownerId: userKeys[i % userKeys.length],
          hasWriteAccess: nextOwner === this.whoAmI || i % 3 === 0,
        },
        recipes: {},
      };
    }
  }

  createRecipeBook(args: ICreateRecipeBookArgs): Promise<IRecipeBookModel> {
    return new Promise((resolve) => {
      setTimeout(() => {
        const nextId = this._idCounter++;
        const key = `z__custom${String(nextId).padStart(10, "0")}`;
        this._books[key] = {
          book: {
            id: key,
            ownerId: this.whoAmI,
            name: args.name,
            shortDescription: args.shortDescription ?? "",
            hasWriteAccess: true,
          },
          recipes: {},
        };
        resolve(this._books[key].book);
      }, this.latency);
    });
  }

  /** @inheritdoc */
  public async getUserById(id: string): Promise<IUserModel | null> {
    return new Promise((resolve) => {
      setTimeout(() => {
        const user = this._users[id];
        resolve(user ?? null);
      }, this.latency);
    });
  }
  /** @inheritdoc */
  public getRecipeBook(id: string): Promise<IRecipeBookModel | null> {
    return new Promise((resolve) => {
      setTimeout(() => {
        const book = this._books[id];
        resolve(book?.book ?? null);
      }, this.latency);
    });
  }

  public getSessionInformation(): Promise<IGetSessionInformationResult> {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        const user = this._users[this.whoAmI];
        if (!user) {
          reject();
          return;
        }
        resolve({
          isAuthenticated: true,
          userData: user,
        });
      }, this.latency);
    });
  }
  getRecipeBooks(
    args?: IGetRecipeBooksArgs,
  ): Promise<IGetRecipeBooksResult | null> {
    return new Promise((resolve) => {
      setTimeout(() => {
        const keys = Object.keys(this._books).sort();
        const pageSize = Math.max(2, Math.min(10, args?.limit ?? 10));

        let startIndex = 0;
        if (args?.cursor) {
          startIndex = keys.indexOf(args.cursor.position);
          if (startIndex == -1) {
            startIndex = 0;
          } else if (args.cursor.type == "next") {
            startIndex = Math.min(keys.length, startIndex + 1);
          } else {
            startIndex = Math.max(0, startIndex - pageSize);
          }
        }

        const keySlice = keys.slice(startIndex, pageSize + startIndex);
        const seenUsers: Record<string, IUserModel> = {};
        const recipeMap: Record<string, IRecipeBookModel> = {};
        const page = keySlice.reduce((prev, key) => {
          const book = this._books[key];
          if (book) {
            prev.push(key);
            recipeMap[key] = book.book;
            if (book.book.ownerId !== this.whoAmI && args?.fetchUserData) {
              const user = this._users[book.book.ownerId];
              if (user) {
                seenUsers[user.id] = user;
              }
            }
          }
          return prev;
        }, [] as string[]);

        let previousCursor: IPageCursor | undefined = undefined;
        let nextCursor: IPageCursor | undefined = undefined;
        if (page.length > 1) {
          const lastKey = page[page.length - 1];
          const lastKeyIndex = keys.indexOf(lastKey);
          if (lastKeyIndex < keys.length - 1 && lastKeyIndex > 0) {
            nextCursor = {
              position: lastKey,
              type: "next",
            };
          }
          if (startIndex > 0) {
            const firstKey = page[0];
            const firstKeyIndex = keys.indexOf(firstKey);
            if (firstKeyIndex < keys.length - 1 && firstKeyIndex > 0) {
              previousCursor = {
                position: firstKey,
                type: "previous",
              };
            }
          }
        }

        resolve({
          recipes: recipeMap,
          page,
          previousCursor,
          nextCursor,
          users: args?.fetchUserData ? seenUsers : undefined,
        });
      }, this.latency);
    });
  }
  getRecipeById(id: string): Promise<IRecipeModel | null> {
    return new Promise((resolve) => {
      setTimeout(() => {
        for (var bookKey in this._books) {
          const recipe = this._books[bookKey].recipes[id];
          if (!recipe) {
            continue;
          }

          resolve(recipe);
          return;
        }

        resolve(null);
      }, this.latency);
    });
  }
  createRecipe(args: ICreateRecipeArgs): Promise<IRecipeModel> {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        const book = this._books[args.bookId];
        if (!book) {
          reject();
        }

        const key = `recipe_key_${this._idCounter++}`;
        book.recipes[key] = {
          id: key,
          bookId: book.book.id,
          name: args.name,
          shortDescription: args.shortDescription ?? "",
          hasWriteAccess: book.book.hasWriteAccess,
          details: "",
          versionTag: `__version_${this._idCounter++}`,
        };

        resolve(book.recipes[key]);
      }, this.latency);
    });
  }

  updateRecipe(
    recipeId: string,
    args: IUpdateRecipeArgs,
  ): Promise<IRecipeModel> {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        for (var bookKey in this._books) {
          const recipe = this._books[bookKey].recipes[recipeId];
          if (!recipe) {
            continue;
          }

          if (!recipe.hasWriteAccess || recipe.versionTag !== args.versionTag) {
            reject();
            return;
          }

          this._books[bookKey].recipes[recipeId] = {
            id: recipeId,
            bookId: recipe.bookId,
            name: args.name,
            shortDescription: args.shortDescription ?? "",
            hasWriteAccess: true,
            details: args.details,
            versionTag: `__version_${this._idCounter++}`,
          };

          resolve(this._books[bookKey].recipes[recipeId]);
          return;
        }

        reject();
      }, this.latency);
    });
  }

  deleteRecipe(recipeId: string): Promise<void> {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        for (var bookKey in this._books) {
          const recipe = this._books[bookKey].recipes[recipeId];
          if (!recipe) {
            continue;
          }

          if (!recipe.hasWriteAccess) {
            reject();
            return;
          }

          delete this._books[bookKey].recipes[recipeId];

          resolve();
          return;
        }

        reject();
      }, this.latency);
    });
  }
}
