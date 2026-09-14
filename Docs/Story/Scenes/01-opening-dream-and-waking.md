# Scene 01 — Opening: Dream and Waking

Built from Orlando's story intake answers (2026-09-13). Dialogue lines below are
**placeholder/sample only** — `Docs/Story/Dialogue/dialogue-intake-form.md` hasn't
been answered yet (Marcus's actual voice, whether he narrates to himself, and Desi's
speech patterns are all still open). Treat every quoted line here as a stand-in
showing the *shape* of the beat, not final script.

## Location and time of day

A dream-space memory (soft, undefined "before" — pre-accident, pre-augmentation)
dissolving into Desi's old lab (see `Docs/Story/locations.md`), present day, likely
pre-dawn or deep night given Marcus's established habit of moving at night.

## Characters present

- **Marcus "Echo" Cross** — both the dreaming self (whole, unaugmented, sighted per
  the pre-accident memory) and the waking self (present-day, augmented).
- **Desdemona "Desi" Cross** — present only within the dream/memory.

## Beat-by-beat action list

1. **Dream begins.** Marcus and Desi share a quiet, warm moment together — a fond
   memory of the life he used to live, before Aegis-Corvus took it from him.
   *(Specific activity/content of the memory not yet specified — placeholder beat
   only.)*
2. **A subtle cue signals this is a dream**, per Orlando's explicit note that the
   audience needs to recognize the dream state without it being announced outright.
   For an audio-first game, this should be an *audio* signal, not a purely visual one
   — e.g. a faint reverb/warmth on Desi's voice that never occurs in "real" present-
   day dialogue elsewhere, so a blind player gets the same signal a sighted player
   gets from a soft-focus visual treatment. (Suggestion, not a locked decision.)
3. **Transition/wake.** Marcus wakes on a bed in Desi's former laboratory.
4. **Disorientation beat.** Marcus registers where he is — the lab, still full of
   Desi's old equipment, which he now uses to maintain his own augments. This is a
   natural moment for the player to first exercise Oculus Sensory Suite mechanics
   (sonar pulse to get his bearings) as a diegetic, story-motivated action rather than
   a pure tutorial prompt.
5. **Internal conflict.** Marcus is angry and worried — specifically, whether it's
   safe to keep using his augmentations for the long periods he's been pushing them.
   This is the seed of his stated flaw (see `marcus-cross.md`) and should land as a
   real beat, not a throwaway line. **This beat is load-bearing as of 2026-09-14**: his
   fear pays off at the ending as the bloodline waking up, not his body failing. Don't
   cut or soften it, and don't play it as suspicion — he genuinely believes the
   cyberware is killing him.
6. **Resolve hardens.** Revenge against Aegis-Corvus Dynamics is already on his mind —
   and specifically against its head, The Madame, not the company as an abstraction.
   Scene ends on this resolve, launching the player into the first real mission.

## Audio cue list

- A dream-state audio filter on Desi's dialogue (see beat 2) — distinct from any
  other voice treatment in the game.
- A waking/alarm-adjacent sound marking the dream-to-reality cut.
- Lab ambience: equipment hums, a retro-future-modern texture consistent with
  `Docs/Story/locations.md`'s "Oracle's apartment" reference — not sterile, not
  purely mechanical.
- Marcus's own breathing/disorientation sound on waking.
- A sonar-pulse SFX (already implemented — `AcousticEventSystem`/`OculusSensorySuite`)
  as Marcus reorients himself in the lab.

## Lighting / mood

- **Dream:** warm, soft — a visual treatment for sighted players; the audio filter
  above is what carries the actual "this is a memory" signal for a blind player.
- **Lab (reality):** dark and dingy, per the established New Carthage interior look.

## Dialogue (draft, per `Docs/Story/Dialogue/voice-notes-draft.md` — still not final)

Per the dialogue voice-notes draft, Marcus narrates sparingly, so his waking/resolve
lines are proposed as spoken (in-fiction narration), not silent internal-monologue-as-
text — matching his sparing-but-present self-narration pattern.

- Desi (dream, warm): *"You're doing that thing again — thinking so loud I can hear it
  from here."*
- Marcus (waking, disoriented): *"...Not real. None of that's real anymore."*
- Marcus (internal, resolve-hardening): *"The Madame. Ten years she's run this city
  like she owns it. She's about to find out what she actually took from me."*

These are draft samples matching the proposed voice notes, not confirmed final lines —
correct freely.
