/** simple abstraction for iterating through a string */
export class StringIterator {
  /** the string the iterator wraps */
  private _wrappedString: string;
  /** iterator position */
  private _iteratorIndex: number = 0;

  /**
   * constructor
   * @param s the string value
   * @param offset the starting offset
   */
  constructor(s: string, offset?: number) {
    this._wrappedString = s;
    this._iteratorIndex = offset ?? 0;
  }

  /** the current index of the iterator */
  public get index(): number {
    return this._iteratorIndex;
  }

  /** sets the iterator index */
  public set index(value: number) {
    this._iteratorIndex = value;
  }

  /** the string value being iterated */
  public get stringValue(): string {
    return this._wrappedString;
  }

  /**
   * moves the iterator forward by one
   * @returns `true` if the iterator could be advanced
   */
  public moveNext(): boolean {
    if (this._iteratorIndex < this._wrappedString.length) {
      this._iteratorIndex++;
      return true;
    }

    return false;
  }

  /** the value at the current index. throws if `hasValue` would return false */
  public get current(): string {
    if (!this.hasValue) {
      throw new Error("out of bound index");
    }
    return this._wrappedString[this._iteratorIndex];
  }

  /** if the `current` has a value */
  public get hasValue(): boolean {
    if (this._iteratorIndex >= this._wrappedString.length) {
      return false;
    }
    return true;
  }

  /**
   * Does a shallow clone the existing iterator's state
   * @returns the iterator
   */
  public clone(): StringIterator {
    const newIterator = new StringIterator(
      this._wrappedString,
      this._iteratorIndex,
    );
    return newIterator;
  }

  public sliceSegment(other: StringIterator, includeCurrent?: boolean) {
    if (other._wrappedString !== this._wrappedString) {
      throw new Error("both string iterators must reference the same string");
    }

    if (other.index < this._iteratorIndex) {
      throw new Error("other should be at an advanced position");
    }

    const offset = includeCurrent ? 1 : 0;

    return this._wrappedString.substring(
      Math.min(this._wrappedString.length, this._iteratorIndex),
      Math.min(this._wrappedString.length, other._iteratorIndex + offset),
    );
  }
}
