import { Project } from "ts-morph";
import { recordUnmatchedDocs, not_transferred_blocks } from "../merge.js";
import { readFileSync } from "fs";
import { join } from "path";

describe("recordUnmatchedDocs", () => {
  let project: Project;

  beforeEach(() => {
    project = new Project();
    // Clear the array before each test
    not_transferred_blocks.length = 0;
  });

  it("should record unmatched declarations", () => {
    const oldFile = project.createSourceFile(
      "old.ts",
      "/** Unmatched */ class OldClass {}",
    );
    const newFile = project.createSourceFile("new.ts", "class NewClass {}");

    recordUnmatchedDocs("test.d.ts", oldFile, newFile);

    expect(not_transferred_blocks.length).toBeGreaterThan(0);
  });

  // Note: Full testing of recordUnmatchedDocs is complex due to global state and file I/O.
  // In a real scenario, refactor to make it more testable by injecting dependencies.
});
