# Narrative Consistency Review Checklist

Run this manually after a writing session, on request — not an automated background
agent, per the accessible-game blueprint's Section 6.2 and Orlando's explicit choice
for this project (2026-09-13).

**Runnable as of 2026-09-13** — the story bible, five character files, a locations doc
and one written scene now exist. First run: `Docs/Story/narrative-review-2026-09-14.md`
(findings only, no story changes made). Its headline finding is a theme/ending mismatch
worth resolving before more scenes get written.

## Checklist (once a story bible and scenes exist)

1. **Does this scene advance a character's stated want/need?** Check against that
   character's file in `Docs/Story/Characters/` — a scene that doesn't move anyone's
   want/need forward is disconnected filler, even if it's well-written.
2. **Are there unresolved setups with no matching payoff?** A planted object, a
   promise, a threat introduced in one scene needs a payoff scene somewhere later in
   the scene list — cross-check `Docs/Story/Scenes/` for orphaned setups.
3. **Does the current ending scene actually satisfy the resolution statement** from
   the story bible's Section 3.1, or has the story drifted from it? This is the one
   check that should block "is this actually done," not just "is this scene good."
4. **Is character voice consistent** with that character's voice-notes file in
   `Docs/Story/Characters/`? A line that reads fine in isolation can still be wrong
   for who's supposedly saying it.

## How to run it

Ask explicitly: "run the narrative consistency audit against `Docs/narrative-review-
checklist.md`" after a writing session, pointing at the specific scene/character files
that changed or were added. Log findings in `MEMORY.md`'s Changelog, same as code
review findings.
