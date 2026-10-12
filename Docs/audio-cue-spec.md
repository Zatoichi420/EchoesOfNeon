# Audio cue spec and auditory hierarchy

2026-10-11. Source: the studio audit (Agents 7 and 8), adjusted for the 2.5D side-scroller pivot.
The player reads the game by ear, so every gameplay event has a distinct cue, and the more urgent a cue is, the less anything else may mask it.

## Middleware decision
- **Built-in Unity audio for now, no FMOD.** FMOD's value is FMOD Studio, a visual timeline editor. No evidence was found that it works with VoiceOver or NVDA, so Orlando could not author in it. Unity's built-in AudioSource plus the low-pass filter covers what the audit asked for (occlusion, ducking) from code.
- **Steam Audio (HRTF) deferred.** HRTF mainly solves front/back, which matters for first person. In the 2.5D side-scroller everything is left/right (pan) and up/down (already coded as pitch, one octave per 5 m). Revisit only if a headphone playtest shows positions are hard to read.

## Priority tiers
Higher tiers are never masked by lower ones. Speech priority uses `SpeechPriority`; background ducking uses `AudioDucking`.

| Tier | Events | Behaviour | Status |
|---|---|---|---|
| 1 Critical | Marcus down, both down | Speech cannot be interrupted except by Critical; music and ambience duck to 35% | Done |
| 2 High | Low health, Cookie down, ledge ahead, enemy above, enemy wind-up | Speech interrupts Normal; music ducks; wind-up has a visual twin | Done |
| 3 | Fend window, Cookie bark, ledge tick | Distinct timbres in `SideScrollTones` | Exists |
| 4 | Cookie position (collar, pawsteps), objective/exit pulse | 3D, occluded | Collar done, occlusion done; exit pulse missing in some scenes |
| 5 | Enemy hums, pickups, rest points | 3D, occluded, pitch encodes type and height | Done |
| 6 | Surface change footsteps | Timbre swap | Missing |
| 7 | Weather and ambience beds | Wide, quiet, ducked | Assets staged, not wired |
| 8 | Music, combat stems | Ducked by tiers 1-2 | Menu music ducks; combat music not composed |

## Occlusion
`AudioOcclusion` raycasts from Marcus's head (not the side-on camera) to each enemy, Cookie and locator sound ten times a second. Solid level geometry in between glides a low-pass filter to 900 Hz; open air is 22 kHz. Characters (any CharacterController) and the sound's own object never occlude.

## Still to do
- Swap procedural tones for the Sonniss library, starting with loop-trimmed rain and lab beds.
- Exit/objective pulse, wall and corridor proximity, surface-change footsteps.
- Combat and tension music (Orlando composes in Ableton and GarageBand).
- A cue-registry test: every hazard and enemy kind must map to a cue.
- Headphone playtest by Orlando: occlusion depth (900 Hz may be too dull), duck depth (35%), and whether HRTF is needed.
