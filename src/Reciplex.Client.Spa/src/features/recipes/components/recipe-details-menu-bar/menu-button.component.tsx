import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";

/**
 * Menu button component inside the markdown recipe details editor menu bar
 */
export function MenuButton<TCommandType>({
  icon,
  onClick,
  command,
  disabled,
  tooltip,
}: {
  /**
   * invoked on click
   * @param command the command
   */
  onClick: (command: TCommandType) => void;
  /**
   * icon shown to the end user
   */
  icon: React.ReactNode;
  /**
   * Passing in true disables this button and styles it as disabled
   */
  disabled?: boolean;
  /**
   * Command value to pass to the callback on click
   */
  command: TCommandType;
  /**
   * tooltip or title for the button
   */
  tooltip: string;
}) {
  return (
    <Tooltip title={tooltip}>
      <IconButton
        role="menuitem"
        disabled={disabled}
        onClick={(e) => {
          e.preventDefault();
          onClick(command);
        }}
      >
        {icon}
      </IconButton>
    </Tooltip>
  );
}
