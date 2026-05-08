export function QuotedValue({ value }: IQuotedValueProps) {
  return <>{`"${value}"`}</>;
}

export interface IQuotedValueProps {
  value: string;
}
