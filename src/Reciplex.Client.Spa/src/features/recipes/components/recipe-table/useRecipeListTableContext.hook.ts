import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

/**
 * Component context for the recipe list table control
 */
export const useRecipeListTableContext = create<IUseRecipeListTableState>()(
  persist(
    (set) => {
      return {
        size: 10,
        updateSize: (newSize: number) => set({ size: newSize }),
      };
    },
    {
      name: "recipe-list-page-size", // name of the item in localStorage
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

/** state shape of `useRecipeListTableContext` */
export interface IUseRecipeListTableState {
  /** the selected size */
  size: number;
  /** invoked to update the size */
  updateSize: (newSize: number) => void;
}
