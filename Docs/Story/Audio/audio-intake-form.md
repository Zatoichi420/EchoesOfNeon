# Echoes of Neon — Audio Intake Form

A fill-in questionnaire, matching the other two intake forms in `Docs/Story/`.

## What's already established

- Audio-first design principle (from the accessible-game blueprint this project is
  drawing on): sound does the job a sighted game's camera and lighting do — it tells
  the player where they are and what matters right now
- Placeholder procedural tones already exist in code (`AcousticEventSystem.cs`,
  `Assets/Scripts/Core/`) for gunfire and mechanical/sonar-answer sounds — synthesized,
  not produced sound design, and explicitly called "placeholder" in the project's own
  changelog
- **First-pass ambient beds exist as of 2026-09-14** for the two locations that
  already have real descriptive detail written (New Carthage night exterior, Desi's
  lab) — `Assets/Audio/Ambient/new_carthage_night_rain.wav` and
  `desi_lab_ambience.wav`, procedurally synthesized, loop-safe, 30s each. Not yet
  wired into any scene, not yet heard by Orlando. The other named locations (Aegis-
  Corvus Tower, resistance base, black market, builder's workshop) still have no
  ambient bed because they don't have descriptive detail to synthesize from yet.
- **First-pass music sketches exist as of 2026-09-14** — two MIDI compositions
  (`Docs/Story/Audio/Music/theme_new_carthage.mid`, `tension_pursuit.mid`) for Orlando
  to open in GarageBand and produce. See that folder's README. These don't answer
  Music Q1 below — they're offered as options for whichever answer it gets.

## Open questions

### Ambient beds and spatial landmarks
1. For each location named in the Narrative intake form: what should its ambient bed
   sound like (rain, machinery hum, distant traffic, crowd noise, near-total silence)?
2. What recurring spatial audio landmarks should mark navigable space by ear alone (a
   dripping pipe = a hallway junction, a specific hum = a working elevator, etc.)?

### Music
1. Should music play during exploration, only during combat/tension, or not at all?
   (Many blind-accessible games deliberately minimize or cut music so it doesn't
   compete with the spatial cues a blind player actually navigates by — worth deciding
   explicitly rather than defaulting to "yes, always.")
2. If music exists: what mood/genre — synthwave, orchestral noir, something else?

### Voice production
Same question as the Dialogue intake form, included here too since it's really an
audio-budget decision: real voice acting vs. synthesized TTS vs. text-only?

### Sound design direction
1. Should the current placeholder procedural tones (gunfire, sonar ping-answer) stay
   fully procedural long-term, or get replaced with produced/recorded sound design
   once the game is further along?
2. Any reference games/sound palettes you want this to feel like (or explicitly not
   feel like)?

### Audio description for cutscenes/story scenes (confirmed requirement, 2026-09-16)

Orlando confirmed this project needs **audio description** — spoken narration of
visually-conveyed action and detail, the same concept as descriptive audio tracks
on film/TV — for story scenes/cutscenes once they exist. **No scenes are built yet**
(the mission drafts in `Docs/Story/Scenes/` are still narrative-only text, not
implemented cutscenes), so nothing needs building right now, but this is a real
requirement to design for going forward, not an afterthought to bolt on later.

**This is a third, distinct audio/accessibility layer, easy to conflate with the two
that already exist — worth keeping clearly separated:**

1. **Menu/UI screen-reader narration** (built) — `AccessibilityManager` +
   `AssistiveSupport`/`AccessibilityHierarchy`, reads static menu items via the
   native OS screen reader. Menus/UI only, never live gameplay.
2. **Live gameplay sonar/acoustic system** (built) — `OculusSensorySuite`,
   `AcousticEventSystem`. Spatial/tactical information during interactive play
   (where things are, what's happening right now) — not descriptive narration, and
   deliberately not screen-reader-based (see the screen-reader architecture
   decision in the project's `MEMORY.md`).
3. **Audio description for cutscenes/story moments** (this section, not yet built)
   — spoken description of what a sighted player would see during non-interactive
   story beats: character actions, expressions, environmental detail that matters
   to the scene but isn't conveyed by dialogue or the acoustic gameplay systems.
   Distinct from both above: it's narration over *non-interactive* content, exists
   only in scenes with visual storytelling (not live gameplay), and needs its own
   voice/delivery treatment (likely a neutral narrator, separate from any character
   voice) rather than routing through either the screen reader or the sonar system.

**Open questions this raises, not yet answered:**
- Delivery: a neutral third-person narrator voice, or does it route through
  Marcus's own perception somehow (since his cybernetics are already established as
  "data not images")? The latter could double as in-fiction justification rather
  than a bolted-on accessibility feature — worth deciding deliberately either way.
- Density/scope: full film-style audio description (every visually meaningful
  detail) or a lighter touch focused only on story-critical beats?
- Production: same voice-production question as Dialogue/Music (real VO vs. TTS)
  applies here too, and could reuse whichever answer those get.
- Implementation timing: since no scenes/cutscenes exist yet, this has no target
  files today — flagged in `MEMORY.md`'s task tracking so it isn't forgotten once
  the first real cutscene gets built.
