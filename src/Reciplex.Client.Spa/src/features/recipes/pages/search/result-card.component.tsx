import Avatar from "@mui/material/Avatar";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardActionArea from "@mui/material/CardActionArea";
import CardContent from "@mui/material/CardContent";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ShareIcon from "@mui/icons-material/Share";
import { NavLink } from "react-router";
// eslint-disable-next-line no-restricted-imports
import type { StandardCSSProperties } from "@mui/system";
import type { ReactNode } from "react";

/**
 * Renders a search result
 */
export function ResultCard({
  name,
  description,
  shared,
  path,
  caption,
  avatarLetter,
  avatarColor,
}: IResultCardProps) {
  return (
    <Card>
      <CardActionArea component={NavLink} to={path}>
        <CardContent>
          <Stack direction={"row"} sx={{ alignItems: "center" }} spacing={2}>
            <Box>
              <Avatar sx={{ bgcolor: avatarColor }}>{avatarLetter}</Avatar>
            </Box>
            <Stack>
              <Typography variant="h4" component="div">
                {name}
                {shared && (
                  <>
                    {" "}
                    <ShareIcon
                      fontSize="small"
                      sx={{ color: "text.secondary" }}
                    />
                  </>
                )}
              </Typography>
              {caption && (
                <Typography
                  variant="caption"
                  sx={{ color: "text.secondary", mb: 1, pl: 0.25 }}
                >
                  {caption}
                </Typography>
              )}
              <Typography variant="body1" sx={{ color: "text.secondary" }}>
                {description}
              </Typography>
            </Stack>
          </Stack>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}

/**
 * Properties for the `<ResultCard />` component
 */
export interface IResultCardProps {
  /**
   * name of the result
   */
  name: string;
  /**
   * result description
   */
  description: string;
  /**
   * flag for if the result is shared, not owned by the user
   */
  shared?: boolean;
  /**
   * path to open on click
   */
  path: string;
  /**
   * the caption for the kind
   */
  caption?: ReactNode;
  /**
   * the single letter to show on the avatar to the left of the card
   */
  avatarLetter: string;
  /**
   * the color of the avatar letter
   */
  avatarColor: StandardCSSProperties["backgroundColor"] | undefined;
}
