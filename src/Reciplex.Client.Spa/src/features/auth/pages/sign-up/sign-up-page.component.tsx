import { ChallengeGuard } from "../../components/challenge-guard/challenge-guard.component";
import { SignUpForm } from "../../components/sign-up-form/sign-up-form.component";

/** page for making an account */
export function SignUpPage() {
  return (
    <ChallengeGuard>
      <SignUpForm />
    </ChallengeGuard>
  );
}
