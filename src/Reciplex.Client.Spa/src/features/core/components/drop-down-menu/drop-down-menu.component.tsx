import { SuccessButton } from "../buttons/success-button.component";

import dropDownMenuStylesModule from "./drop-down-menu.module.css";
import {
  Menu,
  MenuButton,
  MenuItem,
  MenuItems,
  MenuSeparator,
} from "@headlessui/react";
import { Fragment } from "preact/jsx-runtime";

export interface MenuEntry {
  icon?: string;
  caption: string;
  onClick: () => void;
  type: "entry";
  key: string;
  hidden?: boolean;
}

export interface MenuSeparator {
  type: "separator";
  key: string;
  hidden?: boolean;
}

export type MenuSpecs = (
  | MenuEntry
  | MenuSeparator
  | null
  | undefined
  | false
)[];

/** drop down menu */
export function DropDownMenu({
  menu,
  caption,
  icon,
}: {
  menu: MenuSpecs;
  caption: string;
  icon?: string;
}) {
  if (!menu || menu.length === 0) {
    return null;
  }

  return (
    <Menu>
      <MenuButton as={Fragment}>
        <div className={dropDownMenuStylesModule.actionButtonDiv}>
          <SuccessButton buttonType="dotted">
            {icon && <i className={icon}></i>} {caption}
          </SuccessButton>
        </div>
      </MenuButton>
      <MenuItems
        anchor="bottom"
        className={dropDownMenuStylesModule.dropDownMenuContainer}
      >
        <ul className={dropDownMenuStylesModule.dropDownMenu}>
          {menu.map((entry) => {
            if (!entry || entry.hidden) {
              return null;
            }
            if (entry.type === "separator") {
              return (
                <MenuSeparator
                  key={entry.key}
                  className={dropDownMenuStylesModule.menuSeparator}
                />
              );
            }

            return (
              <MenuItem key={entry.key} as="li" onClick={entry.onClick}>
                {entry.icon && <i className={entry.icon}></i>} {entry.caption}
              </MenuItem>
            );
          })}
        </ul>
      </MenuItems>
    </Menu>
  );
}
