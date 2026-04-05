/* eslint-disable react-hooks/incompatible-library */
import { Field, Fieldset, Input, Label, Legend } from "@headlessui/react";
import formCommonStylesModule from "../../../core/form-common/form-common.module.css";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import type { IHttpUserJson } from "../../http-clients/users-http-client";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { useForm } from "react-hook-form";
import accountSettingsPageStylesModule from "./account-settings-page.module.css";
import { useNavigate, useParams } from "react-router";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";
import {
  ErrorBanner,
  InformationBanner,
  SuccessBanner,
} from "../../../core/components/banner/banner.component";
import { useUpdateAccountMutation } from "../../hooks/useUpdateAccountMutation.hook";
import { ActionFailedTryAgainCancel } from "../../../recipes/components/action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import { makeBookListPath } from "../../../recipes/route-utils";
import { useAuthClients } from "../../hooks/useAuthClients.hook";
import { useAsyncResult } from "../../../core/hooks/useAsyncResult.hook";
import { useState } from "preact/hooks";
import { useDeleteAccountMutation } from "../../hooks/useDeleteAccountMutation.hook";
import { ChallengeGuard } from "../../components/challenge-guard/challenge-guard.component";

export function AccountSettingsPage() {
  const { accountKey } = useParams<{ accountKey: string }>();

  if (!accountKey) {
    return <ApplicationErrorBanner />;
  }

  return (
    <main className={formCommonStylesModule.formMain}>
      <ChallengeGuard>
        <PageBody accountKey={accountKey} />
      </ChallengeGuard>
    </main>
  );
}

function PageBody({ accountKey }: { accountKey: string }) {
  const { usersClient } = useAuthClients();
  const accountsQuery = useAsyncResult({
    asyncAction: () => usersClient.getAccounts({ noCache: true }),
  });

  const [hideDelete, setHideDelete] = useState(false);
  const [hideSettings, setHideSettings] = useState(false);

  let account: IHttpUserJson | undefined;
  switch (accountsQuery.state) {
    case "error":
    default:
      return <ApplicationErrorBanner />;

    case "pending":
      return <InformationBanner title="Fetching accounts" />;

    case "success":
      account = accountsQuery.data.find((a) => a.userKey === accountKey);
      if (!account) {
        return (
          <ErrorBanner
            title="Not Found"
            message="Requested account was not found"
            buttonCaption="Select Account"
            to={accountSelectPath}
          ></ErrorBanner>
        );
      }

      return (
        <div className={accountSettingsPageStylesModule.wrapper}>
          {!hideSettings && (
            <AccountSettingsForm
              account={account}
              onActionOccurred={() => setHideDelete(true)}
            />
          )}
          {!hideDelete && (
            <DeleteAccountForm
              account={account}
              onActionOccurred={() => setHideSettings(true)}
            />
          )}
        </div>
      );
  }
}

function AccountSettingsForm({
  account,
  onActionOccurred,
}: {
  account: IHttpUserJson;
  onActionOccurred: () => void;
}) {
  const { register, formState, handleSubmit } = useForm<{
    displayName: string;
  }>({
    defaultValues: {
      displayName: account.displayName,
    },
  });
  const accountMutation = useUpdateAccountMutation();
  const navigate = useNavigate();
  const navigateAccountSelect = useSelectAccountNavigate();

  if (accountMutation.status === "pending") {
    return <InformationBanner title="saving changes" />;
  }

  if (accountMutation.status === "error") {
    return (
      <ActionFailedTryAgainCancel
        message="failed to save changes to account"
        cancelCaption="go back"
        cancelAction={navigateAccountSelect}
      />
    );
  }

  if (accountMutation.status === "success") {
    return (
      <SuccessBanner
        title="Account Updated"
        message="Account settings were saved."
        buttonCaption="View Books"
        onButtonClick={() => navigate(makeBookListPath())}
      />
    );
  }

  return (
    <form
      onSubmit={handleSubmit(async (data) => {
        onActionOccurred();
        await accountMutation.mutateAsync({
          userKey: account.userKey,
          displayName: data.displayName,
          concurrencyToken: account.concurrencyTag,
        });
      })}
    >
      <Fieldset className={formCommonStylesModule.fieldSet}>
        <Legend className={formCommonStylesModule.formLegend}>
          Account Settings for {account.displayName} ({account.userKey})
        </Legend>
        <Field className={formCommonStylesModule.inputGroup}>
          <Label className={formCommonStylesModule.label}>Display Name</Label>
          <Input
            type="text"
            className={formCommonStylesModule.fieldControl}
            required
            maxLength={64}
            placeholder={"Provide a Display Name"}
            {...register("displayName", {
              required: true,
              maxLength: 64,
            })}
          ></Input>
          {formState.errors.displayName?.type === "required" && (
            <span>
              Provide a display name so others can know what to call you
            </span>
          )}
          {formState.errors.displayName?.type === "maxLength" && (
            <span>Display name must be no longer than {64} characters</span>
          )}
        </Field>
      </Fieldset>
      <FormButtons>
        <SuccessButton type="submit">Save</SuccessButton>
      </FormButtons>
    </form>
  );
}

function DeleteAccountForm({
  account,
  onActionOccurred,
}: {
  account: IHttpUserJson;
  onActionOccurred: () => void;
}) {
  const { register, watch, formState, handleSubmit } = useForm<{
    displayName: string;
    enabled: boolean;
  }>();

  const showForm = watch("enabled");

  const deleteAccountMutation = useDeleteAccountMutation();
  const navigateAccountSelect = useSelectAccountNavigate();

  if (deleteAccountMutation.status === "pending") {
    return <InformationBanner title="deleting account..." />;
  }

  if (deleteAccountMutation.status === "error") {
    return (
      <ActionFailedTryAgainCancel
        message="failed to delete account"
        cancelCaption="go back"
        cancelAction={navigateAccountSelect}
      />
    );
  }

  if (deleteAccountMutation.status === "success") {
    return (
      <SuccessBanner
        title="Account Deleted"
        message="Account was deleted."
        buttonCaption="View Accounts"
        onButtonClick={navigateAccountSelect}
      />
    );
  }

  return (
    <form
      onSubmit={handleSubmit((data) => {
        if (!data.enabled) {
          return;
        }
        if (data.displayName !== account.displayName) {
          return;
        }
        onActionOccurred();
        deleteAccountMutation.mutateAsync({
          userKey: account.userKey,
          concurrencyToken: account.concurrencyTag,
        });
      })}
    >
      <Fieldset className={formCommonStylesModule.fieldSet}>
        <Legend className={formCommonStylesModule.formLegend}>
          Delete Account
        </Legend>
        <Field className={formCommonStylesModule.inputGroupCheck}>
          <Input
            type="checkbox"
            required
            {...register("enabled", {
              required: true,
            })}
          ></Input>
          <Label className={formCommonStylesModule.label}>Delete Account</Label>
        </Field>
        {showForm && (
          <>
            <Field className={formCommonStylesModule.inputGroup}>
              <Label className={formCommonStylesModule.label}>
                Type {`'${account.displayName}'`} to delete account
              </Label>
              <Input
                type="text"
                className={formCommonStylesModule.fieldControl}
                required
                maxLength={64}
                placeholder={account.displayName}
                {...register("displayName", {
                  required: true,
                  maxLength: 64,
                  validate: (value) => {
                    return (
                      account.displayName === value ||
                      `type "${account.displayName}"`
                    );
                  },
                })}
              ></Input>
              {formState.errors.displayName && (
                <span>{formState.errors.displayName.message}</span>
              )}
              {formState.errors.displayName?.type === "required" && (
                <span>{`type "${account.displayName}"`}</span>
              )}
            </Field>
          </>
        )}
      </Fieldset>
      {showForm && (
        <FormButtons>
          <DangerButton type="submit">Delete Account</DangerButton>
        </FormButtons>
      )}
    </form>
  );
}
const accountSelectPath = "/accounts/-/select";

function useSelectAccountNavigate() {
  const navigate = useNavigate();
  return () => navigate(accountSelectPath);
}
