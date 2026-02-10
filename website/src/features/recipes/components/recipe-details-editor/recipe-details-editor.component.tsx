import {
  getCommands,
  handleKeyDown,
  shortcuts,
  TextAreaCommandOrchestrator,
} from "@uiw/react-md-editor/nohighlight";
import {
  type FieldErrors,
  type FieldPath,
  type FieldPathValue,
  type FieldValues,
  type UseFormRegister,
  type UseFormSetValue,
  type UseFormWatch,
} from "react-hook-form";
import {
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
} from "preact/hooks";

type StringFormKey<
  TFormModel extends FieldValues,
  TFieldName extends FieldPath<TFormModel>,
> = FieldPathValue<TFormModel, TFieldName> extends string ? TFieldName : never;

import commonFormStyles from "../../../core/form-common/form-common.module.css";
import {
  Dialog,
  DialogPanel,
  Field,
  Fieldset,
  Label,
  Legend,
  Textarea,
} from "@headlessui/react";

import styles from "./recipe-details-editor.module.css";
import { MenuBar } from "./menu-bar.component";
import { useMenuBarCommandHandler } from "./useMenuBarCommandHandler.hook";

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
  legendText,
  disabled,
  errors,
  name,
  register,
  label,
  maxLengthMessage,
  maxLength,
  watch,
  setValue,
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
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const orchestratorRef = useRef<TextAreaCommandOrchestrator | null>(null);
  const { ref, ...theRest } = register(name);
  const textAreaValue = watch(name);

  const menuCommandHandler = useMenuBarCommandHandler(orchestratorRef);

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

  const [isFullscreen, setIsFullscreen] = useState(false);

  return (
    <Fieldset disabled={disabled} className={commonFormStyles.fieldSet}>
      {isFullscreen && (
        <MarkdownPopup
          maxLength={maxLength}
          value={textAreaValue}
          onClose={(s) => {
            setValue(name, s as unknown as any, {
              shouldDirty: true,
              shouldTouch: true,
              shouldValidate: true,
            });
            setIsFullscreen(false);
          }}
        />
      )}
      <Legend className={commonFormStyles.formLegend}>{legendText}</Legend>
      <Field className={commonFormStyles.inputGroup}>
        <Label className={commonFormStyles.label}>{label}</Label>
        {maxLength && errors[name]?.type === "maxLength" && (
          <span>{maxLengthMessage}</span>
        )}
        <div
          className={`${styles.detailsEditor} ${commonFormStyles.fieldComplexControl}`}
          tabIndex={-1}
        >
          <MenuBar
            disabled={disabled}
            commandHandler={menuCommandHandler}
            isFullscreen={false}
            onSizeToggle={() => setIsFullscreen(true)}
          />
          <Textarea
            className={`${styles.textArea}`}
            ref={(r: any | null) => {
              textareaRef.current = r;
              ref(r);
            }}
            rows={8}
            onKeyDown={onKeyDown}
            {...theRest}
            maxlength={maxLength}
          />
        </div>
      </Field>
    </Fieldset>
  );
}

function MarkdownPopup({
  maxLength,
  onClose,
  value,
}: {
  maxLength?: number;
  onClose: (newValue: string) => void;
  value: string;
}) {
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const orchestratorRef = useRef<TextAreaCommandOrchestrator | null>(null);
  const menuCommandHandler = useMenuBarCommandHandler(orchestratorRef);
  const [textAreaValue, setTextAreaValue] = useState(value);

  useLayoutEffect(() => {
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
    <Dialog
      open={true}
      onClose={() => {
        onClose(textareaRef.current?.value ?? value);
      }}
    >
      <DialogPanel className={styles.dialogPanel}>
        <div className={`${styles.detailsEditorFullScreen}`}>
          <MenuBar
            commandHandler={menuCommandHandler}
            isFullscreen
            onSizeToggle={() => onClose(textareaRef.current?.value ?? value)}
          />
          <Textarea
            className={`${styles.textAreaFullScreen}`}
            ref={textareaRef}
            onKeyDown={onKeyDown}
            maxlength={maxLength}
            value={textAreaValue}
            onChange={(e) =>
              setTextAreaValue((e.target as HTMLTextAreaElement).value)
            }
          />
        </div>
      </DialogPanel>
    </Dialog>
  );
}

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
