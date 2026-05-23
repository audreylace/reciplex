import { useLayoutEffect, useState } from "react";

export function useFullscreenState(): useFullscreenStateHookReturn {
  const fullscreenEnabled = document.fullscreenEnabled;

  const initialValue = fullscreenEnabled
    ? document.fullscreenElement
      ? "active"
      : "inactive"
    : "not-supported";

  const [fullscreen, setFullscreen] = useState<FullscreenKeys>(initialValue);
  // trigger close when user exists fullscreen mode
  useLayoutEffect(() => {
    // always set state inside initial use effect mount in
    // case something changed between creation and effect invocation
    fullscreenStateUpdater(setFullscreen);

    // subscribe to changes if fullscreen mode is supported
    if (document.fullscreenEnabled) {
      const subscriber = () => {
        fullscreenStateUpdater(setFullscreen);
      };
      addEventListener("fullscreenchange", subscriber);

      // cleanup listener on demount
      return () => {
        removeEventListener("fullscreenchange", subscriber);
      };
    }
  }, []);

  return {
    fullscreen,
    fullscreenEnabled: document.fullscreenEnabled,
    enterFullscreen: hookEnterFullscreen,
    exitFullscreen: hookExitFullscreen,
  };
}

/** return from `useFullscreenState` */
export interface useFullscreenStateHookReturn {
  /**
   * 'active' when the browser is in fullscreen mode
   * 'inactive' when the browser is not in fullscreen mode
   * 'not-supported' when the browser does not allow or support fullscreen mode
   */
  fullscreen: FullscreenKeys;
  /**
   * True if and only if the fullscreen API is supported
   * and the browser allows entering and exiting full screen.
   */
  fullscreenEnabled: boolean;
  /**
   * Enter fullscreen mode
   * @returns promise that resolves once the browser is fullscreen.
   * If fullscreen is not supported, then this does nothing and resolves right away.
   */
  enterFullscreen: () => Promise<void>;
  /**
   *
   * @returns Promise that resolves once the browser has exited full screen.
   * If fullscreen is not supported, then this does nothing and resolves right away.
   */
  exitFullscreen: () => Promise<void>;
}

type FullscreenKeys = "active" | "inactive" | "not-supported";

/**
 * helper method to calculate fullscreen state
 * @param stateSetter react state setter invoked with the current state
 */
function fullscreenStateUpdater(
  stateSetter: (nextState: FullscreenKeys) => void,
) {
  const fullscreenEnabled = document.fullscreenEnabled;

  const nextValue = fullscreenEnabled
    ? document.fullscreenElement
      ? "active"
      : "inactive"
    : "not-supported";

  console.log(nextValue);

  stateSetter(nextValue);
}

function hookEnterFullscreen() {
  if (
    document.fullscreenEnabled &&
    document.fullscreenElement !== document.body
  ) {
    return document.body.requestFullscreen();
  }

  return Promise.resolve();
}

function hookExitFullscreen() {
  if (document.fullscreenEnabled && document.fullscreenElement) {
    return document.exitFullscreen();
  }
  return Promise.resolve();
}
