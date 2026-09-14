# Asset Credits

Per the accessible-game blueprint's Section 5.2: log every external asset here as it's
added — source, license, and where it's used. This becomes the in-game/store-page
credits screen and the legal record for distribution. CC0 (Poly Haven, Kenney) is
safest for anything distributed or modded publicly; always check the license tab
before use, not after.

**No external *visual* assets have been sourced yet** — the visual layer is still
code-generated (capsule/cube primitives for graybox testing). The blueprint's Section
5.1 sources (Unity Asset Store, Poly Haven, Kenney, Sketchfab for environments;
Mixamo, Ready Player Me for characters) remain unused, so that discipline is in place
before sourcing starts rather than retrofitted after a pile of untracked downloads.

**AI-generated voice audio is logged below.** It isn't a third-party licensed asset in
the usual sense, but it belongs here for two real reasons: it's likely a
credits-screen line if this ships ("Voices generated with ElevenLabs"), and the
licensing tier genuinely matters — one voice pick failed outright with "you need to be
on the creator tier or above to use this voice," which means voice availability is
account-tier-dependent and worth recording per asset rather than rediscovering later.

## Log

| Asset | Source | License | Used in | Notes |
|---|---|---|---|---|
| 3 voice lines, The Madame | ElevenLabs, voice "Nora Vale - Mission Control" (`iV1OKYxbmgRtpzJ2q8kx`), model `eleven_multilingual_v2` | Per Orlando's ElevenLabs account terms — confirm commercial-use rights for library voices before shipping | `Assets/Audio/Dialogue/madame_*.mp3` | Generated 2026-09-13 |
| 3 voice lines, Max Gomez | ElevenLabs, "Santiago - Gravelly and commanding" (`VXZKNah1ssrzM8c81OiY`) | As above | `Assets/Audio/Dialogue/max_*.mp3` | Generated 2026-09-13 |
| 2 voice lines, Geoffrey "G" | ElevenLabs, "Taras Vovk - Tactical Game NPC" (`11JUpgNlQsWfppULB2T8`) | As above | `Assets/Audio/Dialogue/g_*.mp3` | Generated 2026-09-13 |
| 1 voice line, Desdemona Cross | ElevenLabs, "Maryanne" (`wGkDFmrqhadewOUJsHKq`) | As above | `Assets/Audio/Dialogue/desi_opening_dream.mp3` | Generated 2026-09-13. Fallback pick — first choice "Laura - Calm Mediterranean" required a higher account tier |
| 6 enemy bark lines | ElevenLabs, "The Duke – Gritty Mob Boss" (`QyX5mnB5hVBPeNS1oyvU`) | As above | `Assets/Audio/Dialogue/bark_*.mp3` | Generated 2026-09-13. Generic corp security, not a named character |
| Marcus "Echo" Cross voice lines | Orlando's own voice recordings | Owned outright | _(not yet recorded)_ | Pending — Orlando is recording these himself |

**Open licensing question for before release**: confirm what Orlando's ElevenLabs plan
grants for *commercial* use of library (non-owned) voices in a distributed game. The
generated files are usable for development regardless, but shipping is a different
question and shouldn't be assumed.
