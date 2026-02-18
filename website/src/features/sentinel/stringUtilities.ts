
/**
 * Asserts that a value is a string; throw if 's' is not a string, null, or undefined
 * @param s the value to check
 * @returns `s` if it is a string
 */
export function AssertString(s: string | null | undefined) : string {
    if(!s) {
        throw new Error("expected a not null string");
    }

    if(typeof s !== "string") {
        throw new Error(`expected a string type but got ${typeof(s)} : value is ${s}`);
    }
    return s;
}