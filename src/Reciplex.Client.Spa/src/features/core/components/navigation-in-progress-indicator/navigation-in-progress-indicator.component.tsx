import Dialog from "@mui/material/Dialog";
import { useNavigation } from "react-router";
import { LoadingIndicator } from "../loading-indicator/loading-indicator.component";
import { ContentWrapper } from "../content-wrapper/content-wrapper.component";
import { useEffect, useState } from "react";
import { FlyUpTransition } from "./fly-up-transition.component";

/** component that shows a full screen loading indicator when navigation is slow */
export function NavigationInProgressIndicatorComponent() {
  const navigation = useNavigation();
  const isLoading = navigation.state === "loading";
  const [showDialog, setShowDialog] = useState(false);
  if (!isLoading && showDialog) {
    setShowDialog(false);
  }

  useEffect(() => {
    if (!isLoading) {
      return;
    }
    let mounted = true;
    const handle = setTimeout(() => {
      if (mounted) {
        setShowDialog(true);
      }
    }, 600);

    return () => {
      mounted = false;
      clearTimeout(handle);
    };
  }, [isLoading]);

  if (!showDialog) {
    return null;
  }

  return (
    <Dialog
      open
      slots={{
        transition: FlyUpTransition,
      }}
    >
      <ContentWrapper>
        <LoadingIndicator />
      </ContentWrapper>
    </Dialog>
  );
}
