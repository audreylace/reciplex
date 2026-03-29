import { create } from "zustand";
import {
  OptionSelector,
  type IOptionEntry,
} from "../../../core/components/option-selector/option-selector.component";

/** local storage key */
const storageKey = "feature:recipe; BookPageSize";

/**
 * Component context for the recipe list table control
 */
const useBookPageSize = create<{
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

/** predefined app page sizes */
const pageSize: IOptionEntryWithSize[] = [
  { size: 10, name: "10", key: "10" },
  { size: 20, name: "20", key: "20" },
  { size: 50, name: "50", key: "50" },
  { size: 100, name: "100", key: "100" },
];

interface IOptionEntryWithSize extends IOptionEntry {
  size: number;
}

export function BookPageSizeSelector() {
  const selectedSize = useBookPageSize((state) => state.size);
  const updateSize = useBookPageSize((state) => state.updateSize);
  const selectedValue = pageSize.find((entry) => entry.size === selectedSize);

  return (
    <OptionSelector
      options={pageSize}
      value={selectedValue ?? pageSize[0]}
      onChange={(entry) => updateSize(entry.size)}
    />
  );
}

export function useBookSize() {
  return useBookPageSize((state) => state.size);
}
