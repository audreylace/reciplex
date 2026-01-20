import MDEditor from "@uiw/react-md-editor";
import rehypeSanitize from "rehype-sanitize";
import {
  type FieldErrors,
  type FieldPath,
  type FieldPathValue,
  type FieldValues,
  type UseFormRegister,
  type UseFormSetValue,
  type UseFormWatch,
} from "react-hook-form";
import { useCallback, useMemo } from "preact/hooks";

type StringFormKey<
  TFormModel extends FieldValues,
  TFieldName extends FieldPath<TFormModel>,
> = FieldPathValue<TFormModel, TFieldName> extends string ? TFieldName : never;

/**
 * Max length field constraint
 */
type EditorMaxLength = {
  value: number;
  message: string;
};

/**
 * Markdown editor common fields
 */
type EditorCommon<
  TFormModel extends FieldValues,
  TFieldName extends FieldPath<TFormModel>,
> = {
  /**
   * text for the legend
   */
  legendText: string;
  /**
   * controls if the form is disabled
   */
  disabled?: boolean;
  /**
   * form register method
   */
  register: UseFormRegister<TFormModel>;
  /**
   * validation errors if any
   */
  errors: FieldErrors<TFormModel>;
  /**
   * form method for setting values in response to input changes
   */
  setValue: UseFormSetValue<TFormModel>;
  /**
   * form hook for triggering a rerender on value change
   */
  watch: UseFormWatch<TFormModel>;
  /**
   * name of the form field
   */
  name: StringFormKey<TFormModel, TFieldName>;
  /**
   * field label
   */
  label: string;
};

/**
 * markdown editor for recipes
 * @param param0 react props array
 * @returns react jsx tree for rendering
 */
export function RecipeMarkdownEditor<
  TFormModel extends FieldValues,
  TFieldName extends FieldPath<TFormModel>,
>({
  maxLength,
  ...theRest
}: {
  /**
   * max length object. Component takes care to ensure the object
   * reference does not cause the entire markdown editor to re-render.
   * Calling codes does not need to use a memo and can just provide
   * the object.
   */
  maxLength?: EditorMaxLength;
} & EditorCommon<TFormModel, TFieldName>) {
  // use memo to destructure object into primitives to avoid extra renders
  // while still allowing us to have a safe API to use
  const [maxLengthMessage, maxLengthValue] = useMemo(() => {
    return [maxLength?.message, maxLength?.value];
  }, [maxLength?.message, maxLength?.value]);

  return (
    <RecipeMarkdownEditorInternal
      {...theRest}
      maxLength={maxLengthValue}
      maxLengthMessage={maxLengthMessage}
    />
  );
}

/**
 * Internal implementation of @see RecipeMarkdownEditor
 */
function RecipeMarkdownEditorInternal<
  TFormModel extends FieldValues,
  TFieldName extends FieldPath<TFormModel>,
>({
  setValue,
  legendText,
  disabled,
  register,
  errors,
  watch,
  name,
  label,
  maxLengthMessage,
  maxLength,
}: {
  /**
   * max length
   */
  maxLength?: number;
  /**
   * max length error message
   */
  maxLengthMessage?: string;
} & EditorCommon<TFormModel, TFieldName>) {
  const value = watch(name);
  const onChange = useCallback(
    (value: string | undefined | null) =>
      setValue(name, (value ?? "") as unknown as any, {
        shouldDirty: true,
        shouldTouch: true,
        shouldValidate: true,
      }),
    [setValue],
  );
  return (
    <fieldset disabled={disabled}>
      <legend>{legendText}</legend>
      {maxLength && errors[name]?.type === "maxLength" && (
        <span>{maxLengthMessage}</span>
      )}
      <label>
        {label}
        <MDEditor
          value={value}
          onChange={onChange}
          autoFocus={true}
          preview="edit"
          previewOptions={{
            rehypePlugins: [[rehypeSanitize]],
          }}
          textareaProps={{
            ...register(name, {
              maxLength: maxLength,
            }),
          }}
        />
      </label>
    </fieldset>
  );
}
