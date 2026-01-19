import MDEditor from "@uiw/react-md-editor";
import "./app.css";
import { useState } from "preact/hooks";
import Markdown from "react-markdown";
import rehypeSanitize from "rehype-sanitize";
import { visit } from "unist-util-visit";

export function App() {
  return (
    <>
      <ReadOnlyRecipePage model={fakeModel} />
      <Editor />
    </>
  );
}

function ReadOnlyRecipePage({ model }: { model: RecipeDocumentModel }) {
  return (
    <>
      <h1>{model.name}</h1>
      {model.ingredients && model.ingredients.length > 0 && (
        <>
          <h2>Recipe Ingredients</h2>
          <ul>
            {model.ingredients.map((ing) =>
              ing.ingredientType === "freeform-value" ? (
                <li>
                  {ing.label} {ing.optional && <span>optional</span>}
                </li>
              ) : (
                <>
                  <li>
                    {ing.label} : {ing.quantity}
                    {ing.unit} {ing.optional && <span>optional</span>}
                  </li>
                </>
              )
            )}
          </ul>
        </>
      )}
      {Object.keys(model.sections).map((key) => {
        const section = model.sections[key];
        return (
          <>
            {section.title && <h2>{section.title}</h2>}
            <p>{section.body}</p>
          </>
        );
      })}
      {model.ingredients.map((i) => (
        <>
          <IngredientField ingredient={i} />
          <br /> <br />
        </>
      ))}
    </>
  );
}

function Editor() {
  const [value, setValue] = useState<string | undefined>(
    "*[rice](@ingredient 2)"
  );
  return (
    <>
      <div className="container">
        <MDEditor
          value={value}
          onChange={(v) => setValue(v)}
          previewOptions={{
            rehypePlugins: [[rehypeSanitize]],
            remarkPlugins: [[convertAtSyntax]],
          }}
        />
      </div>
      <Markdown
        rehypePlugins={[rehypeSanitize]}
        remarkPlugins={[convertAtSyntax]}
        components={{
          code(props) {
            return <code {...props} />;
          },
        }}
      >
        {value}
      </Markdown>
    </>
  );
}

function IngredientField({ ingredient }: { ingredient: IngredientTypeUnion }) {
  switch (ingredient.ingredientType) {
    case "discrete-quantity":
      return (
        <>
          <div>
            <label>Ingredient Name</label>
            <br />
            <input type="text" value={ingredient.label} />
          </div>
          <div>
            <label>Quantity</label>
            <br />
            <input type="number" value={ingredient.quantity} />
          </div>
          <div>
            <label>Unit</label>
            <br />
            <input type="text" value={ingredient.unit} />
          </div>
          <div>
            <label>Link</label>
            <br />
            <input type="text" value={ingredient.namedLink ?? ""} />
          </div>
          <div>
            <label>Optional</label>
            <input type="checkbox" checked={ingredient.optional ?? false} />
          </div>
        </>
      );
    case "freeform-value":
      return (
        <>
          <div>
            <label>Ingredient</label>
            <br />
            <input type="text" value={ingredient.label} />
          </div>
          <div>
            <label>Link</label>
            <br />
            <input type="text" value={ingredient.namedLink ?? ""} />
          </div>
          <div>
            <label>Optional</label>
            <input type="checkbox" checked={ingredient.optional ?? false} />
          </div>
        </>
      );
    default:
      return null;
  }
}

/**
 * Constants defining the type of an ingredient
 */
type IngredientTypeUnionTag = "discrete-quantity" | "freeform-value";

/**
 * Base interface for all models in `IngredientTypeUnion`
 */
interface PartOfIngredientTypeUnion {
  /** tag identifying the type in the `IngredientTypeUnion`*/
  ingredientType: IngredientTypeUnionTag;
  /** id of the section this ingredient belongs to  */
  recipeSectionId?: string;
}

/**
 * Strongly typed version of `PartOfIngredientTypeUnion`
 * @template T value for `ingredientType`
 */
interface PartOfIngredientTypeUnionWithTag<T extends IngredientTypeUnionTag>
  extends PartOfIngredientTypeUnion {
  /** @inheritdoc */
  ingredientType: T;
  /** user supplied named link */
  namedLink?: string;
  /** if this ingredient is optional */
  optional?: boolean;
  /** ingredient specific notes */
  notes?: string;
}

/**
 * Ingredient with a label, quantity, and unit. Example: `1 cup of banking soda`
 * rendered as `{quantity: 1, unit: cup, label: banking soda}`
 */
interface DiscreteQuantityIngredient
  extends PartOfIngredientTypeUnionWithTag<"discrete-quantity"> {
  /** quantity of the ingredient */
  quantity: number;
  /** the ingredient label */
  label: string;
  /** the unit of the ingredient */
  unit: string;
}

/**
 * Ingredient is just free form text. Example: `bottle of soy sauce`
 * rendered as `{label: bottle of soy sauce}`
 */
interface FreeFormValueIngredient
  extends PartOfIngredientTypeUnionWithTag<"freeform-value"> {
  /** the ingredient label */
  label: string;
}

/**
 * Union of ingredient models
 */
type IngredientTypeUnion = DiscreteQuantityIngredient | FreeFormValueIngredient;

/** Document model for a recipe */
interface RecipeDocumentModel {
  /** name of the document */
  name: string;
  /** unique document ID */
  id: string;
  /** id of the recipe book */
  bookId: string;
  /** full set of ingredients for this document */
  ingredients: IngredientTypeUnion[];
  /** set of sections */
  sections: Record<string, RecipeSection>;
}

/** section in a recipe */
interface RecipeSection {
  /** title of the section */
  title: string;
  /** mark down body */
  body: string;
  /** user supplied named link in the markdown body */
  namedLink?: string;
}

const fakeModel: RecipeDocumentModel = {
  name: "Plain White Rice",
  id: "hCb7gHjfs78jHfs==",
  bookId: "fhkfljl_flfjlsf-=",
  sections: {
    abg1234: {
      namedLink: "fluffy rice",
      title: "White Rice",
      body: "This fluffy rice will go great with the soy tofu. \r\n \
      ## Directions \r\n \
      * Pour [rice](@ingredient $2) and <@ingredient water> into <@ingredient rice> a -${quantity 3,'cup','pot'}. \r\n \
      * Stir until the rice and water are evenly mixed together \
      * Put the pot on the stove top and bring to boil stirring occasionally \r\n \
      * Boil the rice in the pot for 10 minutes \r\n \
      * Drain the water out of the rice \r\n \
      * Add the (tofu)[@section tofu] on top \r\n \
      ",
    },
    gh123: {
      title: "Soy Tofu",
      body: " \r\n \
      ## Directions \r\n \
      ",
      namedLink: "tofu",
    },
  },
  ingredients: [
    {
      ingredientType: "freeform-value",
      label: "bottle of soy sauce",
      optional: true,
      recipeSectionId: "gh123",
    },
    {
      ingredientType: "discrete-quantity",
      label: "rice",
      unit: "cup",
      quantity: 1,
      namedLink: "rice",
      recipeSectionId: "abg1234",
    },
    {
      ingredientType: "discrete-quantity",
      label: "water",
      unit: "cup",
      quantity: 2,
      namedLink: "water",
      recipeSectionId: "abg1234",
    },
    {
      ingredientType: "discrete-quantity",
      label: "tofu",
      unit: "gram",
      namedLink: "tofu",
      quantity: 100,
      recipeSectionId: "gh123",
    },
    {
      ingredientType: "freeform-value",
      label: "bottle of soy sauce",
      optional: true,
      recipeSectionId: "gh123",
    },
  ],
};

// /**
//  * A unit of measurement recognized by the application
//  */
// interface UnitOfMeasure {
//   /** the name */
//   visibleName: string;
//   /** single version of the unit */
//   single: string;
//   /** plural version of the unit */
//   plural: string;
//   /** application wide unique string. Used for matching by other parts of the application */
//   key: string;
//   /** strings that match this unit. Case insensitive by default. Pass an array with second value set to true to make case sensitive. */
//   matches?: (string | [string, boolean])[];
// }

/** Application units of measurements  */
// const UnitsOfMeasurements: UnitOfMeasure[] = [
//   {
//     visibleName: "USA cups",
//     key: "USA_CUP",
//     single: "cup",
//     plural: "cups",
//     matches: ["cup", "cups"],
//   },
//   {
//     visibleName: "USA ounces",
//     key: "USA_OZ",
//     single: "oz",
//     plural: "oz",
//     matches: ["ounce", "ounces", "oz", "℥"],
//   },
// ];

function convertAtSyntax() {
  return function (tree) {
    visit(
      tree,
      "inlineCode",
      function (
        node: { type: any; value: string | undefined | null },
        index,
        parent
      ) {
        if (typeof node.value !== "string") {
          return true;
        }

        if (
          !node.value.startsWith("{{") ||
          !node.value.endsWith("}}") ||
          node.value.length <= 4
        ) {
          return true;
        }

        const SECTION = "{{section ";
        if (node.value.startsWith(SECTION)) {
          const key = node.value.slice(SECTION.length, node.value.length - 2);
          const sectionK = Object.keys(fakeModel.sections).find(
            (k) => fakeModel.sections[k].namedLink === key
          );

          if (sectionK) {
            const section = fakeModel.sections[sectionK];
            node.value = `@${sectionK}, ${section.title} link___${
              section.namedLink ?? "no link"
            }`;
          } else {
            node.value = `*Unable to Resolve Section link (${key})`;
          }
          node.type = "s";
        }

        return true;
      }
    );
  };
}
