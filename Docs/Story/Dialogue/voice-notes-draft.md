# Voice Notes — First Draft (proposal, not confirmed)

Everything in this file is a **proposal drafted from the tone guardrails and
characters already established** (grounded, gritty, minimal humor, cyber-noir,
tactical, methodical) — not something Orlando stated directly, unlike the story
bible's plot content. Treat every line and every voice description here as a
starting point to correct, not a locked decision. Production method (real VO vs.
synthesized TTS) is explicitly undecided for now — these notes are about voice/tone
in writing, not casting or audio pipeline.

## Marcus "Echo" Cross

**Proposed voice:** Terse and controlled, not wry or sarcastic — a man holding a lot
in check. Speaks in short, complete sentences; doesn't editorialize or fill silence.
Under the control there's real anger, close to the surface but almost never let out
in words — it comes out in what he does, not what he says.

**Proposed answer to "does Marcus narrate to himself":** Yes, sparingly — a handful of
short, functional lines during exploration/combat (confirming a plan, registering a
threat, a clipped reaction to taking a hit), not constant chatter. This does double
duty: it's in-character for someone this controlled and methodical, and it gives a
blind player a diegetic accessibility cue without breaking the "screen reader/
narration as the real accessibility layer, not a chatty companion" design this
project has followed elsewhere (`AccessibilityManager.Announce` already exists
for pure state-callouts; Marcus's own lines would be a different, in-fiction layer on
top of that, not a replacement for it).

**Sample lines (draft, not final):**
- *(spotting a target)* "There he is."
- *(taking a hit)* "...Still standing."
- *(after a kill, no triumph)* "One less."
- *(to himself, frustrated)* "Focus. Not now."
- He does **not** refer to his cyberware by a cute nickname or catchphrase — no verbal
  tic proposed. If anything, he treats it as a tool, not an identity, which is itself
  a character note worth keeping (contrast with how naturally he'll eventually have to
  reckon with what he actually is).

## The Madame

**Proposed voice:** Precise, unhurried, never needs to raise her voice — the same
"nothing ever out of place" quality as her wardrobe applies to how she talks. Cutting
when she wants to be, but always controlled; her power reads through composure, not
volume. Corporate-executive diction, colder than any boardroom actually requires.

**Sample lines (draft, not final):**
- *(to an underling, calm)* "Say that again, slower. I want to be sure I heard you
  correctly."
- *(about Marcus, to G — knowing exactly who he is)* "He's persistent. That's not a
  compliment."
- *(closing a conversation)* "That will be all."

## Geoffrey "G"

**Proposed voice:** Almost silent by default — speaks only when there's something to
say, never explains himself, never boasts about the track record everyone else talks
about. When he does speak, it's flat and unnervingly calm, the opposite of Marcus's
barely-contained anger.

**Sample lines (draft, not final):**
- *(before a job, to The Madame)* "Understood."
- *(the only warning he gives)* "Last chance to walk away."
- He should almost never get a full paragraph of dialogue — brevity is the
  characterization.

## Maximilian "Max" Gomez

**Proposed voice:** Warmer than Marcus on the surface — the closest thing to comfort
in Marcus's life — but deflects anything emotionally direct with practical talk (gear,
logistics, the next job). That deflection reads as habit on a first pass and as
guilt-driven avoidance once the player knows what he's carrying.

**Sample lines (draft, not final):**
- *(handing off gear, gruff-warm)* "Sonic emitter's recalibrated. Don't thank me, just
  don't waste it."
- *(when Marcus pushes an emotional topic)* "...Let's talk about the mission."
- *(alone, unheard by Marcus — an aside the player might get)* "*(quiet)* Not yet. Not
  like this."

## Desdemona "Desi" Cross

**Proposed voice:** Warm, intelligent, unhurried — she appears only in memory/dream,
so every line she has should feel like something worth holding onto. Confident in her
own expertise without being cold about it (contrast deliberately with The Madame, who
has confidence but no warmth).

**Sample line (dream sequence, draft only — see
`Docs/Story/Scenes/01-opening-dream-and-waking.md`):**
- *(warm, teasing)* "You're doing that thing again — thinking so loud I can hear it
  from here."

## Enemy barks (generic corp security, not named characters)

Matches `TacticalEnemyAI`'s existing Patrol/Investigate/Alert states and the
announcements already wired into `Assets/Scripts/AI/TacticalEnemyAI.cs` — these are
**audible barks for sighted/hearing players**, separate from and in addition to the
`AccessibilityManager.Announce` lines that already exist for pure accessibility
callouts ("Enemy spotted you," etc.). Keep them short — these fire often.

- **Patrol (muttering, low intensity):** "...another dead shift." / "Ought to run
  this whole block on autopilot."
- **Investigate (call-out):** "Who's there?" / "Thought I heard something."
- **Alert (shout):** "Contact! Get eyes on him!" / "He's here — move!"

## Open items this draft does NOT resolve

- Production method (real VO vs. TTS) — explicitly deferred per Orlando's choice.
- Any lines that need to trigger a specific game event (password/code phrase, a name
  that unlocks something, a taunt that reveals enemy position) — not yet specified.
- Whether other resistance-member or corp-comms/radio-chatter voices exist beyond the
  five named characters and generic enemy barks above.
