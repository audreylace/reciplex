export function mergeClassName(...classNames: (string | undefined)[]) {
  return classNames.filter((s) => s).join(" ");
}
