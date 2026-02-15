import { Fieldset, Legend, Field, Label } from "@headlessui/react";
import { RecipeDetailsMaxLength } from "../../../../services/recipe-store";
import { RecipeDetailsEditor } from "../recipe-details-editor/recipe-details-editor.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { useState } from "preact/hooks";
import { RecipeDetailsEditorFullscreen } from "../recipe-details-fullscreen-editor/recipe-details-editor-fullscreen.component";
import styles from "./recipe-details-field-set.module.css";
export function RecipeDetailsFieldSet({
  showMaxLengthError,
  value,
  onChange,
  disabled,
}: {
  showMaxLengthError: boolean;
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}) {
  const [isFullscreen, setIsFullscreen] = useState(false);
  return (
    <>
      <Fieldset disabled={disabled} className={formStyles.fieldSet}>
        <Legend className={formStyles.formLegend}>
          Ingredients and Directions
        </Legend>
        <Field className={formStyles.inputGroup}>
          <Label className={formStyles.label}>Recipe Directions</Label>
          {showMaxLengthError && (
            <span>{`Recipe Directions has a max length of ${RecipeDetailsMaxLength}`}</span>
          )}
          <RecipeDetailsEditor
            value={value}
            onChange={onChange}
            onSizeToggle={() => setIsFullscreen(true)}
            maxLength={RecipeDetailsMaxLength}
            rows={5}
            className={`${formStyles.fieldComplexControl}`}
            textAreaClassName={styles.detailsEditorTextBox}
          />
        </Field>
      </Fieldset>
      {isFullscreen && (
        <RecipeDetailsEditorFullscreen
          value={value}
          onClose={(newValue) => {
            onChange(newValue);
            setIsFullscreen(false);
          }}
          maxLength={RecipeDetailsMaxLength}
        />
      )}
    </>
  );
}
