# Unity Spark and Unity AI: findings and trial plan (2026-10-08)

Status: research only. Nothing has been uploaded to Spark. Spark was announced 2026-10-07; most of
this comes from news coverage and Unity's own pages, so re-check before relying on any detail.

## What Spark is (public info)
- Browser-based AI game maker (Chrome): chat window + 3D view + a web editor. Not part of the desktop Editor.
- Built on artist-made Asset Store content. Closed beta "later in 2026", via Google's Playground platform.
- Reported at launch: no download, no export to desktop Unity, cannot sell games made with it. New projects only
  (not confirmed). No MCP, CLI or API documented. Data/IP terms, Unity version and accessibility: not published.
- Conclusion: cannot be used on Echoes of Neon today. Worth revisiting if export or an automation route appears.

## Unity plans (unity.com/products/compare-plans, read 2026-10-08)
| Plan | Price | Who | AI credits |
|---|---|---|---|
| Personal | Free | under $200K revenue/funding in prior 12 months | 1,000/month (trial; paid subscription required) |
| Pro | $210/month or from $2,310/year | required above $200K | 2,000/month |
| Enterprise | custom | required above $25M revenue | 3,000/month |
| Industry | custom | non-game apps over $1M | 3,000/month |

Personal excludes console deployment, Apple Vision Pro and splash-screen customization. No refunds on subscriptions; no
downgrade during a commitment. Unity AI's in-editor assistant: free trial for Personal, then $10/month; included in Pro and up.

## MCP: a conflict between sources
The Unity AI page (unity.com/features/ai) says the official MCP server, CLI and plugin are free with no concurrency limits
and need no AI credits or subscription. A research agent's summary of an earlier blog post said it needed the AI beta plan.
Treat the official page as authoritative but verify when installing. Either way, the community connector we use
(mcp-unity) works headless and needs no subscription. The official server was described as needing an open Editor.

## Trial plan (only if Spark offers export or an automation route)
1. Copy the project with `ditto` (not mv), without Library/, to `~/Developer/EchoesOfNeon-Spark`, on a branch `spark-sandbox`.
   Never point Spark at the main repo.
2. Before any upload: read the beta terms (data use, training, ownership of outputs, asset licenses) and log them in
   `Docs/asset-credits.md`. Test whether the Spark interface works with VoiceOver at all.
3. First trial: one greybox corridor with one objective trigger and Cookie navigation. Not menus or UI.
4. Import output as new files only (new scene, `Assets/Spark/`). Wrap generated scripts so they go through
   AccessibilityManager, AudioRouting, InputManager, PauseState, StatusRegistry.
5. Verify with the headless EditMode/PlayMode suites plus a new simulated-input Play Mode scenario; extend the node tests to
   walk every AccessibilityNode in the generated scene. Merge only after a human audio + VoiceOver pass.

## Acceptance checklist for any Spark-generated asset
- [ ] Asset license logged; terms allow commercial use and redistribution.
- [ ] Every interactable and hazard has a unique audio cue, a spoken announcement and a caption.
- [ ] Operable with keyboard and gamepad; rebindable via InputManager.
- [ ] Every AudioSource bound through `AudioRouting.Bind` (barks = Dialogue).
- [ ] Pause honored (`PauseState`); a `StatusRegistry` entry where relevant.
- [ ] No settings that do nothing (`SettingsAuditTests` passes); failures are announced, not silent.
- [ ] Focus lands somewhere meaningful after actions; selection state is spoken, not color-only.
- [ ] One MonoBehaviour per file, in the `EchoesOfNeon.Runtime` asmdef, with tests.
- [ ] Story canon and `Docs/narrative-review-checklist.md` pass (e.g. Marcus: cane only, no phone).
- [ ] `Docs/code-review-checklist.md` audit run; `MEMORY.md` changelog entry added.
- [ ] Human audio and VoiceOver pass before merge.
