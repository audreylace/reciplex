import Skeleton from "@mui/material/Skeleton";
import { AddAccountButton } from "./add-account-button.component";
import Stack from "@mui/material/Stack";
import Fade from "@mui/material/Fade";
import { TopBanner } from "./top-banner.component";

/** Placeholder shown while the page loads */
export function ComponentSkeleton() {
  return (
    <>
      <Fade
        in
        style={{
          transitionDelay: "400ms",
        }}
        unmountOnExit
      >
        <div>
          <Skeleton>
            <TopBanner />
          </Skeleton>
          <Stack spacing={2}>
            <Skeleton variant="rectangular">
              <AddAccountButton />
            </Skeleton>
            <Skeleton variant="rectangular" height={90} />
            <Skeleton variant="rectangular" height={90} />
            <Skeleton variant="rectangular" height={90} />
            <Skeleton variant="rectangular">
              <AddAccountButton />
            </Skeleton>
          </Stack>
        </div>
      </Fade>
    </>
  );
}
