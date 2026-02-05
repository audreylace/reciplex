import { Link } from "react-router";

/**
 * Renders a set of table rows as a skeleton
 */
export function TableRowsSkeleton({
  count,
}: {
  /**
   * The number of rows to render
   */
  count: number;
}) {
  const rows = [];
  // tie key to tuple such that if the count changes,
  // then all of the rows get collected and destroyed
  for (let i = 0; i < count; i++) {
    rows.push(<TableRow key={`${count}_${i}`} />);
  }

  return <>{rows}</>;
}

/**
 * Renders a single skeleton row
 */
function TableRow() {
  return (
    <tr className="placeholder-glow">
      <td>
        <span class="placeholder placeholder-lg col-6"></span>
      </td>
      <td>
        <span class="placeholder col-12"></span>
        <span class="placeholder col-8"></span>
      </td>
      <td>
        <Link to="#">
          <i class="bi bi-arrow-right-circle"></i>
        </Link>
      </td>
    </tr>
  );
}
