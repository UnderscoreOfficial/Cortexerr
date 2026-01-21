import globals from "globals";

import eslint from "@eslint/js";
import tseslint from "typescript-eslint";

import importPlugin from "eslint-plugin-import";

export default [
  eslint.configs.recommended,
  ...tseslint.configs.recommended,
  {
    languageOptions: {
      ecmaVersion: "latest",
      sourceType: "module",
      globals: {
        ...globals.commonjs,
        ...globals.es2021,
        ...globals.node,
      },
    },
  },
  {
    files: ["**/*.{js,ts}"],
    settings: {
      "import/resolver": {
        typescript: {},
      },
    },
  },
  {
    files: ["**/*.ts"],
    languageOptions: {
      parser: tseslint.parser,
    },
    plugins: {
      "@typescript-eslint": tseslint.plugin,
      import: importPlugin,
    },
    settings: {
      "import/internal-regex": "^~/",
      "import/resolver": {
        node: {
          extensions: [".ts"],
        },
        typescript: {
          alwaysTryTypes: true,
        },
      },
    },
    rules: {
      ...tseslint.configs.recommendedTypeChecked?.rules,
      ...importPlugin.configs.recommended.rules,
      ...importPlugin.configs.typescript.rules,
    },
  },
];
