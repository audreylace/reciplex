import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { RecipeBookMetaFields } from "../../components/recipe-book-meta-fields/recipe-book-meta-fields.component";
import { RetryBannerComponent } from "../../../core/components/banner/retry-banner.component";
import { useCreateRecipeBookPage } from "./useCreateRecipeBookPage.hook";
import formStyles from "../../../core/form-common/form-common.module.css";
import { InformationBanner } from "../../../core/components/banner/banner.component";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";

/**
 * Create recipe book page component
 */
export function CreateRecipeBookPage() {
  const { state, onSubmit, register, errors, onCancel } =
    useCreateRecipeBookPage();

  return (
    <main className={`${formStyles.formMain}`}>
      {(() => {
        switch (state) {
          case "idle":
            return (
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
            );
          case "pending":
            return (
              <InformationBanner
                title="Creating Recipe Book"
                message="Publishing new recipe book to the cloud. Do not leave or close this window."
              />
            );
          case "error":
            return (
              <RetryBannerComponent message="Creating recipe book failed." />
            );
          default:
            <ApplicationErrorBanner />;
        }
      })()}
    </main>
  );
}
