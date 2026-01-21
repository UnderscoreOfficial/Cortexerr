import { mergeDocsForFile } from "../merge.js";
import { existsSync } from "fs";
import { join } from "path";

// Integration test - requires temp files, so limited
describe("mergeDocsForFile integration", () => {
  it("should process files without errors", () => {
    // This would need temp files; for now, just check it doesn't throw on non-existent
    // Note: mergeDocsForFile will log "No docs file for: nonexistent.d.ts"
    expect(() => mergeDocsForFile("nonexistent.d.ts")).not.toThrow();
  });
});
