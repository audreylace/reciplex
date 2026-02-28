import { create } from "zustand";

/** local storage key */
const storageKey = "feature:recipe; RecipeListTableContext";

/**
 * Component context for the recipe list table control
 */
export const useRecipeListTableContext = create<{
  size: number;
  updateSize: (newSize: number) => void;
}>((set) => ({
  size: initialSize(),
  updateSize: (newSize: number) => set({ size: updateSize(newSize) }),
}));

/** computes initial size for the application */
function initialSize() {
  const storedSize = localStorage.getItem(storageKey);
  const size = storedSize ? Number.parseInt(storedSize) : null;
  if (!size || isNaN(size)) {
    return 10;
  }

  return size;
}

/**
 * Stores the new size in local storage for persistence and then returns it
 * @param newSize the new page size
 * @returns the new size
 */
function updateSize(newSize: number) {
  localStorage.setItem(storageKey, `${newSize}`);
  return newSize;
}
