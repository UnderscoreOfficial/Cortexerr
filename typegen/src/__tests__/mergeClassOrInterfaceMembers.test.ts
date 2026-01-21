import { Project } from "ts-morph";
import { mergeClassOrInterfaceMembers } from "../merge.js";

describe("mergeClassOrInterfaceMembers", () => {
  let project: Project;

  beforeEach(() => {
    project = new Project();
  });

  it("should copy JSDoc for methods if new has none", () => {
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
      const merged = mergeClassOrInterfaceMembers(newDecl, oldDecl);

      expect(merged).toBe(1);
      const method = newDecl.getMethods()[0];
      if (method) {
        expect(method.getFullText()).toContain("Old method");
      }
    }
  });

  it("should copy JSDoc for properties if new has none", () => {
    const newFile = project.createSourceFile(
      "new.ts",
      "class Test { prop: string; }",
    );
    const oldFile = project.createSourceFile(
      "old.ts",
      "class Test {\n  /** Old prop */\n  prop: string; }",
    );

    const newDecl = newFile.getClasses()[0];
    const oldDecl = oldFile.getClasses()[0];

    if (newDecl && oldDecl) {
      const merged = mergeClassOrInterfaceMembers(newDecl, oldDecl);

      expect(merged).toBe(1);
      const property = newDecl.getProperties()[0];
      if (property) {
        expect(property.getFullText()).toContain("Old prop");
      }
    }
  });

  it("should not copy if new already has JSDoc", () => {
    const newFile = project.createSourceFile(
      "new.ts",
      "class Test { /** New method */ method() {} }",
    );
    const oldFile = project.createSourceFile(
      "old.ts",
      "class Test { /** Old method */ method() {} }",
    );

    const newDecl = newFile.getClasses()[0];
    const oldDecl = oldFile.getClasses()[0];

    if (newDecl && oldDecl) {
      const merged = mergeClassOrInterfaceMembers(newDecl, oldDecl);

      expect(merged).toBe(0);
      const methodDocs = newDecl.getMethods()[0]?.getJsDocs();
      if (methodDocs && methodDocs[0]) {
        expect(methodDocs[0].getInnerText()).toBe("New method");
      }
    }
  });

  it("should work for interfaces", () => {
    const newFile = project.createSourceFile(
      "new.ts",
      "interface Test { method(): void; }",
    );
    const oldFile = project.createSourceFile(
      "old.ts",
      "interface Test {\n  /** Old method */\n  method(): void; }",
    );

    const newDecl = newFile.getInterfaces()[0];
    const oldDecl = oldFile.getInterfaces()[0];

    if (newDecl && oldDecl) {
      const merged = mergeClassOrInterfaceMembers(newDecl, oldDecl);

      expect(merged).toBe(1);
    }
  });
});
