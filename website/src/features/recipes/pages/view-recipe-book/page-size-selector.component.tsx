import {
  Field,
  Listbox,
  ListboxButton,
  ListboxOptions,
  ListboxOption,
} from "@headlessui/react";
import { useRecipeListTableContext } from "./useRecipeListTableContext.hook";

import styles from "./page-size-selector.module.css";
import buttonCommonStyle from "../../../core/button-common/button-common.module.css";
import dangerButtonStyle from "../../../core/components/danger-button/danger-button.module.css";

/** selector control for choosing a page size */
export function PageSizeSelector() {
  const selectedSize = useRecipeListTableContext((state) => state.size);
  const updateSize = useRecipeListTableContext((state) => state.updateSize);
  const selectedValue = pageSize.find((entry) => entry.id === selectedSize);

  return (
    <Field className={styles.pageSizeFormWrapper}>
      <Listbox value={selectedValue} onChange={(value) => updateSize(value.id)}>
        <ListboxButton
          className={`${buttonCommonStyle.commonButton} ${dangerButtonStyle.danger} ${styles.dropDownButton}`}
          data-button-type="hidden"
        >
          <div className={styles.textWrapper}>
            <div className={styles.visibleValue}>
              {selectedValue?.name ?? selectedValue}
            </div>
            {pageSize.map((sz) => (
              <div
                key={sz.id}
                role="structure"
                aria-hidden
                className={styles.hiddenValue}
              >
                {sz.name}
              </div>
            ))}
          </div>
        </ListboxButton>
        <ListboxOptions
          className={styles.dropDownOptionContainer}
          anchor="bottom"
        >
          {pageSize.map((sz) => (
            <ListboxOption
              className={styles.dropDownOption}
              key={sz.id}
              value={sz}
            >
              {sz.name}
            </ListboxOption>
          ))}
        </ListboxOptions>
      </Listbox>
    </Field>
  );
}

/** format of a page size entry */
type PageSizeEntry = {
  /** the entries unique id */
  id: number;
  /** the entries name */
  name: string;
};

/** predefined app page sizes */
const pageSize: PageSizeEntry[] = [
  { id: 10, name: "10" },
  { id: 20, name: "20" },
  { id: 50, name: "50" },
  { id: 100, name: "100" },
];
