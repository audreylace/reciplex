import { QuotedValue } from "./quoted-value.component";
import { SyntaxExample } from "./syntax-example.component";
import { SyntaxOverviewText } from "./syntax-overview-text.component";
import { SyntaxSection } from "./syntax-section.component";

export function ToolSyntaxReference() {
  return (
    <SyntaxSection title="Tools Syntax Guide">
      <SyntaxOverviewText>
        Use tools syntax to define the tools and other equipment used in the
        recipe. The syntax comes in two variants: `t` is used to just specify a
        tool name, size, and unit, `tQ` works the same as `t` but takes the
        amount of the tool as the first argument. Use `tQ` when multiple of a
        tool needs to be included. Eg, the recipe calls for using 6 spoons.
        Tools with the same name unit and size are combined and the quantities
        are summed up.
      </SyntaxOverviewText>
      <SyntaxExample
        output="3 cup pot"
        description={<>Defines a tools with a unit and size.</>}
      >
        (( t 3 <QuotedValue value="cup" /> <QuotedValue value="pot" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="blender"
        description={<>Defines a tool by name only.</>}
      >
        (( t <QuotedValue value="blender" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="large blender"
        description={
          <>Changes the text that is shown in the recipe for a tool. </>
        }
      >
        (( t <QuotedValue value="blender" />{" "}
        <QuotedValue value="large blender" />
        ))
      </SyntaxExample>
      <SyntaxExample
        output="6 one cup pots"
        description={
          <>
            Defines a tool by name, unit and size. Includes an additional
            quantity.
          </>
        }
      >
        (( tQ 6 1 <QuotedValue value="cup" /> <QuotedValue value="pot" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="6 spoons"
        description={<>Defines a tool by name and specifies the quantity.</>}
      >
        (( tQ 6 <QuotedValue value="spoons" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="6 spoons"
        description={
          <>
            Tools can be defined with an identifier to allow fast linking later.
            This is accomplished by providing the identifier for the tool as the
            first parameter. The name must not be in quotes and contain no
            spaces and start with a letter.
          </>
        }
      >
        (( tQ Spoons 6 <QuotedValue value="spoons" /> ))
      </SyntaxExample>
      <SyntaxExample
        output="6 spoons"
        description={
          <>
            Tools with identifiers can then be quickly referenced later in the
            text.
          </>
        }
      >
        (( tQ Spoons ))
      </SyntaxExample>
      <SyntaxExample
        output={'the "tool" sauce'}
        description={
          <>
            Quotes can be included in an tool name or text by typing a double
            quote.
          </>
        }
      >
        (( t <QuotedValue value={"the tool name"} />{" "}
        <QuotedValue value={'the ""tool"" sauce'} /> ))
      </SyntaxExample>
    </SyntaxSection>
  );
}
