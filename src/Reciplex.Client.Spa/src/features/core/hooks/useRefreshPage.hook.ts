import { useCallback } from "react";
import { useLocation, useNavigate } from "react-router";

export function useRefreshPage(): [string, () => void] {
  const navigate = useNavigate();
  const location = useLocation();
  return [location.pathname, useCallback(() => navigate(0), [navigate])];
}
