# Voice Scripts — How to Use These

One file per character, each line already drafted in `../voice-notes-draft.md` or
`../../Scenes/01-opening-dream-and-waking.md`. Nothing new was written here — this is
those same lines reorganized so each character's dialogue can be generated (via
ElevenLabs) or recorded (by you) one line at a time.

## Table columns

- **Line ID** — a stable reference so a generated/recorded audio file can be named to
  match (see Suggested Filename) and so `Docs/action-mapping.md` or a future line-list
  can point at it unambiguously.
- **Scene / Context** — reference only, **do not paste this into ElevenLabs**. Tells
  you what's happening when the line is said.
- **Delivery direction** — reference only, **do not paste this into ElevenLabs**.
  Deliberately kept out of the actual script text: unsupported inline style tags (like
  `[angry]`) get read aloud *literally* by most TTS systems instead of being
  interpreted as direction, which would ruin the take. Use this column to pick/adjust
  the ElevenLabs voice's stability/style settings before generating, or as your own
  acting direction if you're reading the line yourself.
- **Script text** — paste exactly this into ElevenLabs' text field (or read it
  yourself). Nothing else.
- **Suggested output filename** — name the generated/recorded file this when you save
  it, so it's traceable back to this table later.

## Voice casting — not yet decided

Which ElevenLabs voice (or your own voice) goes with which character isn't specified
yet. `voice-notes-draft.md` has a proposed tone per character if that helps pick a
voice — Marcus (controlled, terse), The Madame (precise, cold), Max (warm-but-
deflecting), G (flat, minimal), Desi (warm, unhurried). Enemy barks are generic corp
security, not a named character — one consistent voice is enough unless you want
variety across different guards.

## Files

- `marcus-cross-script.md`
- `the-madame-script.md`
- `max-gomez-script.md`
- `geoffrey-g-script.md`
- `desdemona-cross-script.md`
- `enemy-barks-script.md` (generic corp security, not a named character)
