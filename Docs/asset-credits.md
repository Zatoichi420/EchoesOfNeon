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

**External SFX library available (not yet used in-project)**: the Sonniss GDC 2026 Game
Audio Bundle — 7.5GB, 122 vendor sound-effect packs, downloaded 2026-09-17 to
`~/Documents/GameAudioLibraries/SonnissGDC2026/Part1`-`Part5` (a cross-project location,
not inside this repo, since it's a shared resource — see the `sfx-resource-directory`
memory entry). License: royalty-free, commercial use permitted, no attribution required,
lifetime/unlimited projects (official license PDF included in each Part folder); the one
restriction is no use for AI/ML training, which doesn't apply to using these as game
assets. **Nothing has been pulled from this into `Assets/Audio/` yet** — when a specific
sound gets used, log it as its own line below (source pack name + which vendor within
the bundle), same as every other asset here.

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
| Thunder clap (sharp, single strike) | Sonniss GDC 2026 bundle, Epic Stock Media — "Public Spaces: Storms, Lakes, Parks..." (Part2) | Sonniss GDC license — commercial use permitted, no attribution | `Assets/Audio/Weather/STORM_Texas Rain Thunder Initial Crash Boom Storm 01 Clap Lightning_ESM_CPS.wav` | Staged 2026-09-28 for Marcus's lair moment Orlando asked for. **2:12 long, raw studio master — needs trimming to just the clap transient before real use**, not yet wired to any code |
| Interior thunder rumble | Sonniss bundle, Jake Fielding — "Interior Wind Rain and Storms" (Part3) | As above | `Assets/Audio/Weather/THUN_Interior Thunder Rumble_JF_INT Storm_01.wav` | Staged 2026-09-28, for distant/ambient thunder heard from inside the lab. Not yet wired |
| Exterior city rain (medium, with traffic) | Sonniss bundle, The Noisery — "City Rain" (Part4) | As above | `Assets/Audio/Weather/RAINConc_Rain Medium Exterior Splatter City Traffic 6_The Noisery_City Rain.wav` | Staged 2026-09-28, candidate for outdoor navigation ambience (Marcus/Cookie). **2:37, raw master — needs loop-trimming.** Not yet wired |
| Interior rain on window | Sonniss bundle, Jake Fielding — "Interior Wind Rain and Storms" (Part3) | As above | `Assets/Audio/Weather/RAINInt_Heavy Rain on Window,  Constant _JF_INT Storm.wav` | Staged 2026-09-28, candidate for lab background layer. 30s, already loop-length-reasonable. Not yet wired |
| Storm ambience (city block) | Sonniss bundle, Sonic Bat — "Stormy Night Ambience" (Part5) | As above | `Assets/Audio/Weather/SBsna_City Block Square Storm 003.wav` | Staged 2026-09-28, alt exterior storm candidate. 2:02, raw master. Not yet wired |
| "Dark Industrial Ambience" (lab background hum) | Sonniss bundle, Cinematic Sound Design — "Sci-Fi Drones" (Part2) | As above | `Assets/Audio/Lab/Dark Industrial Ambience.wav` | Staged 2026-09-28 for Desi's lab low background hum (fan/AC/equipment layer). 31s, loop-length-reasonable. Not yet wired |
| "Roomtone Space Ship Interior Muted" | Sonniss bundle, Cinematic Sound Design — "Sci-Fi Drones" (Part2) | As above | `Assets/Audio/Lab/Roomtone Space Ship Interior Muted.wav` | Staged 2026-09-28, alt lab equipment-hum candidate. 73s. Not yet wired |
| Fridge hum, deep + rhythmic variants (2 files) | Sonniss bundle, Jake Fielding — "Fridge Hums Vol.2" (Part3) | As above | `Assets/Audio/Lab/MACHAppl_*Fridge Hum*.wav` | Staged 2026-09-28 as a fan/AC-hum stand-in — closest electric-hum texture found, not a literal fan recording. **4-4.5 min each, raw masters — need trimming to a short loop.** Not yet wired |
| Menu selection click (2 candidates) | Sonniss bundle, Cinematic Sound Design — "Interface & Infographics" / "System & UI Feedback Elements" (Part2) | As above | `Assets/Audio/UI/Interface Percussion Snap.wav`, `Assets/Audio/UI/Interface Sci-Fi Ping Down.wav` | Staged 2026-09-28 for the menu's audible selection-confirmed cue. Both under 1 second, ready to use as-is once one is picked. Not yet wired |

| Cookie base model (Dog, rigged, Walking+Idle) | Quaternius "Animal Pack Vol.2" via OpenGameArt (opengameart.org/content/animated-animales-low-poly) | CC0 (public domain) | `Assets/Models/Cookie/Cookie.fbx`, `Assets/ThirdParty/Quaternius/` | Downloaded 2026-09-29 (sourced 2026-09-28). Heavily modified: recolored, legs/tail/paws re-materialed as cyber-metal/cyan glow, emissive eye added. Credit optional, courtesy line recommended |

| Marcus base model (Animated Human, rigged, Idle/Walk/Run/Punch/Jump/Death) | Quaternius "Animated Human" via OpenGameArt (opengameart.org/content/animated-human-low-poly) | CC0 (public domain) | `Assets/Models/Marcus/Marcus.fbx`, `Assets/ThirdParty/Quaternius/animated_human/` | Downloaded 2026-09-29. Modified: 32x32 palette atlas recolored (black tactical clothing, dark skin, hair mapped to skin = bald), scaled to 6'2" and widened for a heavyset build, glowing mobility cane and dark smart glasses added as separate props. No phone (standing rule). |

**Open licensing question for before release**: confirm what Orlando's ElevenLabs plan
grants for *commercial* use of library (non-owned) voices in a distributed game. The
generated files are usable for development regardless, but shipping is a different
question and shouldn't be assumed.
