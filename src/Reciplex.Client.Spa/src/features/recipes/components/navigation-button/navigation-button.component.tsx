import IconButton from "@mui/material/IconButton";
import { useSearchParams } from "react-router";

export function NavigationButton({
  children,
  at,
  source,
  disabled,
  loading,
}: INavigationButtonProps) {
  const setSearchParams = useSearchParams()[1];

  return (
    <IconButton
      loading={loading}
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
  children?: React.ReactNode;
  loading?: boolean;
}
