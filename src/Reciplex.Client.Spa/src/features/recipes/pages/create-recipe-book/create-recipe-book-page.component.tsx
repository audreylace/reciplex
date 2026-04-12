import { NameAndShortDescriptionForm } from "../../components/name-and-short-description-form/name-and-short-description-form.component";
import { useCreateRecipeBookMutation } from "../../hooks/useCreateRecipeBookMutation.hook";
import { useNavigate } from "react-router";
import { makeViewRecipeBookPath } from "../../route-utils";

/**
 * Create recipe book page component
 */
export function CreateRecipeBookPage() {
  const { mutateAsync, reset, isError, isPending } =
    useCreateRecipeBookMutation();
  const navigate = useNavigate();

  return (
    <NameAndShortDescriptionForm
      legendText="Create New Recipe Book"
      nameLabel="Book Title"
      nameHelpText="Title of the recipe book"
      nameMaxLength={128}
      shortDescriptionHelpText="Concise description of the book's content or purpose"
      shortDescriptionLabel="Book Short Description"
      shortDescriptionMaxLength={256}
      onSuccess={async (data) => {
        const book = await mutateAsync(data);
        navigate(makeViewRecipeBookPath(book.id));
      }}
      onReset={reset}
      pending={isPending}
      showError={isError}
      submitText="Create Book"
    />
  );
}
