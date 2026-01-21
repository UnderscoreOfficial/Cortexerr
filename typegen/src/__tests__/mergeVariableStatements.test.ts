import { Project } from "ts-morph";
import { mergeVariableStatements } from "../merge.js";

describe("mergeVariableStatements", () => {
  let project: Project;

  beforeEach(() => {
    project = new Project();
  });

  it("should copy JSDoc for variables if new has none", () => {
    const newFile = project.createSourceFile("new.ts", 'const test = "value";');
    const oldFile = project.createSourceFile(
      "old.ts",
      '/** Old var */ const test = "value";',
    );

    const merged = mergeVariableStatements(newFile, oldFile);

    expect(merged).toBe(1);
    const stmt = newFile.getVariableStatements()[0];
    if (stmt) {
      expect(stmt.getJsDocs()).toHaveLength(1);
    }
  });

  it("should handle leading comments as pseudo JSDoc", () => {
    const newFile = project.createSourceFile("new.ts", 'const test = "value";');
    const oldFile = project.createSourceFile(
      "old.ts",
      '// Leading comment\nconst test = "value";',
    );

    const merged = mergeVariableStatements(newFile, oldFile);

    expect(merged).toBe(1);
  });

  it("should not copy if new already has JSDoc", () => {
    const newFile = project.createSourceFile(
      "new.ts",
      '/** New var */ const test = "value";',
    );
    const oldFile = project.createSourceFile(
      "old.ts",
      '/** Old var */ const test = "value";',
    );

    const merged = mergeVariableStatements(newFile, oldFile);

    expect(merged).toBe(0);
    const stmt = newFile.getVariableStatements()[0];
    if (stmt) {
      const docs = stmt.getJsDocs();
      if (docs[0]) {
        expect(docs[0].getInnerText()).toBe("New var");
      }
    }
  });

  it("should return 0 if no matching variables", () => {
    const newFile = project.createSourceFile("new.ts", 'const test = "value";');
    const oldFile = project.createSourceFile(
      "old.ts",
      'const other = "value";',
    );

    const merged = mergeVariableStatements(newFile, oldFile);

    expect(merged).toBe(0);
  });
});
