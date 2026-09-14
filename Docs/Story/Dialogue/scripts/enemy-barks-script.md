# Enemy Barks (Generic Corp Security) — Voice Script

Not a named character — generic Aegis-Corvus security. One consistent voice is enough
unless you want variety across different guards (in which case, generate/record this
same table 2-3 times with different voices and randomize which set plays at runtime).
Matches `TacticalEnemyAI`'s existing Patrol/Investigate/Alert states
(`Assets/Scripts/AI/TacticalEnemyAI.cs`) — these are audible flavor barks, separate
from the `AccessibilityManager.Announce` lines already wired into that script for
pure accessibility callouts ("Enemy spotted you," etc.).

See `README.md` in this folder for how to use this table. Delivery direction is
reference only — never paste it into ElevenLabs.

**Generated 2026-09-13** with ElevenLabs voice "The Duke – Gritty Mob Boss"
(`QyX5mnB5hVBPeNS1oyvU`). Audio files are in `Assets/Audio/Dialogue/`. Flow for
regenerating/trying alternates: https://elevenlabs.io/app/flows/wowum720BJ0x3GUneC7X

| Line ID | Scene / Context (reference only) | Delivery direction (reference only) | Script text (paste this) | Suggested output filename |
|---|---|---|---|---|
| bark-patrol-01 | Patrol state, low intensity | Bored muttering | ...another dead shift. | `bark_patrol_01.mp3` |
| bark-patrol-02 | Patrol state, low intensity | Bored muttering | Ought to run this whole block on autopilot. | `bark_patrol_02.mp3` |
| bark-investigate-01 | Investigate state, reacting to a sound | Alert but not yet certain | Who's there? | `bark_investigate_01.mp3` |
| bark-investigate-02 | Investigate state, reacting to a sound | Alert but not yet certain | Thought I heard something. | `bark_investigate_02.mp3` |
| bark-alert-01 | Alert state, spotted the player | Shouting, urgent | Contact! Get eyes on him! | `bark_alert_01.mp3` |
| bark-alert-02 | Alert state, spotted the player | Shouting, urgent | He's here — move! | `bark_alert_02.mp3` |
