# Playtest Log

Per the accessible-game blueprint's Section 7: technical QA (does it run, does it
crash) is separate from accessibility QA (can a blind player actually complete this
using only audio). This log is for the second kind — real playtesting, ideally with
blind testers, throughout development rather than only before release.

Log every finding here: date, tester, scene/build, and outcome. Re-run
`Docs/code-review-checklist.md` after any significant playtest-driven change.

## Entries

| Date | Tester | Scene / Build | Outcome |
|---|---|---|---|
| _(none yet)_ | | | |

## Standing open items from the project's own `MEMORY.md` (as of 2026-09-13)

These are the specific things the next live playtest needs to confirm — carried over
here so they're tracked in the same place future playtest findings will be:

- Does the "test scene loaded" announcement actually get spoken with VoiceOver on?
- Do WASD/mouse-look/jump/pause work by hand (only synthetic input has been tested)?
- Does a real PS5/Xbox controller work by hand?
- Are the placeholder acoustic tones audible and panning correctly?
- Are sound-compass blips visible on screen?
- Does left-click fire the pistol with the expected suppressed sound and recoil feel?
- Does the test enemy patrol/investigate/chase correctly and react to gunfire/sonar?
- (Added 2026-09-13) Are the new spoken announcements from this session's Phase 2 work
  (enemy state changes, Neural Dilate, sonar contact) actually audible and clear, not
  overlapping or cut off by other narration?
- (Added 2026-09-13) Does a Mac build launch cleanly, or does it hang on a macOS
  Accessibility-permission prompt the way one freshly-rebuilt test binary did this
  session? If it prompts, grant it and note here whether that resolved the hang.
  **Recurred on the later build too** — the app launches, stays alive, and writes a
  0-byte `Player.log`. This is currently the single thing blocking every by-ear
  verification below.
- (Added 2026-09-13, enemy barks) **Do the spoken enemy barks and the accessibility
  announcements talk over each other?** Both now fire on the same state transition, by
  design — the bark is the positional, in-world voice ("Contact! Get eyes on him!"),
  the announcement is the screen-reader status line ("Enemy spotted you."). They're
  meant to be complementary channels, but whether they collide in practice can only be
  judged by ear. If they do, the likely fix is a short delay on one of them (this
  project already did exactly that once, giving narration a head start over the
  music-ducking fade), not removing either.
- (Added 2026-09-13, enemy barks) Is the 12-25s patrol idle-chatter interval right —
  atmospheric and useful for locating a guard by ear, or annoying/too frequent? Tunable
  on `EnemyBarkPlayer` (`patrolChatterMinInterval`/`patrolChatterMaxInterval`).
- (Added 2026-09-13, enemy barks) Does bark spatialization actually convey guard
  direction/distance usefully? `barkRadius` defaults to 25m with linear rolloff.
- (Added 2026-09-13) Does the new player footstep emission make the test enemy
  actually react to you walking near it? That path existed in code but had nothing
  emitting footsteps until this session.
