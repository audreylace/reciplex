import {
  bold,
  getCommands,
  handleKeyDown,
  shortcuts,
  TextAreaCommandOrchestrator,
  heading1,
  heading2,
  heading3,
  heading4,
  heading5,
  heading6,
  italic,
  unorderedListCommand,
  orderedListCommand,
  fullscreen,
  divider,
  code,
  quote,
} from "@uiw/react-md-editor/nohighlight";
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
import { useCallback, useEffect, useMemo, useRef } from "preact/hooks";

type StringFormKey<
  TFormModel extends FieldValues,
  TFieldName extends FieldPath<TFormModel>,
> = FieldPathValue<TFormModel, TFieldName> extends string ? TFieldName : never;

import commonFormStyles from "../../../core/form-common/form-common.module.css";
import { Field, Fieldset, Label, Legend, Textarea } from "@headlessui/react";

import styles from "./recipe-details-editor.module.css";
import { link } from "@uiw/react-md-editor";

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

  const textareaRef = useRef(null);
  const orchestratorRef = useRef<TextAreaCommandOrchestrator | null>(null);

  useEffect(() => {
    if (textareaRef.current) {
      orchestratorRef.current = new TextAreaCommandOrchestrator(
        textareaRef.current,
      );
    }
  }, []);

  const onKeyDown = (e: any) => {
    handleKeyDown(e, 2, false);
    if (orchestratorRef.current) {
      shortcuts(e, getCommands(), orchestratorRef.current);
    }
  };

  return (
    <Fieldset disabled={disabled} className={commonFormStyles.fieldSet}>
      <Legend className={commonFormStyles.formLegend}>{legendText}</Legend>
      <Field className={commonFormStyles.inputGroup}>
        <Label className={commonFormStyles.label}>{label}</Label>
        {maxLength && errors[name]?.type === "maxLength" && (
          <span>{maxLengthMessage}</span>
        )}
        <ul className={styles.editorMenu}>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(bold);
              }}
            >
              <i class="bi bi-type-bold"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(italic);
              }}
            >
              <i class="bi bi-type-italic"></i>
            </button>
          </li>
          <li className={styles.menuDivider}>&#8203;</li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(heading1);
              }}
            >
              <i class="bi bi-type-h1"></i>
            </button>
          </li>

          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(heading2);
              }}
            >
              <i class="bi bi-type-h2"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(heading3);
              }}
            >
              <i class="bi bi-type-h3"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(heading4);
              }}
            >
              <i class="bi bi-type-h4"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(heading5);
              }}
            >
              <i class="bi bi-type-h5"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(heading6);
              }}
            >
              <i class="bi bi-type-h6"></i>
            </button>
          </li>
          <li className={styles.menuDivider}>&#8203;</li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(code);
              }}
            >
              <i class="bi bi-quote"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(quote);
              }}
            >
              <i class="bi bi-blockquote-left"></i>
            </button>
          </li>
          <li className={styles.menuDivider}>&#8203;</li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(unorderedListCommand);
              }}
            >
              <i class="bi bi-list-ul"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(orderedListCommand);
              }}
            >
              <i class="bi bi-list-ol"></i>
            </button>
          </li>
          <li className={styles.menuDivider}>
            <div>&#8203;</div>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                const orchestrator = orchestratorRef.current;
                if (!orchestrator) {
                  return;
                }

                const state = orchestrator.getState();
                if (!state) {
                  return;
                }
                let modifyText = "`{{@ingredient " + state.selectedText + "}}`";

                if (!state.selectedText) {
                  modifyText = "`{{@ingredient ###}}`";
                }
                orchestrator.textApi.replaceSelection(modifyText);
              }}
            >
              <i class="bi bi-cart4"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                const orchestrator = orchestratorRef.current;
                if (!orchestrator) {
                  return;
                }

                const state = orchestrator.getState();
                if (!state) {
                  return;
                }
                let modifyText = "`{{@section " + state.selectedText + "}}`";

                if (!state.selectedText) {
                  modifyText = "`{{@selection ###}}`";
                }
                orchestrator.textApi.replaceSelection(modifyText);
              }}
            >
              <i class="bi bi-puzzle-fill"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                const orchestrator = orchestratorRef.current;
                if (!orchestrator) {
                  return;
                }

                const state = orchestrator.getState();
                if (!state) {
                  return;
                }
                let modifyText = "(" + state.selectedText + ")[recipe://]";

                if (!state.selectedText) {
                  modifyText = "()[recipe://]";
                }
                orchestrator.textApi.replaceSelection(modifyText);
              }}
            >
              <i class="bi bi-fork-knife"></i>
            </button>
          </li>
          <li>
            <button
              onClick={(e) => {
                e.preventDefault();
                orchestratorRef.current?.executeCommand(link);
              }}
            >
              <i class="bi bi-link-45deg"></i>
            </button>
          </li>
        </ul>

        <Textarea
          className={`${styles.detailsEditor}`}
          ref={textareaRef}
          value={value}
          onChange={(e: any) => onChange(e.target.value)}
          onKeyDown={onKeyDown}
          rows={8}
        />
      </Field>
    </Fieldset>
  );
}
