import { NavLink } from "react-router";

/**
 * Banner with a message and an action
 * @param param0 react args
 * @returns jsx tree to render
 */
export function ActionBanner({
  to,
  message,
  linkText,
}: {
  to: string;
  message: string;
  linkText: string;
}) {
  return (
    <>
      <p>{message}</p>
      <p>
        <NavLink to={to}>{linkText}</NavLink>
      </p>
    </>
  );
}
