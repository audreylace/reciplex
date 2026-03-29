import {
  Listbox,
  ListboxButton,
  ListboxOptions,
  ListboxOption,
} from "@headlessui/react";

import componentStyles from "./option-selector.module.css";
import { SuccessButton } from "../buttons/success-button.component";
import { Fragment } from "preact/jsx-runtime";

export function OptionSelector<T extends IOptionEntry>({
  options,
  value,
  onChange,
}: IOptionSelectorProps<T>) {
  return (
    <Listbox value={value} onChange={(newValue) => onChange(newValue)}>
      <ListboxButton as={Fragment}>
        {/** must wrap success button inside fragment..div. Otherwise the option selector crashes. */}
        <div>
          <SuccessButton buttonType="dotted">
            <div className={componentStyles.textWrapper}>
              <div className={componentStyles.visibleValue}>{value.name}</div>
              {options.map((sz) => (
                <div
                  key={sz.key}
                  role="structure"
                  aria-hidden
                  tabIndex={-1}
                  className={componentStyles.hiddenValue}
                >
                  {sz.name}
                </div>
              ))}
            </div>
          </SuccessButton>
        </div>
      </ListboxButton>
      <ListboxOptions
        className={componentStyles.dropDownOptionContainer}
        anchor="bottom"
      >
        {options.map((sz) => (
          <ListboxOption
            className={componentStyles.dropDownOption}
            key={sz.key}
            value={sz}
          >
            {sz.name}
          </ListboxOption>
        ))}
      </ListboxOptions>
    </Listbox>
  );
}

export interface IOptionEntry {
  /** the entries unique id */
  key: string;
  /** the entries name */
  name: string;
}

export interface IOptionSelectorProps<T extends IOptionEntry> {
  value: T;
  options: T[];
  onChange: (value: T) => void;
}
