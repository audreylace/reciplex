import type { IToken, TokenType } from "chevrotain";

/**
 * Helper for changing `IToken.payload` from `any` to `T`.
 */
export type ITokenWithPayload<T> = Omit<IToken, "payload"> & { payload: T };

export type INamedTokenType<TName> = Omit<TokenType, "name"> & { name: TName };
