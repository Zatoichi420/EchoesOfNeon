# Accessibility milestones

Source: 2026-10-07 multi-agent review of the repo against Unity's accessibility manual
(https://docs.unity3d.com/6000.5/Documentation/Manual/accessibility/_index.html).
Work in sets of three; after each set, stop and playtest before starting the next.

## Milestone 1: make it honest, testable, pausable
1. **Wire or hide stubbed settings.** Colorblind mode and high-contrast outlines drive the
   sound compass. Settings with no system behind them (screen shake, aim assist, subtitles,
   audio description, SFX/dialogue volume) are hidden from the menu until they work.
   `AccessibilityManager.UnimplementedSettings` is the single list; an EditMode test fails
   if a setting is neither consumed nor listed there.
2. **Test framework.** `EchoesOfNeon.Runtime` asmdef, `Assets/Tests/EditMode`, tests for
   the settings audit and for AccessibleButton/Toggle/Slider node create/remove balance.
3. **Real pause menu.** Time stops, Resume / Main Menu / Quit, spoken "Paused" and
   "Resumed", focus lands on Resume, Neural Dilate cannot fight the pause.

**Test gate:** open the game, press Escape (pause), navigate with arrows + VoiceOver,
resume; check the Accessibility tab no longer shows dead controls; run EditMode tests.

## Milestone 2: speech and structure
4. Menu focus speech + TTS fallback when no screen reader is running.
5. TextMeshPro-aware labels, Back/dismiss action, hints, panel headers, selected state.
6. HUD "status" hotkey (health, objective, ammo, threats) + announcement queue.

## Milestone 3: audio and captions
7. SFX and dialogue volume actually applied; mono option.
8. CaptionService: subtitles, bark text, non-speech sound captions; re-enable the settings.
9. Text size / UI scale; real colorblind palettes beyond the compass.

## Milestone 4: controls and onboarding
10. Rebind WASD; toggle variants for held abilities; timing options.
11. Difficulty setting.
12. Audio tutorial, hint key, first-run accessibility prompt.

## Later / platform
Hook `screenReaderStatusChanged` and `nodeFocusChanged`; OS caption/font-scale settings on
iOS/Android builds; haptics toggle; rename our `AccessibilitySettings` to avoid clashing
with `UnityEngine.Accessibility.AccessibilitySettings`.
