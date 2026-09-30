# AGENTS.md

## Project
Unity 6000.3.21f1 project.
Keep implementations simple and maintainable.

## Working Rules
- Do not scan the entire project unless explicitly requested.
- Read only files directly relevant to the task.
- Follow dependencies only when necessary.
- Reuse context already learned in the current session.
- Modify only files required for the requested task.
- Do not refactor, rename, reformat, or modify unrelated code.
- Preserve the existing architecture, namespaces, and folder structure.
- Prefer extending existing systems over creating duplicate systems.

## Unity MCP
Use Unity MCP only when Editor access is necessary, such as:
- Scene, GameObject, Component, or Prefab operations.
- Checking Unity Console errors.
- Verifying changes that require the Editor.

Do not use Unity MCP for simple C# edits or information already available from files.
Do not repeatedly inspect unchanged Editor state.

## Debugging
When fixing errors:
1. Start from the error and referenced file.
2. Inspect direct dependencies only if necessary.
3. Expand the search only if the cause cannot be found.
4. Fix only errors related to the requested/current change unless asked otherwise.

## Code
- Follow existing project conventions.
- Prefer simple C# over unnecessary abstraction.
- Do not add packages or change ProjectSettings unless required.
- Do not delete assets or perform broad changes without explicit permission.

## Efficiency
For normal tasks:
targeted lookup -> minimal edit -> necessary verification -> stop.

Avoid unnecessary planning, project-wide searches, repeated analysis, and repeated verification.

## Response
After a task, briefly state:
- What changed.
- Which files or Unity objects changed.
- Any important issue requiring user attention.