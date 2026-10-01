# Side-scroller pivot: design notes (started 2026-09-30)

Orlando decided to pivot Echoes of Neon from first-person to a **2.5D side-scroller** (3D models, movement locked to the
X/Y plane). Work lives on branch `sidescroller-prototype`; the first-person code on `main` is untouched. Scene:
`Assets/Scenes/SideScrollProto.unity` (rebuild: `-executeMethod SideScrollSceneSetup.Build`; test:
`-executeMethod SideScrollPlayTest.Run`, no `-nographics`/`-quit`). Code: `Assets/Scripts/SideScroll/`.

## Orlando's requested abilities
- **Cane fend-off** (`Fend`, F / left trigger): a 0.3 s window that deflects an incoming attack with no damage and staggers
  the attacker. Orlando's words were "send off a text from anybody that's attacking him" - read as *fend/swat off an attacker*
  (dictation slip); confirm with him.
- **Whistle** (`Whistle`, V / D-pad up): sends Cookie after the nearest enemy (prefers the way Marcus faces, range 16 m).
  She dashes, bites (28 dmg + stagger), trots back; 7 s cooldown, rising chime when ready.
- Cane strike (`Fire`): button-mash 3-hit combo, third hit is a stronger finisher.

## Perception model (blind player)
- Left/right = stereo pan (listener sits on the plane). Height = pitch (enemy hum +1 octave per 5 m above Marcus).
- Enemy attacks: ~0.5 s rising wind-up tone from the enemy's own position, then the strike - learnable by ear.
- Cane edge ticks: tick rate and pitch rise as a gap gets closer (probes 0.8/2/4 m ahead); silence = solid ground.
- `SonarPulse` (Q): spoken scan, e.g. "1 enemy ahead. Nearest ahead, 3 metres."
- Forgiving jump: coyote time 0.18 s, input buffer 0.18 s.

## Super Liam research (2026-09-30; L-Works free accessible side-scroller, 1.54, NVGT)
Sources: l-works.net manual/product page (read); forum/AppleVis pages were 403 (snippets/memory only). The Mac app was NOT found
on this machine (only a 1.52-era save folder); if Orlando moves the download to ~/Desktop we can inspect its sound files.
Verified from the manual: sound left = behind, right = ahead, louder/more central as it closes; every hazard has its own
loop; footstep timbre changes with surface; hazards ~5 tiles wide with 2-tile safe margins; hold the arrow in a jump for air
control (top-requested remake fix); pickups give a slow locator beep, level exit hums; bosses have one gimmick each and
announce attacks by voice ("take that"); 3 save slots, auto-save per act; assists are "cheats" that void score saving; one
patch was needed because hazards spawned without their warning sound.
**Adopt:** guaranteed unique audio cue for every hazard (add a test that fails if a hazard has none); locator beep for
pickups + exit hum; status-query keys (health/position); spoken or sonic telegraph before every attack; one gimmick per
boss; escape/chase sequences with timer-extension assist; per-act autosave; a real interactive tutorial.
**Do differently:** no silent hazards (Super Liam's fans/barriers have no warning margin); named assists that still allow
saving; a cue never goes fully silent (its power-outage level removes all feedback); Cookie's state is voiced.

## Not yet done / open
- Nothing here has been HEARD or played by Orlando; verification is a Play Mode test with scripted commands only.
- Untested: crossing the pit by jumping, fall-respawn, platform jumps, Cookie's collar/paw audio levels, the mix when several
  enemies hum at once, gamepad.
- Fend/Whistle not yet shown in the keybinding menu (`MainMenuUISetup` action list at ~line 477 is hardcoded; also the menu
  scene needs a rebuild).
- Next: pickups + exit hum, hazard loops (pit rumble), audio priority/ducking, damage/death feedback, camera-less level
  readout, a tutorial room, replace procedural tones with real SFX (Sonniss library).

---
# Progression / recovery / augments plan (2026-09-30, DRAFT - awaiting Orlando's answers)

## Canon conflicts to confirm
1. Story bible: Marcus has NO awareness of the vampire bloodline / twin sister until the ending. Per-level "bloodline clues"
   must therefore be worded about his past, Desi, the attack and augment "strain" only, ambiguous until the reveal (no
   "vampire"/"twin" early) - unless Orlando loosens the rule.
2. Overt blood-feeding heal would break (1); use covert "Resonance" (heal on fend/finisher, framed as augments feeding back).
3. Character doc mentions a smartphone; standing rule is cane only - no phone abilities.
Canon already gives Marcus: smart glasses, cybernetic eyes, enhanced hearing/sonar, cane (flash + hidden blade), Max's
non-lethal sonic weapons. Opening flaw: fears prolonged augment use is killing him (strain = bloodline waking). Max's
workshop = upgrade hub. Cookie: 10x limbs, wireless voice only Marcus hears.

## Levels 1-10 (each level also +10 max health, +3% damage; spoken "Level N. X unlocked. Clue available."; Journal key replays clue)
| Lv | Gain | Marcus augment | Cookie augment | Clue |
|1|base|Cane combo, Fend|Whistle bite|"My name is Echo. Before that, I had another one. I can't hear it."|
|2|+10% speed|Echo Step [D] dash through attack|Collar Ping marks threats|"Desi's tools still know my hands better than I do."|
|3|+0.1s fend|Sonar Lock [O] next hit crits|whistle cooldown -1.5s|"Max says I move like my father did..."|
|4|+15% sonar|Flash-Blind [D/O] stuns group|Cyber-Eye calls ledges/enemies above|"The strain again. My hands hum for an hour..."|
|5|+20 hp|Sonic Pulse [O] ranged knockback|Titan Bite breaks guard|"They took my eyes and left everything else. Why only the eyes?"|
|6|+10% dmg|Shadow Veil [D] quiet movement|Guard Mode absorbs a hit|"I do better at night..."|
|7|-20% dmg taken|Resonance [D] heal on fend/finisher|Rally heals Marcus <30%|"The woman on the news speaks like someone I used to know."|
|8|+0.1s combo|Overclock [O/D] 6s boost, strain cost|mirrors Overclock|"Desi was frightened the night before..."|
|9|+15% dmg|Execution Chain [O] blade finisher|Shock Howl sonic stun|"I died once, or something close..."|
|10|lower strain|Echo Storm [O] screen stun|Tandem Strike|"Whatever is waking up in me is not broken. It is mine."|
Marcus's sonic attacks stay stun/knockdown only; the cane blade is the only lethal tool.

## Recovery (ranked)
Rest points (workshop/safe nooks, hum + "rested") > Cookie Rally (Lv7) > Resonance (Lv7) > med-cell pickups with locator
beep (+25%) > slow out-of-combat regen as an assist toggle > overt feeding (post-reveal only).

## Street enemies (each: unique audio signature, wind-up tell, counter)
Pipe Thug (overhead swing; fend) | Knife Runner (lunge; jump) | Bottle Thrower (projectile; fend/duck) | Shield Guard
(front-immune; Cookie from behind) | Zip-Gun Punk (shoots along plane; jump/Echo Step) | Alley Swarm (x3 weak; Flash-Blind) |
Drone Hawk (aerial dive; jump-strike/Cookie Eye) | Brute Bouncer (unblockable slam; jump/Titan Bite).

## Specials
Meter fills on hits/fends; "special ready" spoken. Sonic Pulse, Flash-Blind, Execution Chain, Overclock, Cookie Titan
Bite/Shock Howl, Echo Storm (Lv10). Costs/cooldowns in the agent plan (15-60 special, 4-45 s).

## Code mapping
Extend UpgradeDefinition.TargetStat (MaxHealth, StrikeDamage, FendWindow, ComboWindow, CookieCooldown, CookieBiteDamage,
SpecialCap, DamageTaken); new PlayerXP/LevelProgression (+save), LoreClueLibrary, SpecialMeter, Healer (rest points,
pickups, Resonance, Rally), more SideScrollEnemy archetypes (data-driven), audio-cue registry test (every enemy/hazard/pickup
must have a unique cue).

## Super Liam local inspection (2026-09-30)
App at ~/Desktop/Super Liam/Super Liam.app: NVGT runtime + Steam Audio (Phonon HRTF) + 115 MB encrypted asset pack
(`Resources/exec`) - individual sound names NOT readable (we did not attempt to crack it) - plus a readable manual.html.
From the manual: 100 hp / 3 lives; health boost +25; extra life; pickups = slow beeping locator; harder modes suppress heals;
bosses: one gimmick each (jump-and-land, charge-then-jump, duck overhead swing, two timed escapes, chase + voiced taunt,
final 3-platform boss with tracking laser); end-of-act score tally spoken (kills, time, health %, lives); S/H/L/T status keys
speak the number first; unlockables: boss rush, survival waves, no-damage, one-life, cheat menu (disables score).
Adopt: pickup locator beacons, terrain-keyed footsteps/cane taps, fixed warning lead-time per hazard, spoken boss taunt that
follows the boss in 3D, escape/chase set-pieces, act-end read-out + personal-best sting, status hotkeys, extras/modes.
