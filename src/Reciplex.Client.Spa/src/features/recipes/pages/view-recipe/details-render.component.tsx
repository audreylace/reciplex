import Markdown from "react-markdown";
import rehypeSanitize from "rehype-sanitize";

import detailsRenderStyleModule from "./details-render.module.css";

/** renders the recipe details */
export function DetailsRender(props: DetailsRenderProps) {
  return (
    <>
      <h4>Recipe Details</h4>
      <div className={detailsRenderStyleModule.detailsWrapper}>
        <DetailsMdRender {...props} />
      </div>
    </>
  );
}

/** props for  `DetailsRender` */
export interface DetailsRenderProps {
  detailsMd?: string;
  mayEdit: boolean;
  goToEditAction: () => void;
}

/** inner md render */
function DetailsMdRender({
  detailsMd,
  mayEdit,
  goToEditAction,
}: DetailsRenderProps) {
  if (!detailsMd) {
    if (!mayEdit) {
      return (
        <p>
          <i>No details</i>
        </p>
      );
    }
    return (
      <p
        className={detailsRenderStyleModule.emptyDetails}
        onClick={goToEditAction}
      >
        <i>Click to edit and add details</i>
      </p>
    );
  }

  return <Markdown rehypePlugins={[rehypeSanitize]}>{detailsMd}</Markdown>;
}
