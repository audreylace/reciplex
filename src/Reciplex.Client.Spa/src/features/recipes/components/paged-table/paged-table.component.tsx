import type { ComponentChildren } from "preact";
import pagedTableModuleStyles from "./paged-table.module.css";
import { PageNavigationButton } from "./page-navigation-button.component";
import { mergeClassName } from "../../../core/utils/css-utils";

export function PagedTable({
  navigateNext,
  navigateBack,
  navigateStart,
  navigateEnd,
  rows,
  message,
  title,
  onSelect,
  additionalPaginationControls,
}: IPagedTableProps) {
  return (
    <div className={pagedTableModuleStyles.tableWrapper}>
      <Table>
        <TableHead>
          <TableHeadRow>
            <TableHeaderCell
              className={pagedTableModuleStyles.tableTitle}
              span={2}
            >
              {title}
            </TableHeaderCell>
          </TableHeadRow>
          <TableHeadRow className={pagedTableModuleStyles.hideSmallScreen}>
            <TableHeaderCell>Name</TableHeaderCell>
            <TableHeaderCell>Description</TableHeaderCell>
          </TableHeadRow>
        </TableHead>
        <TableBody>
          {!rows || rows.length === 0 ? (
            <TableMessage>{message}</TableMessage>
          ) : (
            rows.map((r) => (
              <TableRow
                key={r.id}
                onClick={() => {
                  onSelect?.(r);
                }}
              >
                <TableCell>
                  <span className={pagedTableModuleStyles.titleColumnCell}>
                    {r.title}
                  </span>
                </TableCell>
                <TableCell>{r.details}</TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
      <div className={pagedTableModuleStyles.paginationRow}>
        <PaginationControls
          navigateBack={navigateBack}
          navigateEnd={navigateEnd}
          navigateNext={navigateNext}
          navigateStart={navigateStart}
          additionalPaginationControls={additionalPaginationControls}
        />
      </div>
    </div>
  );
}

export interface IPagedTableRow {
  title: string;
  details: string;
  id: string;
}

export interface IPagedTableProps extends IPaginationActions {
  rows?: IPagedTableRow[];
  onSelect?: (row: IPagedTableRow) => void;
  message: ComponentChildren;
  title: ComponentChildren;
}

interface IPaginationActions {
  navigateNext?: () => void;
  navigateBack?: () => void;
  navigateStart?: () => void;
  navigateEnd?: () => void;
  additionalPaginationControls?: ComponentChildren;
}

function TableMessage({ children }: { children?: ComponentChildren }) {
  return (
    <TableRow className={pagedTableModuleStyles.tableMessageRow}>
      <TableCell className={pagedTableModuleStyles.tableMessageCell} span={2}>
        <p className={pagedTableModuleStyles.tableMessage}>{children}</p>
      </TableCell>
    </TableRow>
  );
}

function PaginationControls({
  navigateBack,
  navigateEnd,
  navigateNext,
  navigateStart,
  additionalPaginationControls,
}: IPaginationActions) {
  return (
    <>
      <PageNavigationButton
        icon="bi bi-skip-start"
        hoverIcon="bi bi-skip-start-fill"
        onClick={navigateStart}
        visibilityHidden={!navigateStart}
      />
      <PageNavigationButton
        icon="bi bi-rewind"
        hoverIcon="bi bi-rewind-fill"
        onClick={navigateBack}
        visibilityHidden={!navigateBack}
      />
      <div className={pagedTableModuleStyles.paginationGrow}></div>
      {additionalPaginationControls && (
        <>
          {additionalPaginationControls}
          <div className={pagedTableModuleStyles.paginationGrow}></div>
        </>
      )}
      <PageNavigationButton
        icon="bi bi-fast-forward"
        hoverIcon="bi bi-fast-forward-fill"
        onClick={navigateNext}
        visibilityHidden={!navigateNext}
      />
      <PageNavigationButton
        icon="bi bi-skip-end"
        hoverIcon="bi bi-skip-end-fill"
        onClick={navigateEnd}
        visibilityHidden={!navigateEnd}
      />
    </>
  );
}

function Table({ children }: { children: ComponentChildren }) {
  return <table className={pagedTableModuleStyles.table}>{children}</table>;
}

function TableHead({ children }: { children: ComponentChildren }) {
  return <thead className={pagedTableModuleStyles.tableHead}>{children}</thead>;
}

function TableHeadRow({
  children,
  className,
}: {
  children: ComponentChildren;
  className?: string;
}) {
  return (
    <tr
      className={mergeClassName(pagedTableModuleStyles.tableHeadRow, className)}
    >
      {children}
    </tr>
  );
}

function TableHeaderCell({
  children,
  className,
  span,
}: {
  children: ComponentChildren;
  className?: string;
  span?: number;
}) {
  return (
    <th
      className={mergeClassName(
        pagedTableModuleStyles.tableHeadCell,
        className,
      )}
      colSpan={span}
    >
      {children}
    </th>
  );
}

function TableBody({ children }: { children: ComponentChildren }) {
  return <tbody className={pagedTableModuleStyles.tableBody}>{children}</tbody>;
}

function TableRow({
  children,
  className,
  onClick,
}: {
  children: ComponentChildren;
  className?: string;
  onClick?: () => void;
}) {
  return (
    <tr
      className={mergeClassName(pagedTableModuleStyles.tableBodyRow, className)}
      onClick={onClick}
      onKeyDown={(e) => {
        if (e.key === " ") {
          e.preventDefault();
        }
      }}
      onKeyUp={(e) => {
        if (e.key === " " || e.key === "Enter") {
          onClick?.();
          e.preventDefault();
        }
      }}
      tabIndex={0}
    >
      {children}
    </tr>
  );
}

function TableCell({
  children,
  span,
  className,
}: {
  children: ComponentChildren;
  span?: number;
  className?: string;
}) {
  return (
    <td
      className={mergeClassName(
        pagedTableModuleStyles.tableBodyCell,
        className,
      )}
      colSpan={span}
    >
      {children}
    </td>
  );
}
