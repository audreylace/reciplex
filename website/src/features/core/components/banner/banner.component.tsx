import type { PropsWithChildren } from "preact/compat";
import bannerStylesModule from "./banner.module.css";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { useNavigate } from "react-router";
import type { ComponentChildren } from "preact";

/**
 * Reusable banner component
 */
export function Banner({
  children,
  title,
  message,
  icon,
  type,
  className,
  to,
  onButtonClick,
  buttonCaption,
}: PropsWithChildren<BannerProps>) {
  const navigate = useNavigate();
  const clickHandler = () => {
    if (onButtonClick) {
      onButtonClick();
    }

    if (to) {
      navigate(to);
    }
  };

  return (
    <div
      className={`${bannerStylesModule.root} ${className}`}
      data-banner-type={type ?? "default"}
    >
      <div className={bannerStylesModule.icon}>
        <i className={icon ?? "bi bi-fork-knife"}></i>
      </div>
      <div>
        <h1 className={bannerStylesModule.title}>{title}</h1>
        {message && <p className={bannerStylesModule.message}>{message}</p>}
        {children}
        {buttonCaption && (
          <SuccessButton onClick={clickHandler}>{buttonCaption}</SuccessButton>
        )}
      </div>
    </div>
  );
}

export interface BannerProps {
  title: string;
  message?: string;
  icon?: string;
  type?: "error" | "default" | "warning" | "information" | "success";
  className?: string;
  /** the path navigate to when clicking the button */
  to?: string;
  /** the caption of the button to add to the bottom of the banner */
  buttonCaption?: ComponentChildren;
  /** invoked on button click. Overrides `to`. */
  onButtonClick?: () => void;
}

/**
 * Reusable error banner component
 */
export function ErrorBanner(props: PropsWithChildren<BannerProps>) {
  return <Banner icon="bi bi-exclamation-diamond" type="error" {...props} />;
}

/**
 * Reusable error banner component
 */
export function WarningBanner(props: PropsWithChildren<BannerProps>) {
  return <Banner icon="bi bi-exclamation-triangle" type="warning" {...props} />;
}

/**
 * Reusable information banner
 */
export function InformationBanner(props: PropsWithChildren<BannerProps>) {
  return <Banner icon="bi bi-info-circle" type="information" {...props} />;
}

export function SuccessBanner(props: PropsWithChildren<BannerProps>) {
  return <Banner icon="bi bi-check2-circle" type="success" {...props} />;
}
