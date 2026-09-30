# COOQI "Cookie" — Marcus's Companion

Built from Orlando's direct answers, 2026-09-14. Voice notes still open — see
`Docs/Story/Dialogue/dialogue-intake-form.md`. "Other abilities to be added later" is
Orlando's own phrasing — this file is explicitly not final.

## Name

**COOQI** — Canine Organism for Orientation and Quantum Intelligence *(renamed
2026-09-14 from the original "C.O.Q.I. — Canine Organism with Quantum Intelligence")*.
Pronounced, and always referred to as, **"Cookie."** The acronym is background/lore,
not something spoken aloud in dialogue — nobody calls her "COOQI" to her face.

## What she is

A black Labrador. Desi's legacy — Desi's own creation, built before she died. Marcus
was training her as a puppy when the attack that killed Desi happened, and **both
Marcus and Cookie were injured in that same attack.**

## Body

One cybernetic eye. All four limbs and her tail are made from **Desi's experimental
cybernetic flesh-bonding technology** — not simple prosthetics, but living tissue
fused to mechanical structure. Those limbs are **10x stronger than a normal dog's.**

**Confirmed canon (2026-09-14): Cookie's limbs share the same origin as Marcus's own
rebuild.** Both were hurt in the attack that killed Desi; Marcus rebuilt them both in
the same lab, from the same body of Desi's research, using the same equipment he still
uses today (established in the opening scene). They are, literally, made of the same
thing.

## Connection to Marcus

Linked to Marcus wirelessly. She speaks in a voice **only Marcus can hear** — this is
a private channel, not something the world (or the player, necessarily, outside
Marcus's own perception) has access to. Given Marcus's own perception is already
established as data-driven rather than sight-driven (see `marcus-cross.md`), Cookie's
voice is plausibly one more channel he's trained himself to parse — offered as a
natural fit, not asserted as confirmed mechanism.

## Abilities

- **Scouts ahead** and **locates threats.**
- **Guides Marcus through the world** (confirmed 2026-09-16, Mission 1 draft) —
  distinct from threat-scouting: she also helps him navigate/maneuver, closer to a
  real guide dog's role. Mechanically this is a **new requirement, not yet
  implemented** — the existing `OculusSensorySuite.PulseFrom(origin)` covers her
  scouting/threat-detection reuse of the sonar system, but wayfinding/guidance
  (e.g., leading Marcus toward an objective, or the player following an audio cue
  from her position) is a different mechanic and hasn't been designed yet.
- **Lethal up close** — a real combat asset in melee range, not just a utility/scout
  unit.
- **More to come** — Orlando was explicit that this list isn't finished.

**Guide-dog design decided 2026-09-29 (Orlando), first playable version built:** Cookie **leads automatically toward
the current objective**, staying a few metres ahead. She never forces the player: if Marcus falls behind she **stops and
waits, turned back toward him**; she resumes when he closes up, and **comes back for him** if he wanders far away; when the
two of them reach the objective she stops (Arrived). The player follows her **by ear**: her real 3D-positioned pawsteps
plus a distinct **collar jingle** while she moves, and a quieter **collar rustle every couple of seconds while she waits**,
so her position can always be located without sight. Implemented as `CompanionGuide` (Leading/Waiting/Returning/Arrived)
and `CompanionAudio` in `Assets/Scripts/Player/`; verified in a real Play Mode test (`GuideDogPlayTest`). Still open:
obstacle avoidance/pathfinding (she walks in a straight line), how objectives are handed to her from missions, her voice/
personality, and whether the player can also *command* her.

**Design direction confirmed 2026-09-14: unified with the sonar system, not a
duplicate of it.** Cookie's "scouting" isn't a second, separate detection mechanic —
it's the *same* `OculusSensorySuite` sonar pulse, triggered from wherever she physically
is instead of only from Marcus. Narratively, this reads as her literally scanning an
area with the same sensory suite technology (her own cybernetic eye, presumably built
alongside her limbs from Desi's research) and relaying it over their wireless link,
rather than Marcus mysteriously "knowing" things through an unrelated companion
ability. Mechanically, it means no new detection/scan system needs inventing — see
`OculusSensorySuite.PulseFrom(origin)` (added 2026-09-14), which the player's own
sonar button now also calls. Cookie triggering a pulse from her own position is the
concrete implementation of "scouts ahead," once her own movement/control model exists
(still an open question below).

## Open questions this file does NOT answer

- What does Cookie's "voice" actually sound like/say — is she limited to
  utility/tactical callouts, or does she have real personality and dialogue? This
  matters a lot for tone (a wisecracking companion vs. a purely functional one are very
  different games).
- Is Cookie player-controllable in any way (a command she can be given), or purely
  autonomous/reactive?
- Does she have her own health/can she be taken out of a fight, given her real combat
  role ("lethal up close")?
- Was she rebuilt using Desi's leftover research/equipment specifically, and if so, who
  did the rebuilding — Marcus himself, Max, someone else?
- The remaining abilities Orlando mentioned are coming later — don't assume the list
  above is complete when writing scenes that involve her.

## Relationships

- **Marcus "Echo" Cross** — her handler/partner; the only one who can hear her. See
  `marcus-cross.md`.
- **Desdemona "Desi" Cross** — her creator. Cookie is a living piece of Desi's legacy,
  distinct from (but likely technologically related to) Marcus's own augments. See
  `desdemona-cross.md`.
