import { Project, SourceFile } from "ts-morph";
import { collectJsDocNodes } from "../merge.js"; // Note: .js extension for ESM

describe("collectJsDocNodes", () => {
  let project: Project;
  let file: SourceFile;

  beforeEach(() => {
    project = new Project();
  });

  it("should collect JSDoc from classes", () => {
    file = project.createSourceFile(
      "test.ts",
      `
      /** Class description */
      class TestClass {}
    `,
    );
    const nodes = collectJsDocNodes(file);
    expect(nodes).toHaveLength(1);
    if (nodes[0]) {
      expect(nodes[0].name).toBe("TestClass");
    }
  });

  it("should collect JSDoc from interfaces", () => {
    file = project.createSourceFile(
      "test.ts",
      `
      /** Interface description */
      interface TestInterface {}
    `,
    );
    const nodes = collectJsDocNodes(file);
    expect(nodes).toHaveLength(1);
    if (nodes[0]) {
      expect(nodes[0].name).toBe("TestInterface");
    }
  });

  it("should collect JSDoc from functions", () => {
    file = project.createSourceFile(
      "test.ts",
      `
      /** Function description */
      function testFunc() {}
    `,
    );
    const nodes = collectJsDocNodes(file);
    expect(nodes).toHaveLength(1);
    if (nodes[0]) {
      expect(nodes[0].name).toBe("testFunc");
    }
  });

  it("should collect JSDoc from type aliases", () => {
    file = project.createSourceFile(
      "test.ts",
      `
      /** Type description */
      type TestType = string;
    `,
    );
    const nodes = collectJsDocNodes(file);
    expect(nodes).toHaveLength(1);
    if (nodes[0]) {
      expect(nodes[0].name).toBe("TestType");
    }
  });

  it("should collect JSDoc from enums", () => {
    file = project.createSourceFile(
      "test.ts",
      `
      /** Enum description */
      enum TestEnum {}
    `,
    );
    const nodes = collectJsDocNodes(file);
    expect(nodes).toHaveLength(1);
    if (nodes[0]) {
      expect(nodes[0].name).toBe("TestEnum");
    }
  });

  it("should handle anonymous nodes", () => {
    file = project.createSourceFile(
      "test.ts",
      `
      /** Anonymous */
      export {};
    `,
    );
    const nodes = collectJsDocNodes(file);
    expect(nodes).toHaveLength(0); // Since name is empty after trim
  });

  it("should return empty array for no JSDoc", () => {
    file = project.createSourceFile(
      "test.ts",
      `
      class NoDoc {}
    `,
    );
    const nodes = collectJsDocNodes(file);
    expect(nodes).toHaveLength(0);
  });
});
