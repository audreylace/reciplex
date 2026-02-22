import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { RecipeBookMetaFields } from "../../components/recipe-book-meta-fields/recipe-book-meta-fields.component";
import { RetryBannerComponent } from "../../components/retry-banner/retry-banner.component";
import { useCreateRecipeBookPage } from "./useCreateRecipeBookPage.hook";
import formStyles from "../../../core/form-common/form-common.module.css";

/**
 * Create recipe book page component
 */
export function CreateRecipeBookPage() {
  const { state, onSubmit, register, errors, onCancel } =
    useCreateRecipeBookPage();
  return (
    <main className={`${formStyles.formMain}`}>
      {state === "idle" && (
        <form onSubmit={onSubmit}>
          <RecipeBookMetaFields
            register={register}
            legend="Create New Recipe Book"
            errors={errors}
          />
          <FormButtons>
            <SuccessButton type="submit">Create</SuccessButton>
            <DangerButton onClick={onCancel}>Cancel</DangerButton>
          </FormButtons>
        </form>
      )}
      {state === "pending" && <p>Creating recipe book...</p>}
      {state === "error" && (
        <RetryBannerComponent message="Creating recipe book failed." />
      )}
    </main>
  );
}
