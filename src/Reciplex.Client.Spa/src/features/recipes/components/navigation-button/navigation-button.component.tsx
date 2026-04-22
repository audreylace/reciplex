import IconButton from "@mui/material/IconButton";
import type { ComponentChildren } from "preact";
import { useSearchParams } from "react-router";

export function NavigationButton({
  children,
  at,
  source,
  disabled,
}: INavigationButtonProps) {
  const setSearchParams = useSearchParams()[1];

  return (
    <IconButton
      disabled={disabled}
      onClick={() => {
        if (at) {
          setSearchParams({
            at: at,
            source: source,
          });
          return;
        }

        setSearchParams({
          source: source,
        });
      }}
    >
      {children}
    </IconButton>
  );
}

export interface INavigationButtonProps {
  at?: string;
  source: "next" | "previous";
  disabled?: boolean;
  children?: ComponentChildren;
}
