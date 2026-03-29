/* eslint-disable react-hooks/incompatible-library */
import {
  Field,
  Fieldset,
  Input,
  Label,
  Legend,
  Switch,
} from "@headlessui/react";
import formCommonStylesModule from "../../../core/form-common/form-common.module.css";
import { AuthenticatedRouteGuard } from "../../components/authenticated-route-guard/authenticated-route-guard.component";
import { useGetAccountsQuery } from "../../hooks/useGetAccountsQuery.hook";
import type { IHttpUserJson } from "../../http-clients/users-http-client";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { useState } from "preact/hooks";
import { useForm } from "react-hook-form";
import accountSettingsPageStylesModule from "./account-settings-page.module.css";
import { useParams } from "react-router";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";
import {
  ErrorBanner,
  InformationBanner,
} from "../../../core/components/banner/banner.component";
import { OfflineBanner } from "../../../core/components/banner/offline-banner.component";

export function AccountSettingsPage() {
  const { accountKey } = useParams<{ accountKey: string }>();

  if (!accountKey) {
    return <ApplicationErrorBanner />;
  }

  return (
    <main className={formCommonStylesModule.formMain}>
      <AuthenticatedRouteGuard>
        <AccountSettingBody accountKey={accountKey} />
      </AuthenticatedRouteGuard>
    </main>
  );
}

function DeleteAccountForm({ account }: { account: IHttpUserJson }) {
  const { register, watch, formState } = useForm<{
    displayName: string;
    enabled: boolean;
  }>();

  const showForm = watch("enabled");
  return (
    <form>
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
      {showForm && <DangerButton type="submit">Delete Account</DangerButton>}
    </form>
  );
}

function AccountSettingsForm({ account }: { account: IHttpUserJson }) {
  const { register, formState } = useForm<{
    displayName: string;
  }>();

  return (
    <form>
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
            value={account.displayName}
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
        <SuccessButton>Save</SuccessButton>
        <DangerButton>Cancel</DangerButton>
      </FormButtons>
    </form>
  );
}

function AccountSettingsForms({ account }: { account: IHttpUserJson }) {
  return (
    <div className={accountSettingsPageStylesModule.wrapper}>
      <AccountSettingsForm account={account} />
      <DeleteAccountForm account={account} />
    </div>
  );
}

function AccountSettingBody({ accountKey }: { accountKey: string }) {
  const accountListQuery = useGetAccountsQuery(true);

  let account: IHttpUserJson | undefined;
  switch (accountListQuery.status) {
    case "error":
    default:
      return <ApplicationErrorBanner />;
    case "pending":
      if (accountListQuery.fetchStatus === "paused") {
        return <OfflineBanner />;
      }
      return <InformationBanner title="Fetching accounts" />;
    case "success":
      account = accountListQuery.data.find((a) => a.userKey === accountKey);
      if (!account) {
        return (
          <ErrorBanner
            title="Not Found"
            message="Requested account was not found"
            buttonCaption="Select Account"
            to="/accounts/-/select"
          ></ErrorBanner>
        );
      }
      return <AccountSettingsForms account={account} />;
  }
}
