# Marcus "Echo" Cross — Protagonist

Built from Orlando's story intake answers (2026-09-13). Voice notes are still open —
see `Docs/Story/Dialogue/dialogue-intake-form.md`, not yet answered.

## Role, want, need, flaw, arc

- **Want:** revenge against Aegis-Corvus Dynamics — specifically against The Madame,
  not the company in the abstract.
- **Need (inferred, not stated — confirm or correct):** the resistance's larger fight
  suggests his arc may widen from personal revenge toward something that actually
  serves New Carthage, not just himself. This is Claude's inference from the premise,
  not something Orlando specified — flag if wrong.
- **Flaw:** established directly in the opening scene — he's angry, disoriented, and
  genuinely unsure whether pushing his augments this hard for this long is safe. His
  pursuit of The Madame may be costing him something physical, not just emotional.
  **Payoff, canon as of 2026-09-14:** what he reads as his body failing is actually the
  bloodline waking up. It's the same phenomenon The Madame and G read as proof the
  prophecy is real — they see a man doing what no one else can with the same cyberware;
  he feels only strain and dread. The audience holds both halves long before Marcus
  does, which is where the tension in his arc lives. Write his fear as genuine and
  un-ironic: he is not secretly suspicious of the truth, he thinks he's dying.
- **Arc destination:** the ending's "tough decision" — per the story bible's now-
  confirmed spoiler, this centers on discovering The Madame is his twin sister and
  that the Cross bloodline is descended from vampires. **Marcus does not know either
  fact for the entire game** until the reveal; write him with zero awareness of it,
  not subconscious hints he'd recognize as such.

## MAJOR SPOILER — do not let this leak into early dialogue or narration

Marcus is unknowingly descended from vampires, and The Madame — the woman he's hunting
for revenge — is his twin sister. Neither fact should be stated or implied by Marcus
himself, or by any narration/UI text, before the actual reveal. See
`Docs/story-bible.md` for the full reveal and its still-open follow-up questions
(does The Madame already know; does this connect to Desi's death).

Worth noting, not asserting as intentional: his established habits — moving only at
night, keeping to shadows, and augments that prioritize hearing/echolocation over
restored sight — already read as thematically consistent with vampiric traits he
doesn't consciously know he has. This is Claude's observation from the two pieces of
canon landing together, not something Orlando stated as deliberate; flagged for
confirmation.

## Physical description

6'2", 250 lbs, African-American male, late 40s to early 50s, salt-and-pepper goatee,
scar under his left eye.

## Costume and gear

- Black tactical military gear.
- Black smart glasses that enhance his ocular sensors, feeding him more information
  about the world around him.
- A mobility cane that doubles as a weapon: flashing lights capable of blinding an
  enemy, plus an embedded bladed weapon he wields with deadly accuracy.
- His own smartphone for hacking and bypassing firewalls — used via his screen reader
  and accessibility settings, the same way a real blind user would.
- Arsenal favors sonic weapons (pulses of energy/sound that disable rather than kill
  outright) over traditional firearms — Max Gomez supplies this tech.

## Movement

Stealthy — moves quickly in and out of shadows. Tries to operate at night as much as
possible.

## Perception — and why this matters for the actual game, not just the story

Marcus's augments work like a heightened version of real assistive technology: smart
glasses plus enhanced hearing. Critically, **his cybernetic eyes provide only data,
not images** — a deliberate Matrix-style parallel. He perceives the world as patterns
of raw information rather than pictures, and had to train himself to interpret that
data before he mastered it.

This is not just flavor text — it's the same design principle already built into
`OculusSensorySuite.cs` (sonar echolocation, not a visual HUD overlay) and
`AcousticEventSystem.cs` (the sound-visualizer compass reads acoustic events, not
sight lines). The lore and the mechanics were built independently and land on the
same idea: Marcus's power is real assistive technology turned into a superpower
framing, not sight restored. Worth keeping the two in sync as both develop further.

## Relationships

- **Desdemona "Desi" Cross** — wife, deceased. The reason for everything. See
  `desdemona-cross.md`.
- **Maximilian "Max" Gomez** — military brother, mentor/supplier figure. See
  `max-gomez.md`.
- **The Madame** — the specific target of Marcus's revenge, and (secret, see above)
  his twin sister. See `the-madame.md`.
- **Geoffrey "G"** — a cousin Marcus has no idea exists, working as The Madame's
  enforcer. See `geoffrey-g.md`.
- **COOQI "Cookie"** — his companion, a black lab Desi built and Marcus was
  training as a puppy. Both were injured in the attack that killed Desi. Linked to
  Marcus wirelessly; speaks in a voice only he can hear. See `cookie.md`.

## Visual reference

`References/marcus-cross-visual-reference.png` — provided by Orlando: a heavyset,
bald, bearded man in dark tactical gear and sunglasses, walking through a rain-soaked,
fire-lit industrial alley at night, carrying a glowing cane. Designated as visual
inspiration for both Marcus's look and New Carthage's overall tone.
