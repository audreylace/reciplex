import { Fieldset, Field, Label, Input } from "@headlessui/react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";
import {
  SuccessBanner,
  InformationBanner,
} from "../../../core/components/banner/banner.component";
import { RetryBannerComponent } from "../../../core/components/banner/retry-banner.component";
import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { makeCreateRecipeBookPath } from "../../../recipes/route-utils";
import { useActiveUser } from "../../hooks/useActiveUser.hook";
import { useCreateAccountMutation } from "../../hooks/useCreateAccountMutation.hook";
import formCommonStylesModule from "../../../core/form-common/form-common.module.css";
import signUpFormStylesModule from "./sign-up-form.module.css";

export function SignUpForm({ cancel }: ISignUpFormProps) {
  const setActiveUser = useActiveUser((s) => s.setActiveUser);
  const navigate = useNavigate();
  const createAccountMutation = useCreateAccountMutation();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<{ displayName: string }>();

  const onSubmit = handleSubmit(async (data) => {
    const newAccount = await createAccountMutation.mutateAsync(data);
    setActiveUser({
      userKey: newAccount.userKey,
      displayName: newAccount.displayName,
    });
  });

  switch (createAccountMutation.status) {
    case "success":
      return (
        <SuccessBanner
          title="Account Created"
          message="Your account has been created. Create your first book and start cooking."
          buttonCaption="Create Recipe Book"
          onButtonClick={() => navigate(makeCreateRecipeBookPath())}
        />
      );
    case "error":
      return <RetryBannerComponent />;
    case "pending":
      return (
        <InformationBanner
          title="Creating User"
          message="Creating your account. Do not leave or close this window."
        />
      );
    default:
      return <ApplicationErrorBanner />;
    case "idle":
      return (
        <>
          <h2>Welcome</h2>
          <p>Create an account and get cooking with Reciplex!</p>
          <form
            className={signUpFormStylesModule.createAccountForm}
            onSubmit={onSubmit}
          >
            <Fieldset className={formCommonStylesModule.fieldSet}>
              <Field className={formCommonStylesModule.inputGroup}>
                <Label className={formCommonStylesModule.label}>
                  Display Name
                </Label>
                <Input
                  type="text"
                  className={formCommonStylesModule.fieldControl}
                  required
                  maxLength={64}
                  {...register("displayName", {
                    required: true,
                    maxLength: 64,
                  })}
                ></Input>
                {errors.displayName?.type === "required" && (
                  <span>
                    Provide a display name so others can know what to call you
                  </span>
                )}
                {errors.displayName?.type === "maxLength" && (
                  <span>
                    Display name must be no longer than {64} characters
                  </span>
                )}
              </Field>
            </Fieldset>
            <FormButtons>
              <SuccessButton type="submit">Create Account</SuccessButton>
              {cancel && <DangerButton onClick={cancel}>Cancel</DangerButton>}
            </FormButtons>
          </form>
        </>
      );
  }
}

interface ISignUpFormProps {
  cancel?: () => void;
}
