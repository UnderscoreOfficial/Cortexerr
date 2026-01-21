/**
 * Merges JSDoc comments from documentation files into type declaration files.
 *
 * This script processes .d.ts files in the dist/ directory, copying JSDoc from
 * corresponding files in docs/, and outputs merged files to merged/.
 * Unmatched JSDoc blocks are saved to not-transferred/unmatched-docs.txt.
 */
import { globSync } from "glob";
import { join, dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import {
  Project,
  InterfaceDeclaration,
  ClassDeclaration,
  FunctionDeclaration,
  TypeAliasDeclaration,
  EnumDeclaration,
  Node,
  SourceFile,
  JSDoc,
} from "ts-morph";
import {
  mkdirSync,
  writeFileSync,
  existsSync,
  rmSync,
  copyFileSync,
  cpSync,
} from "node:fs";

// setup -----------------------------------------------------------------------

const __dirname = dirname(fileURLToPath(import.meta.url));
const TYPES_DIR = resolve(__dirname, "..");
const DIST_ROOT = resolve(TYPES_DIR, "dist");
const DOCS_ROOT = resolve(TYPES_DIR, "docs");
const MERGED_ROOT = resolve(TYPES_DIR, "merged");
const NOT_TRANSFERRED_ROOT = resolve(TYPES_DIR, "not-transferred");
const NOT_TRANSFERRED_FILE = join(NOT_TRANSFERRED_ROOT, "unmatched-docs.ts");

// types -----------------------------------------------------------------------

type NamedDeclaration =
  | InterfaceDeclaration
  | ClassDeclaration
  | FunctionDeclaration
  | TypeAliasDeclaration
  | EnumDeclaration;

/**
 * Type guard to check if a Node is a NamedDeclaration.
 */
function isNamedDeclaration(node: Node): node is NamedDeclaration {
  return (
    node instanceof ClassDeclaration ||
    node instanceof InterfaceDeclaration ||
    node instanceof FunctionDeclaration ||
    node instanceof TypeAliasDeclaration ||
    node instanceof EnumDeclaration
  );
}

interface HasName {
  getName(): string | undefined;
}

interface HasJsDoc {
  getJsDocs(): JSDoc[];
  addJsDoc(doc: { description: string }): void;
}

type MergableNode = Node & Partial<HasName> & Partial<HasJsDoc>;

// state -----------------------------------------------------------------------

let file_count = 0;
let merged_count = 0;
export const not_transferred_blocks: string[] = [];

const project = new Project({
  tsConfigFilePath: resolve(TYPES_DIR, "tsconfig.json"),
});

// utils -----------------------------------------------------------------------

export function prepareOutputDirectories(): void {
  for (const dir of [MERGED_ROOT, NOT_TRANSFERRED_ROOT]) {
    if (existsSync(dir)) rmSync(dir, { recursive: true, force: true });
    mkdirSync(dir, { recursive: true });
  }
}

export function collectJsDocNodes(
  file: SourceFile,
): { name: string; text: string }[] {
  const nodes: { name: string; text: string }[] = [];

  for (const node of file.getDescendants()) {
    const candidate = node as unknown as MergableNode;
    if (typeof candidate.getJsDocs == "function") {
      const docs = candidate.getJsDocs();
      if (docs.length > 0 && docs[0]) {
        const name = candidate.getName?.() ?? "(anonymous)";
        if (name.trim().length > 0) {
          nodes.push({ name, text: node.getText() });
        }
      }
    }
  }
  return nodes;
}

// core merging ----------------------------------------------------------------

/**
 * Merges JSDoc from old declaration to new if new lacks docs.
 * Also merges member docs for classes/interfaces.
 */
export function mergeDeclarationDocs(
  kind: string,
  new_decl: NamedDeclaration,
  old_decl: NamedDeclaration,
): number {
  let merged_in_decl = 0;

  const old_docs = old_decl.getJsDocs();
  const new_docs = new_decl.getJsDocs();

  if (old_docs.length > 0 && old_docs[0] && new_docs.length == 0) {
    new_decl.addJsDoc({ description: old_docs[0].getInnerText() });
    merged_in_decl++;
    console.log(`  Copied docs -> ${kind} ${new_decl.getName()}`);
  }

  if (
    (new_decl instanceof ClassDeclaration &&
      old_decl instanceof ClassDeclaration) ||
    (new_decl instanceof InterfaceDeclaration &&
      old_decl instanceof InterfaceDeclaration)
  ) {
    merged_in_decl += mergeClassOrInterfaceMembers(new_decl, old_decl);
  }

  return merged_in_decl;
}

/**
 * Merges JSDoc for methods and properties in classes/interfaces.
 */
export function mergeClassOrInterfaceMembers(
  new_decl: ClassDeclaration | InterfaceDeclaration,
  old_decl: ClassDeclaration | InterfaceDeclaration,
): number {
  let merged_in_members = 0;

  const mergeMembers = (getMembers: () => Node[], label: string): void => {
    const new_members = getMembers.call(new_decl) as MergableNode[];
    const old_members = getMembers.call(old_decl) as MergableNode[];

    for (const new_member of new_members) {
      const name = new_member.getName?.();
      const old_member = old_members.find((m) => m.getName?.() == name) as
        | MergableNode
        | undefined;
      if (!old_member) continue;

      const old_docs = old_member.getJsDocs?.() ?? [];
      const new_docs = new_member.getJsDocs?.() ?? [];

      if (
        old_docs.length > 0 &&
        old_docs[0] &&
        new_docs.length == 0 &&
        new_member.addJsDoc
      ) {
        new_member.addJsDoc({ description: old_docs[0].getInnerText() });
        merged_in_members++;
        console.log(`  Copied JSDoc for ${label} ${name}`);
      }
    }
  };

  mergeMembers(new_decl.getMethods, "method");
  mergeMembers(new_decl.getProperties, "property");

  return merged_in_members;
}

/**
 * Merges JSDoc for variable statements.
 */
export function mergeVariableStatements(
  new_file: SourceFile,
  old_file: SourceFile,
): number {
  let merged_vars = 0;
  const new_stmts = new_file.getVariableStatements();
  const old_stmts = old_file.getVariableStatements();

  for (const new_stmt of new_stmts) {
    const declarations = new_stmt.getDeclarationList().getDeclarations();
    if (declarations.length === 0) continue;
    const first_decl = declarations[0]!;
    const name = first_decl.getName();
    if (!name) continue;

    const old_stmt = old_stmts.find(
      (s) => s.getDeclarationList().getDeclarations()[0]?.getName() == name,
    );
    if (!old_stmt) continue;

    let old_docs = old_stmt.getJsDocs();
    const stmt_comments = old_stmt.getLeadingCommentRanges();
    if (stmt_comments.length > 0) {
      const decl_comments = stmt_comments.map((r: { getText: () => string }) =>
        r.getText(),
      );
      if (old_docs.length == 0) {
        const pseudo_doc: JSDoc = {
          getInnerText: () => decl_comments.join("\n"),
        } as unknown as JSDoc;
        old_docs = [pseudo_doc];
      }
    }

    if (
      old_docs.length > 0 &&
      old_docs[0] &&
      new_stmt.getJsDocs().length == 0
    ) {
      new_stmt.addJsDoc({ description: old_docs[0].getInnerText() });
      merged_vars++;
      console.log(`  Copied docs -> var-stmt ${name}`);
    }
  }

  return merged_vars;
}

// unmatched handling ----------------------------------------------------------

/**
 * Records JSDoc blocks from old file that couldn't be matched to new declarations.
 */
export function recordUnmatchedDocs(
  rel_path: string,
  old_file: SourceFile,
  new_file: SourceFile,
): void {
  const old_jsdocs = collectJsDocNodes(old_file);
  const new_jsdocs = collectJsDocNodes(new_file);
  const new_names = new Set(new_jsdocs.map((n) => n.name));
  const unmatched_nodes = old_jsdocs.filter((n) => !new_names.has(n.name));

  if (unmatched_nodes.length == 0) return;

  const emitted = new Set<Node>();
  not_transferred_blocks.push(`// === ${rel_path} ===`);

  for (const item of unmatched_nodes) {
    const matching_node = old_file
      .getDescendants()
      .find((n) => n.getText() == item.text);
    let block: Node | undefined = matching_node;

    while (
      block &&
      !(
        block instanceof ClassDeclaration ||
        block instanceof InterfaceDeclaration ||
        block instanceof FunctionDeclaration ||
        block instanceof TypeAliasDeclaration ||
        block instanceof EnumDeclaration
      )
    ) {
      block = block.getParent();
    }

    if (!block) continue;
    if (!isNamedDeclaration(block)) continue;
    let emit_target = block as NamedDeclaration;
    const parent = emit_target?.getParent();

    if (
      emit_target &&
      parent &&
      "getKindName" in parent &&
      parent.getKindName() == "VariableStatement"
    ) {
      emit_target = parent as unknown as NamedDeclaration;
    }

    if (emit_target && !emitted.has(emit_target)) {
      not_transferred_blocks.push(emit_target.getFullText());
      emitted.add(emit_target);
    }
  }

  console.log(`  ${unmatched_nodes.length} unmatched JSDoc block(s)`);
}

// per-file merge ----------------------------------------------------------------------

/**
 * Processes a single file pair for merging docs.
 */
/**
 * Processes declarations for a file.
 */
export function processDeclarations(
  new_file: SourceFile,
  old_file: SourceFile,
): number {
  const declaration_groups: [string, NamedDeclaration[], NamedDeclaration[]][] =
    [
      ["interface", new_file.getInterfaces(), old_file.getInterfaces()],
      ["class", new_file.getClasses(), old_file.getClasses()],
      ["function", new_file.getFunctions(), old_file.getFunctions()],
      ["type", new_file.getTypeAliases(), old_file.getTypeAliases()],
      ["enum", new_file.getEnums(), old_file.getEnums()],
    ];

  let merged = 0;
  for (const [kind, new_decls, old_decls] of declaration_groups) {
    for (const new_decl of new_decls) {
      const name = new_decl.getName();
      if (!name) continue;

      const old_decl = old_decls.find((d) => d.getName() == name);
      if (!old_decl) continue;

      merged += mergeDeclarationDocs(kind, new_decl, old_decl);
    }
  }
  return merged;
}

/**
 * Processes variables for a file.
 */
export function processVariables(
  new_file: SourceFile,
  old_file: SourceFile,
): number {
  return mergeVariableStatements(new_file, old_file);
}

/**
 * Logs results for a file.
 */
function logResults(rel_path: string, merged_in_file: number): void {
  console.log(`Merging: ${rel_path}`);
  if (merged_in_file > 0) {
    console.log(`  Added ${merged_in_file} TSDoc block(s)\n`);
  } else {
    console.log(`  No matching declarations with docs found\n`);
  }
}

/**
 * Updates global stats.
 */
function updateStats(merged: number): void {
  merged_count += merged;
}

export function mergeDocsForFile(rel_path: string): void {
  const dist_file = join(DIST_ROOT, rel_path);
  const docs_file = join(DOCS_ROOT, rel_path);
  const out_file = join(MERGED_ROOT, rel_path);

  mkdirSync(dirname(out_file), { recursive: true });

  if (!existsSync(dist_file)) {
    console.log(`No dist file for: ${rel_path}`);
    return;
  }

  if (!existsSync(docs_file)) {
    copyFileSync(dist_file, out_file);
    console.log(`No docs file for: ${rel_path}`);
    return;
  }

  file_count++;

  const new_file = project.addSourceFileAtPath(dist_file);
  const old_file = project.addSourceFileAtPath(docs_file);

  let merged_in_file = processDeclarations(new_file, old_file);
  merged_in_file += processVariables(new_file, old_file);
  recordUnmatchedDocs(rel_path, old_file, new_file);
  writeFileSync(out_file, new_file.getFullText());

  updateStats(merged_in_file);
  logResults(rel_path, merged_in_file);
}

// main ------------------------------------------------------------------------

/**
 * Main entry point: sets up directories and processes all .d.ts files.
 */
function main(): void {
  try {
    prepareOutputDirectories();

    for (const rel_path of globSync("**/*.d.ts", { cwd: DIST_ROOT })) {
      mergeDocsForFile(rel_path);
    }

    if (not_transferred_blocks.length > 0) {
      writeFileSync(NOT_TRANSFERRED_FILE, not_transferred_blocks.join("\n\n"));
      console.log(
        `Wrote ${not_transferred_blocks.length} unmatched JSDoc blocks → ${NOT_TRANSFERRED_FILE}`,
      );
    } else {
      console.log("No unmatched JSDoc blocks detected.");
    }

    console.log(
      `\nSummary: processed ${file_count} file(s), copied ${merged_count} doc block(s).\n`,
    );

    // Copy merged contents to type packages
    try {
      const tempPaths = [
        join(TYPES_DIR, "../core/merged/"),
        join(TYPES_DIR, "../partial/merged/"),
        join(TYPES_DIR, "../additive/merged/"),
      ];
      for (const tempPath of tempPaths) {
        if (existsSync(tempPath))
          rmSync(tempPath, { recursive: true, force: true });
        mkdirSync(tempPath, { recursive: true });
        cpSync(MERGED_ROOT, tempPath, { recursive: true });
        console.log(`Copied merged contents to: ${tempPath}`);
      }
    } catch (copyError) {
      console.error("Error copying merged contents:", copyError);
    }
  } catch (error) {
    console.error("Error during merging:", error);
    process.exit(1);
  }
}

main();
