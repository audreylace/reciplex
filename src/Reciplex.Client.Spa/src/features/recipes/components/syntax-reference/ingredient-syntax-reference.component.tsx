import { QuotedValue } from "./quoted-value.component";
import { SyntaxExample } from "./syntax-example.component";
import { SyntaxOverviewText } from "./syntax-overview-text.component";
import { SyntaxSection } from "./syntax-section.component";

export function IngredientSyntaxReference() {
  return (
    <SyntaxSection title="Ingredient Syntax Guide">
      <SyntaxOverviewText>
        Use ingredient syntax to define the ingredients used in the recipe.
        Ingredients with the same name and unit are combined and the quantities
        are summed up.
      </SyntaxOverviewText>
      <SyntaxExample
        output="1/2 cups of milk"
        description={
          <>
            Defines an ingredient with an amount, unit, and quantity. Here we
            define 1/2 cups of milk as an ingredient. Unit can be left unquoted
            if it does not have any spaces and is alphanumeric.
          </>
        }
      >
        (( i 1/2 <QuotedValue value="cup" /> <QuotedValue value="milk" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="2 oranges"
        description={
          <>
            Setting the unit to <QuotedValue value="-" /> defines an ingredient
            with an amount, and quantity only. In the example above, this
            expression includes 2 oranges in the ingredient list.
          </>
        }
      >
        (( i 2 <QuotedValue value="-" /> <QuotedValue value="orange" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="soy sauce"
        description={
          <>
            Defines an ingredient with just name. Quantity or amount are left
            unspecified. Common use for this is calling out a potential topping
            to add to the recipe. The recipe might say to season rice with soy
            sauce to desired taste.
          </>
        }
      >
        (( i <QuotedValue value="soy sauce" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="mustard"
        description={
          <>
            The display text for any ingredient may be customized by providing
            the custom text as the last argument. This customization only
            impacts the display inside the recipe. It does not impact what shows
            up in the ingredient list. The demonstration adds 1/2 cup of milk to
            the recipe but inline text will render as mustard.
          </>
        }
      >
        (( i 1/2 <QuotedValue value="cup" /> <QuotedValue value="milk" />{" "}
        <QuotedValue value="mustard" /> ))
      </SyntaxExample>

      <SyntaxExample
        output="1/2 tbsp of soy sauce"
        description={
          <>
            Ingredients can be defined with an identifier to allow fast linking
            later. This is accomplished by providing the identifier for the
            ingredient as the first parameter. The name must not be in quotes
            and contain no spaces and start with a letter.
          </>
        }
      >
        (( i SauceForRice 0.5 tbsp <QuotedValue value="soy sauce" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="0.5 tbsp of soy sauce"
        description={
          <>
            Ingredients with identifiers can then be quickly referenced later in
            the text.
          </>
        }
      >
        (( i SauceForRice ))
      </SyntaxExample>
      <SyntaxExample
        output={'special "soy" sauce'}
        description={
          <>
            Quotes can be included in an ingredient name or text by typing a
            double quote.
          </>
        }
      >
        (( i <QuotedValue value={"I can't believe its not soy sauce"} />{" "}
        <QuotedValue value={'special ""soy"" sauce'} /> ))
      </SyntaxExample>
    </SyntaxSection>
  );
}
