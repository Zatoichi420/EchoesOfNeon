# Character Actions and Animation Mapping

Per the accessible-game blueprint's Section 3.4: one row per recurring action, mapping
it to its Animator state, its audio cue, and its accessibility announcement text. This
is what the code-review checklist (`Docs/code-review-checklist.md`) checks new actions
against going forward - if you add an action, add a row here and make sure the other
two columns aren't both "none."

Built from a full code audit (2026-09-13) plus that session's Phase 2 fixes. **Every
row's Animator column says "none" on purpose** - there is no character model or
Animator anywhere in this project yet (the player is a `CharacterController` with no
mesh, enemies are capsule primitives). Fill that column in once real character models
exist; don't invent Animator state names for a rig that doesn't exist.

| Action | Animator state | Audio cue | Accessibility announcement |
|---|---|---|---|
| Move (walk) | none | `AcousticEmitter.Emit(footstepLoudnessWalk)` at a fixed cadence — `TacticalPlayerController.UpdateFootsteps()` | none (footsteps are meant to be heard positionally, not narrated) |
| Sprint | none | Same footstep emitter, faster cadence + louder — `TacticalPlayerController.UpdateFootsteps()` | none |
| Jump | none | none | none |
| Interact | none | none (depends on what's interacted with — not yet implemented) | none (depends on the interactable — `IInteractable` has no implementations yet) |
| Fire weapon (Vanguard 9) | none — recoil is raycast-direction bias, not an animation | `AcousticEmitter.Emit(fireLoudness)` — `BallisticWeapon.cs` | none |
| Sonar pulse (player) | none | Player's own pulse: none directly | none |
| Sonar contact detected | none | Target's own `AcousticEmitter.Emit()` answers back — `TacticalEnemyAI.OnSonarPing` | **"Sonar contact, N meters."** — `OculusSensorySuite.DispatchPingAfterDelay`, fires once per pingable hit at the moment it answers back |
| Neural Dilate engage | none | none | **"Neural Dilate engaged."** — `OculusSensorySuite.HandleDilatePressed` |
| Neural Dilate disengage | none | none | **"Neural Dilate disengaged."** — `OculusSensorySuite.HandleDilateReleased` |
| Enemy: Patrol → Alert (spotted) | none | Spoken alert bark, 3D-spatialized, random of 2 — `EnemyBarkPlayer` via `TacticalEnemyAI.OnStateChanged` | **"Enemy spotted you."** — `TacticalEnemyAI.Update` |
| Enemy: → Investigate (heard something) | none | Spoken investigate bark, random of 2 — `EnemyBarkPlayer` | **"Enemy investigating a sound."** — `TacticalEnemyAI.HandleAcousticEvent` |
| Enemy: Investigate → Patrol (stands down) | none | none — deliberately no bark; the patrol clips are idle muttering, not a reaction, and the announcement already covers this beat | **"Enemy stands down."** — `TacticalEnemyAI.UpdateInvestigate` |
| Enemy: Alert → Investigate (lost sight) | none | Spoken investigate bark (destination state is Investigate) — `EnemyBarkPlayer` | **"Enemy lost sight of you."** — `TacticalEnemyAI.UpdateAlert` |
| Enemy: hit, not killed | none | Spoken alert bark (destination state is Alert) — `EnemyBarkPlayer` | **"Enemy alerted."** — `TacticalEnemyAI.TakeDamage` |
| Enemy: idle patrol chatter | none | Spoken patrol bark on a 12-25s randomized timer while in Patrol — `EnemyBarkPlayer.Update`. Doubles as a stealth affordance: an audibly muttering guard is one a blind player can locate and avoid by ear | none — ambient atmosphere, not a state change to announce |
| Enemy: die | none — `enabled = false` only, no death system yet | none | **"Enemy down."** — `TacticalEnemyAI.TakeDamage` |
| Player takes damage | none | none | **none — no system exists.** There is no `PlayerHealth` script and the player doesn't implement `IDamageable`. This is a real gap, not an oversight; flagged for whenever a player-damage system gets built. |

## Known gaps this table exists to prevent repeating

- Any new player or enemy action that ships with both the audio-cue and
  announcement columns still "none" should fail a code review against
  `Docs/code-review-checklist.md`.
- When real character models arrive, every "none" in the Animator column becomes a
  real gap to fill, not a row to delete.
