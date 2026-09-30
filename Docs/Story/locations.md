# New Carthage — Locations

Built from Orlando's story intake answers (2026-09-13). This is a reference doc for
whoever writes a scene file in `Docs/Story/Scenes/` — each scene should pick a
location from here (or add a new one, per its own entrance/exit/beat structure) and
inherit its established look/feel/sound.

## Overall look, feel, and sound

Interiors are dark and dingy, with a Matrix-style aesthetic — comparable to the
Oracle's apartment in *The Matrix*: older but modernistic, a retro-future-modern feel.
This should inform ambient audio design too, once `Docs/Story/Audio/audio-intake-form.md`
is answered — an interior that looks like the Oracle's apartment probably doesn't sound
like a sterile corporate space.

## Known locations

- **Desi's old lab.** Marcus's home base — he still uses her old equipment to build
  out his own augments, and it's where his memories of her are strongest. Site of the
  opening scene. Environment concept art exists as of 2026-09-28:
  `~/Desktop/EchoesOfNeon_ConceptArt/environments/Desi_Lab_environment_v2.png` (final —
  `Desi_Lab_environment.png` is the earlier pass before the Easter eggs/keepsake below
  were added) — a rumpled bed in the corner (someone lives here), rows of aging
  bio-mechanical research equipment, disassembled cybernetic limb prosthetics with the
  established cyan/blue glow, a chalkboard of molecular diagrams/notation, and the
  warm-interior/cold-blue-window-city contrast established in `visual-standards.md`.
  Approved by Orlando.
  **Keepsakes of Desi, confirmed 2026-09-28**: her glasses (see her physical
  description in `desdemona-cross.md`) resting beside a small photograph, placed on
  the nightstand by the bed — Marcus's one object tying him to her in the space he
  lives in now. Visible in `Desi_Lab_environment_v2.png`.
  **Chalkboard Easter eggs, confirmed and drawn into the art 2026-09-28** (see the
  story bible's open questions/reveals for what each seeds):
  - **Twin gene signature** — two near-identical molecular/DNA diagrams labeled
    "Subject A" / "Subject B (?)" with a margin note about an unexplained duplicate
    anomaly. Seeds the eventual Madame-is-his-twin reveal — Desi found the echo and
    never knew what it meant.
  - **Cookie's funding trail** — a torn corner of an Aegis-Corvus requisition letter
    taped to the board, redacted project name, noting a denied request for
    flesh-bonding tech "but they keep asking." Reinforces that Cookie's tech, not
    Desi, was the attack's real target (already resolved in the story bible).
  - **Unfinished sentence + circled date** — a half-erased line trailing into an
    eraser smear ("...it's not disease. It's not even—"), plus a circled date.
    Currently pure flavor/mystery; the date is a candidate for reuse as an actual
    in-game puzzle code (a locked terminal, a hidden cache) rather than staying
    decorative — not yet wired to anything mechanically. **The specific date rendered
    in the art (12/04/78) is arbitrary, picked by the image model, not a real story
    date** — don't treat it as canon unless/until Orlando deliberately locks one in.
  Note: a couple of the small text fragments the AI rendered on the torn-letter prop
  came out garbled up close (a known AI-text-rendering limitation) — fine for
  concept-art purposes, would need a real typography pass before any production use.
  **New narrative device, proposed 2026-09-28 (not yet built as a system)**: a radio
  in the lab that plays as an ordinary station but is actually how Marcus receives
  covert correspondence from his contacts — a diegetic, audio-native way to deliver
  Easter-egg/lore content that reaches a blind player the same way a sighted player
  reads the chalkboard, matching this project's established assistive-tech-as-power
  design principle (see `marcus-cross.md`'s Perception section). The physical radio
  prop is now in the art (nightstand, by the bed) so the space is ready for it, but
  the mechanic itself needs real design work: how a "broadcast" is triggered, how the
  player tells a real broadcast from a covert one, whether it's tied to specific
  missions/locations or ambient/always-on in the lab.
- **New Carthage at night.** The city itself. Marcus tries to move at night as much as
  possible, keeping to the shadows — this is where his stealth movement (per
  `marcus-cross.md`) and echolocation ability actually get used.
- **The Madame's penthouse.** Her office and her home in one — the top floor of a
  corporate tower, simultaneously her seat of power and where she lives. Environment
  concept art exists as of 2026-09-28:
  `~/Desktop/EchoesOfNeon_ConceptArt/environments/Madame_Penthouse_environment.png` —
  severe black-marble corporate minimalism mixed with eclectic, expensive personal
  taste (fine art, a wine/book wall, rich dark furniture), a large glass terrarium
  built into the room housing an exotic snake as a status-object centerpiece, and
  floor-to-ceiling windows looking DOWN over the rain-soaked neon city far below —
  meant to convey her god's-eye vantage over New Carthage's citizens, not just a
  skyline view. Not yet reviewed by Orlando.
- **The Aegis-Corvus Tower.** Looms large over everything — the corporation controls
  all elements of life in the city. The Madame's seat of power; presumably the site of
  the climax.
- **The resistance's base.** A small rebellion force fighting back against the
  mega-corp — tight-knit and secretive. Not yet given a specific physical description
  beyond that.
- **The black market augment scene.** Where Marcus sources some of his own
  augmentation tech — good, but not top quality compared to what Aegis-Corvus itself
  offers internally.
- **The builder's workshop.** Max Gomez's base of operations, in the slums of the
  mega-corp city, deliberately in an area with no surveillance. Where Marcus gets his
  weapons/mission tech.
