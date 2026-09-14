# Marcus "Echo" Cross — Recording Sheet

Six lines, read in your own voice (per the production decision in `README.md` — Marcus
is the one character not generated via ElevenLabs). Each entry below has: the context
(what's happening when this line is said), the delivery direction (how to play it), the
line itself, and the filename to save the take as. Save finished files into
`Assets/Audio/Dialogue/`, matching the other characters' files already there.

These six lines are everything currently written for Marcus. His proposed voice —
terse, controlled, holding a lot in check, no wry humor, no catchphrase for his
cyberware — was drafted by me, not confirmed by you line-by-line, so if any of these
don't sound like him once you're actually reading them aloud, that's exactly the kind
of thing worth catching now rather than after recording.

---

## 1. Waking, disoriented

**Context:** Opening scene. Marcus wakes on a bed in Desi's old lab, still catching up
to where/when he is.
**Direction:** Quiet, unsteady — not fully present yet.
**Line:** "...Not real. None of that's real anymore."
**Save as:** `marcus_opening_waking.mp3`

---

## 2. Resolve hardening

**Context:** End of the opening scene, right after the waking beat above. His anger and
intent toward The Madame crystallize here — this is the line that launches the player
into the first mission.
**Direction:** Cold, controlled. The anger is real but held in check, not shouted.
**Line:** "The Madame. Ten years she's run this city like she owns it. She's about to
find out what she actually took from me."
**Save as:** `marcus_opening_resolve.mp3`

---

## 3. Spotting a target (combat)

**Context:** Combat — Marcus sights an enemy.
**Direction:** Flat, focused. No excitement, no dread — just noting a fact.
**Line:** "There he is."
**Save as:** `marcus_combat_spot_target.mp3`

---

## 4. Taking a hit (combat)

**Context:** Combat — Marcus gets hit and shrugs it off out loud.
**Direction:** Strained but controlled. He will not let the pain show as weakness.
**Line:** "...Still standing."
**Save as:** `marcus_combat_take_hit.mp3`

---

## 5. After a kill (combat)

**Context:** Combat — right after Marcus takes down an enemy.
**Direction:** Flat. No triumph, no relish — this is a job, not a victory.
**Line:** "One less."
**Save as:** `marcus_combat_after_kill.mp3`

---

## 6. Frustrated self-talk

**Context:** Any moment of self-correction — a missed shot, a lost trail, anything that
breaks his focus for a second.
**Direction:** Clipped, self-correcting — he catches himself and resets immediately.
**Line:** "Focus. Not now."
**Save as:** `marcus_frustrated_selftalk.mp3`

---

## After recording

Drop the six files into `Assets/Audio/Dialogue/` using the filenames above (matching
the other characters' files already there) and let me know — `Assets/Editor/
TestSceneSetup.cs` already knows to look for clips by these exact names once they
exist, the same way it already wires in the ElevenLabs-generated lines for everyone
else.
