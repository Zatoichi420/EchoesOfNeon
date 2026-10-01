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
