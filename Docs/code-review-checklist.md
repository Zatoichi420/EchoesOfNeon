# Code Accessibility Review Checklist

Run this manually after a coding session, on request — not an automated background
agent, per the accessible-game blueprint's Section 6.1 and Orlando's explicit choice
for this project (2026-09-13).

## Checklist

1. **Every new player or enemy action has an accessibility announcement AND an audio
   cue**, cross-checked against `Docs/action-mapping.md`. If a new row's Animator
   state, audio cue, or announcement columns would all read "none," that's a finding,
   not a style note — add the row to the table either way so the gap is visible.
2. **No mouse-only or visual-only interaction path was introduced.** Every input must
   be reachable via keyboard AND gamepad, per `InputManager`'s existing dual-input
   design.
3. **New UI elements are wired to the screen-reader bridge**, not just visually
   rendered. This project uses `UnityEngine.Accessibility`'s native
   `AssistiveSupport`/`AccessibilityHierarchy` API (see `AccessibilityManager.cs`) —
   check that a new UI element either registers an `AccessibilityNode` or has an
   explicit `AccessibilityManager.Announce(...)` fallback if it conveys information a
   screen reader user needs.
4. **Decompiled or third-party game code is never committed.** Not currently relevant
   (this is an original game, not a mod of an existing title), but keep this check in
   case that ever changes.
5. **Silent failure check** (this project's own recurring bug pattern, not in the
   original blueprint template — added after this codebase's first real audit found
   it twice): does a new system fail *silently* if a dependency is missing or a
   permission/hardware capability isn't available? `TacticalPlayerController`,
   `OculusSensorySuite`, and `AccessibilityManager` all log a warning and either
   disable themselves or degrade gracefully when `InputManager`/`AccessibilityManager`
   is missing — match that pattern, don't let a new script fail with no signal at all.

## How to run it

Ask explicitly: "run the code accessibility audit against `Docs/code-review-
checklist.md`" (or similar) after a coding session, pointing at the specific files
that changed. Log findings and fixes in `MEMORY.md`'s Changelog, same as every other
session in this project.
