# Echoes of Neon — Dialogue Intake Form

**Mostly answered as of 2026-09-14.** Voice/tone was drafted 2026-09-13 in
`voice-notes-draft.md` as a *proposal* (Orlando hadn't directly corrected it line-by-
line); production method got decided the same night (see below); all non-Marcus lines
existing so far have already been generated and are in `Assets/Audio/Dialogue/`. This
original questionnaire is kept as-is for reference — see **Current answers** just below
for where things actually stand, and **Still open** for what genuinely isn't decided
yet. Full line-by-line content lives in `scripts/` (one file per character +
`enemy-barks-script.md`); casting/production detail lives in `scripts/README.md`.

## Current answers

### Marcus's voice
- **How he talks:** terse and controlled, not wry or sarcastic — holds a lot in check,
  speaks in short complete sentences, doesn't editorialize. Anger stays close to the
  surface but rarely comes out in words. *(Proposed in `voice-notes-draft.md`, never
  explicitly corrected by Orlando — already baked into the 6 lines in
  `scripts/marcus-cross-script.md`, so it's live in production even though it was never
  formally confirmed the way the plot reveals were. Worth a deliberate look, not
  assumed-correct by default.)*
- **Verbal tic/nickname for his cyberware:** none proposed — he treats the Oculus
  Sensory Suite as a tool, not an identity. Also never explicitly corrected.
- **Does he narrate to himself:** yes, sparingly — a few short functional lines
  (confirming a plan, reacting to a hit, self-correcting frustration), not constant
  chatter. This is intentionally a second, in-fiction layer on top of
  `AccessibilityManager.Announce`'s pure state-callouts, not a replacement for them.
  Already in the script as `marcus-06`.

### Other voices
- **Who speaks:** Marcus, The Madame, Max, Geoffrey "G", Desdemona "Desi" Cross
  (memory/dream only), and generic Aegis-Corvus enemy security (barks, not a named
  character). No corp comms/radio chatter or resistance-member voices exist yet — see
  **Still open**.
- **Sample lines showing each voice distinct from the others:** drafted in
  `voice-notes-draft.md`, expanded into full per-character tables in `scripts/`.
- **Enemy barks per AI state:** yes — 2 lines each for Patrol (bored muttering),
  Investigate (alert call-out), and Alert (shouting), matching `TacticalEnemyAI`'s
  three states exactly. Live in `scripts/enemy-barks-script.md`, generated, and wired
  into gameplay via `EnemyBarkPlayer.cs`.

### Production
- **Real VO, TTS, or text-only:** hybrid, decided 2026-09-13 — **Marcus is Orlando's
  own voice** (not yet recorded as of this writing); **everyone else is ElevenLabs
  TTS**. Full voice-casting table (names + ElevenLabs voice IDs) in
  `scripts/README.md`.
- **Budget/casting:** no professional real-actor casting beyond Orlando himself —
  ElevenLabs library voices for the rest.
- **Event-trigger lines** (password/code phrase, name that unlocks something, a taunt
  that reveals enemy position): not yet specified — see **Still open**.

## Still open

1. **Marcus's proposed voice/tone was never directly confirmed or corrected** — it's
   already in the recorded scripts, so worth a real look now rather than later, the
   same way the story bible's guesses got corrected once Orlando saw them written out.
2. **Event-trigger lines** — no line currently needs to fire a specific game event
   (password, unlock phrase, position-revealing taunt). Not decided either way.
3. **Corp comms/radio chatter or resistance-member voices** beyond the five named
   characters and generic enemy barks — not decided whether these exist at all.
4. **Audio source quality**: all 15 non-Marcus lines came down as `.mp3` (lossy
   source); fine for development, but Desi's dream line especially carries real
   emotional weight — worth deciding whether to regenerate any line as WAV once it's
   actually been heard in-game.

## Original questionnaire (for reference — what prompted the answers above)

A fill-in questionnaire, matching `Narrative/story-intake-form.md`'s approach — answer
what you can, skip what you can't yet.

### What's already established (at the time this was first written)

- Genre/tone: cyber-noir, tactical, methodical (not fast/quippy action-movie pacing)
- No dialogue has been written yet — no lines exist anywhere in the project
- No voice notes exist yet for Marcus or any other character

### Open questions

### Marcus's voice
1. How does Marcus talk — terse and clipped, wry/sarcastic, formally precise,
   something else?
2. Does he have a verbal tic, a catchphrase, or a specific way he refers to his own
   cyberware/the Oculus Sensory Suite?
3. Does he talk to himself/narrate during gameplay (common in accessible games, since
   it doubles as a diegetic accessibility cue), or stay silent except in cutscenes/
   story beats?

### Other voices
1. Who else actually speaks in this game — other on-screen characters, an AI
   companion or cyberware assistant, corp comms/radio chatter, enemy barks?
2. For each: a few sample lines, even rough ones, that show how they sound different
   from Marcus and from each other?
3. Do enemies have distinct barks per state (patrol muttering, alert shouts,
   investigate call-outs) — the project's `TacticalEnemyAI` already has those state
   transitions in code (see `Docs/action-mapping.md` once it exists), they just have
   no actual line content yet.

### Production
1. Real voice acting, synthesized TTS, or text-only with a screen-reader announcement
   (relevant specifically because this is a blind-first game — dialogue that's only
   ever shown as on-screen text with no audio would be a real accessibility failure)?
2. If real voice acting: any budget/casting preference, or is that a later decision?
3. Any lines that need to trigger a specific game event (a password/code phrase, a
   name that unlocks something, a taunt that reveals enemy position)?
