import { Project } from "ts-morph";
import { mergeDeclarationDocs } from "../merge.js";

describe("mergeDeclarationDocs", () => {
  let project: Project;

  beforeEach(() => {
    project = new Project();
  });

  it("should copy JSDoc from old to new class if new has none", () => {
    const newFile = project.createSourceFile("new.ts", "class Test {}");
    const oldFile = project.createSourceFile(
      "old.ts",
      "/** Old description */ class Test {}",
    );

    const newDecl = newFile.getClasses()[0];
    const oldDecl = oldFile.getClasses()[0];

    if (newDecl && oldDecl) {
      const merged = mergeDeclarationDocs("class", newDecl, oldDecl);

      expect(merged).toBe(1);
      expect(newDecl.getJsDocs()).toHaveLength(1);
      const docs = newDecl.getJsDocs();
      if (docs[0]) {
        expect(docs[0].getInnerText()).toBe("Old description");
      }
    }
  });

  it("should not copy if new already has JSDoc", () => {
    const newFile = project.createSourceFile(
      "new.ts",
      "/** New description */ class Test {}",
    );
    const oldFile = project.createSourceFile(
      "old.ts",
      "/** Old description */ class Test {}",
    );

    const newDecl = newFile.getClasses()[0];
    const oldDecl = oldFile.getClasses()[0];

    if (newDecl && oldDecl) {
      const merged = mergeDeclarationDocs("class", newDecl, oldDecl);

      expect(merged).toBe(0);
      const docs = newDecl.getJsDocs();
      if (docs[0]) {
        expect(docs[0].getInnerText()).toBe("New description");
      }
    }
  });

  it("should merge members for classes and interfaces", () => {
    const newFile = project.createSourceFile(
      "new.ts",
      "class Test { method() {} }",
    );
    const oldFile = project.createSourceFile(
      "old.ts",
      "class Test {\n  /** Old method */\n  method() {} }",
    );

    const newDecl = newFile.getClasses()[0];
    const oldDecl = oldFile.getClasses()[0];

    if (newDecl && oldDecl) {
      const merged = mergeDeclarationDocs("class", newDecl, oldDecl);

      expect(merged).toBe(1);
      const method = newDecl.getMethods()[0];
      if (method) {
        expect(method.getFullText()).toContain("Old method");
      }
    }
  });

  it("should return 0 if no docs to copy", () => {
    const newFile = project.createSourceFile("new.ts", "class Test {}");
    const oldFile = project.createSourceFile("old.ts", "class Test {}");

    const newDecl = newFile.getClasses()[0];
    const oldDecl = oldFile.getClasses()[0];

    if (newDecl && oldDecl) {
      const merged = mergeDeclarationDocs("class", newDecl, oldDecl);

      expect(merged).toBe(0);
    }
  });
});
