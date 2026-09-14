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

## Voice casting — decided 2026-09-13

- **Marcus "Echo" Cross** — Orlando's own voice. Not generated via ElevenLabs.
- **The Madame** — ElevenLabs "Nora Vale - Mission Control" (`iV1OKYxbmgRtpzJ2q8kx`)
- **Maximilian "Max" Gomez** — ElevenLabs "Santiago - Gravelly and commanding" (`VXZKNah1ssrzM8c81OiY`)
- **Geoffrey "G"** — ElevenLabs "Taras Vovk - Tactical Game NPC" (`11JUpgNlQsWfppULB2T8`)
- **Desdemona "Desi" Cross** — ElevenLabs "Maryanne" (`wGkDFmrqhadewOUJsHKq`) — the
  first pick, "Laura - Calm Mediterranean," required a higher ElevenLabs account tier
  and failed; Maryanne was the fallback.
- **Enemy barks** (generic corp security) — ElevenLabs "The Duke – Gritty Mob Boss"
  (`QyX5mnB5hVBPeNS1oyvU`)

**All 15 non-Marcus lines have already been generated** (one take each) and downloaded
into `Assets/Audio/Dialogue/` in this project, named per each script's "Suggested
output filename" column.

**Known quality caveat, not yet acted on:** these came down as `.mp3`, which is a
lossy *source* format. Unity re-encodes audio at build time, so an mp3 source means
lossy-on-lossy. It's fine for development and probably fine for barks, but if any line
ever needs to be pristine — Desi's dream line especially, since it carries real
emotional weight — regenerate it from ElevenLabs as WAV and replace the file. Worth
deciding once the lines have actually been heard, not before. They're also still live on ElevenLabs in five flows (one per
character) if you want to regenerate a line, try a different voice, or hear alternate
takes — the flow URLs are in each character's script file below. Re-running a line
there starts and charges a new generation; it doesn't touch the files already
downloaded into this project.

## Files

- `marcus-cross-script.md`
- `the-madame-script.md`
- `max-gomez-script.md`
- `geoffrey-g-script.md`
- `desdemona-cross-script.md`
- `enemy-barks-script.md` (generic corp security, not a named character)
