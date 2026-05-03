import { MenuButton } from "./menu-button.component";
import type { MenuBarCommands } from "./useRecipeDetailsMenuBarCommandHandler.hook";
import FormatBoldIcon from "@mui/icons-material/FormatBold";
import FormatItalicIcon from "@mui/icons-material/FormatItalic";
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";
import FormatListBulletedIcon from "@mui/icons-material/FormatListBulleted";
import FormatListNumberedIcon from "@mui/icons-material/FormatListNumbered";
import LinkIcon from "@mui/icons-material/Link";
import RestaurantIcon from "@mui/icons-material/Restaurant";
import EggIcon from "@mui/icons-material/Egg";
import TitleIcon from "@mui/icons-material/Title";
import FullscreenIcon from "@mui/icons-material/Fullscreen";
import FullscreenExitIcon from "@mui/icons-material/FullscreenExit";
import ButtonGroup from "@mui/material/ButtonGroup";
import IconButton from "@mui/material/IconButton";

export function RecipeDetailsMenuBar({
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
  return (
    <>
      <ButtonGroup>
        <MenuButton
          command="bold"
          icon={<FormatBoldIcon />}
          tooltip="bold"
          disabled={disabled}
          onClick={commandHandler}
        />
        <MenuButton
          command="italic"
          icon={<FormatItalicIcon />}
          tooltip="italic"
          disabled={disabled}
          onClick={commandHandler}
        />
        <MenuButton
          command="h1"
          icon={<TitleIcon />}
          tooltip="header 1"
          disabled={disabled}
          onClick={commandHandler}
        />
        <MenuButton
          command="inline-quote"
          icon={<FormatQuoteIcon />}
          tooltip="inline quote"
          disabled={disabled}
          onClick={commandHandler}
        />
        <MenuButton
          command="bullet-list"
          icon={<FormatListBulletedIcon />}
          tooltip="bulleted list"
          disabled={disabled}
          onClick={commandHandler}
        />
        <MenuButton
          command="number-list"
          icon={<FormatListNumberedIcon />}
          tooltip="number list"
          disabled={disabled}
          onClick={commandHandler}
        />
        <MenuButton
          command="hyperlink"
          icon={<LinkIcon />}
          tooltip="hyperlink"
          disabled={disabled}
          onClick={commandHandler}
        />
        <MenuButton
          command="recipe-tool-reference"
          icon={<RestaurantIcon />}
          tooltip="add tool"
          disabled={disabled}
          onClick={commandHandler}
        />
        <MenuButton
          command="ingredient-reference"
          icon={<EggIcon />}
          tooltip="add ingredient"
          disabled={disabled}
          onClick={commandHandler}
        />
        <IconButton
          title={isFullscreen ? "exit fullscreen" : "enter fullscreen"}
          role="menuitem"
          disabled={disabled}
          onClick={() => {
            onSizeToggle();
          }}
        >
          {isFullscreen ? <FullscreenIcon /> : <FullscreenExitIcon />}
        </IconButton>
      </ButtonGroup>
    </>
  );
}
