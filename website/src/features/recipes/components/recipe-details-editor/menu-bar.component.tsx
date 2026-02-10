import styles from "./menu-bar.module.css";
import { MenuButton } from "./menu-button.component";
import type { MenuBarCommands } from "./useMenuBarCommandHandler.hook";

export function MenuBar({
  disabled,
  commandHandler,
  onSizeToggle,
  isFullscreen,
}: {
  disabled?: boolean;
  commandHandler: (command: MenuBarCommands) => void;
  isFullscreen: boolean;
  onSizeToggle: () => void;
}) {
  const buttonFactory = (
    iconName: string,
    command: MenuBarCommands,
    tooltip: string,
  ) => {
    return (
      <MenuButton
        iconName={`bi bi-${iconName}`}
        command={command}
        disabled={disabled}
        onClick={commandHandler}
        tooltip={tooltip}
      />
    );
  };
  return (
    <ul className={`${styles.menuBar}`}>
      {buttonFactory("type-bold", "bold", "bold")}
      {buttonFactory("type-italic", "italic", "italic")}
      {buttonFactory("type-h1", "h1", "header 1")}
      {buttonFactory("type-h2", "h2", "header 2")}
      {buttonFactory("type-h3", "h3", "header 3")}
      {buttonFactory("type-h4", "h4", "header 4")}
      {buttonFactory("type-h5", "h5", "header 5")}
      {buttonFactory("type-h6", "h6", "header 6")}
      {buttonFactory("quote", "inline-quote", "inline quote")}
      {buttonFactory("blockquote-left", "block-quote", "block quote")}
      {buttonFactory("list-ul", "bullet-list", "numbered list")}
      {buttonFactory("list-ol", "number-list", "bulleted list")}
      {buttonFactory("link-45deg", "hyperlink", "insert hyper link")}
      {buttonFactory("hr", "hr", "insert horizontal divider line")}
      {buttonFactory(
        "cart4",
        "ingredient-reference",
        "add ingredient reference",
      )}
      {buttonFactory(
        "puzzle-fill",
        "recipe-section-reference",
        "add recipe section reference",
      )}
      {buttonFactory("fork-knife", "recipe-link", "add recipe link")}
      {buttonFactory(
        "tools",
        "recipe-tool-reference",
        "add recipe tool reference",
      )}
      <li className={styles.expandSpacer}></li>
      <MenuButton<"notused">
        iconName={
          isFullscreen ? "bi bi-fullscreen-exit" : "bi bi-arrows-fullscreen"
        }
        command={"notused"}
        onClick={onSizeToggle}
        tooltip={isFullscreen ? "exit fullscreen" : "enter fullscreen"}
      />
    </ul>
  );
}
