import { SuccessButton } from "../../../core/components/buttons/success-button.component";

/**
 * Menu button component inside the markdown recipe details editor menu bar
 */
export function MenuButton<TCommandType>({
  iconName,
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
   * icon name passed to `<i class={iconName}></i>`
   */
  iconName: string;
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
    <li>
      <SuccessButton
        buttonType="hidden"
        title={tooltip}
        role="menuitem"
        disabled={disabled}
        onClick={(e) => {
          e.preventDefault();
          onClick(command);
        }}
      >
        <i className={iconName}></i>
      </SuccessButton>
    </li>
  );
}
