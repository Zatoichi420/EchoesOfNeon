# Echoes of Neon — Onboarding Design Intake Form

Added 2026-09-17 after reviewing an external AI-game-dev workflow that calls out
"player onboarding design" as its own explicit step — this project doesn't have one
yet. Matches the intake-form pattern used in `Docs/Story/*/`.

## Why this needs its own pass

Most FPS onboarding leans on visual tutorial prompts (an on-screen icon, a highlighted
object, a HUD arrow). None of that exists here by design — live gameplay deliberately
does not go through the screen reader (see `MEMORY.md`'s screen-reader architecture
decision), so a new player's *first* few minutes have to teach sonar pinging, the
acoustic sound-visualizer compass, and Neural Dilate entirely through audio and feel,
with no visual crutch to fall back on if the audio-only explanation doesn't land.

## What's already established

- Full core-loop systems exist and are code-complete (sonar/Neural Dilate/predictive
  ballistics, acoustic event bus, footstep audio, enemy bark/state audio) — onboarding
  would be teaching *existing* mechanics, not designing new ones.
- Mission 1 (`Docs/Story/Scenes/02-mission-one-black-market.md`) is already scoped as
  a soft tutorial for movement and Cookie's scouting/threat-detection use — but it's
  a narrative mission, not a dedicated onboarding sequence, and doesn't yet cover
  sonar pinging or Neural Dilate specifically.

## Open questions

1. Should onboarding be a dedicated tutorial space (a small graybox area, like the
   existing `TestRange_Proto` test scene) before Mission 1 starts, or should it be
   fully diegetic — taught through Mission 1 itself with no separate "tutorial" the
   player can tell apart from the real game?
2. For each core mechanic (sonar ping, sound-visualizer compass, Neural Dilate,
   footstep-noise stealth, Cookie's scouting once she exists), what's the actual
   teaching moment — a spoken prompt the first time it's usable, a forced first use,
   or trusting the player to read a controls doc beforehand?
3. How much does onboarding lean on spoken instruction (via `AccessibilityManager`'s
   `Announce()`, the same pipe used for menu narration) versus teaching purely through
   in-fiction dialogue (Cookie or Marcus's own narration explaining a mechanic in
   character)?
4. Is there a "practice makes it click" design (e.g., a low-stakes area to freely
   sonar-ping without enemy pressure) before the player is under real threat?
5. Should difficulty/pacing of the first real threat encounter be tuned specifically
   to assume the player just learned these mechanics minutes ago?
