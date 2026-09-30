# Maximilian "Max" Gomez — Mentor / Supplier

Built from Orlando's story intake answers (2026-09-13). Voice notes still open — see
`Docs/Story/Dialogue/dialogue-intake-form.md`.

## Role

Marcus's contact in the slums of the mega-corp city, who supplies his equipment. Knew
Marcus from their time in the military together — served in the same unit. Acts as a
mentor/brother figure to Marcus, one of his few real remaining relationships.

## Physical description

Latin American descent, heavily tattooed. Wears an eyepatch over one eye that
conceals cybernetic modifications — a multi-use tech device that enhances what he
sees and hears through the cybernetic eye, feeding him information at a constant
rate. Walks with a limp from a field injury sustained on a mission that went wrong
(military service, predates the current story).

## Function in the story

Supplies Marcus with tech for his weapons and missions — Marcus's arsenal favoring
sonic weapons (pulses of energy/sound to disable rather than kill) over traditional
firearms comes from Max. Operates out of "the builder's workshop," in the slums of the
mega-corp city, in an area with no surveillance.

**Confirmed 2026-09-16: the workshop is a recurring upgrade hub, not a one-off
visit.** Marcus can return throughout the game for:
- **Cane upgrades** (his mobility cane per `marcus-cross.md` — blinding flash +
  embedded blade).
- **Augment upgrades** — faster movement, and "more in tune with everything in the
  world" (Orlando's own phrasing). Proposed, not confirmed: this likely maps to
  `TacticalPlayerController` move speed and `OculusSensorySuite` sonar range/
  clarity/`Neural Dilate` effectiveness — a real gameplay system, not yet
  designed or built. Flagged in the project's own `MEMORY.md` task tracking.
- **Cookie upgrades** too — exact scope not yet specified (her scouting/guiding/
  combat abilities are all candidates, per `cookie.md`).

## MID-STORY REVEAL — Max secretly leads the resistance (confirmed by Orlando, 2026-09-16)

**Max Gomez is the leader of the resistance.** This is separate from, and revealed
earlier than, the deeper guardian-angel/father's-death/bloodline secret below — it
comes to light at the end of **Mission 2** (`Docs/Story/Scenes/03-mission-two-the-
resistance.md`), not the final act. The lead Marcus forces out of the Mission 1
contact leads him to infiltrate the resistance's hidden base — and the leader he
finally confronts there turns out to be Max. **Marcus is caught completely off
guard by this — he has no idea beforehand.**

This means Max now operates in two capacities in Marcus's life that the player must
be able to tell apart while writing: the mentor/supplier persona Marcus already
knows from earlier in the game (see "Role" above), and the secret resistance
leadership, hidden even from Marcus, until Mission 2's end. **Per Orlando (2026-09-
16), Marcus and Max need real screen time together as friends, in Max's known
persona, before Mission 2's reveal** — so the reveal actually lands as a betrayal-
shaped shock rather than meeting a stranger. Where that friend-time content lives in
the mission structure is still an open question (see the note in
`03-mission-two-the-resistance.md`).

**Confirmed 2026-09-16: after the reveal, Max does not come clean.** He stalls and
deflects, and actively sends Marcus on wild goose chases and red herrings — false
leads that keep Marcus busy and (presumably, protectively) away from the real
truth. This isn't a one-scene beat; it's a throughline for however much of the
mid-game follows Mission 2, until it eventually unravels. **RESOLVED 2026-09-16: Mission 3 is a partial red herring** — see
`Docs/Story/Scenes/04-mission-three-the-lie-unravels.md`. During that mission,
Marcus figures out for himself that Max has been lying to him, and it hardens his
resolve rather than breaking him. Exact mechanism of discovery and what's "partial"
about the herring are still open in that file.

**Inferred, not confirmed** — worth checking with Orlando: since stalling/
deflecting is a manipulation tactic, not a combat one, this suggests the Mission 2
confrontation itself de-escalates once Max reveals himself (he pivots to talking
his way out, rather than the fight continuing) — but that's Claude's read of the
implication, not something stated outright.

Writing guidance: nothing in Max's earlier friendly appearances should tip his
resistance leadership before Mission 2 - same hindsight-clue discipline already used
for his deeper secret below. Once past Mission 2, Max's dialogue needs to support a
second re-read too: red herrings that feel like genuine leads on a first playthrough
but read as deliberate misdirection once the player knows what he's protecting.

## MAJOR SPOILER — the planted secret, now concrete (confirmed by Orlando, 2026-09-13)

**The timeline goes back further than Marcus and Max's shared unit.** Max knew
Marcus's father in the military — a high-ranking officer who was killed under
suspicious circumstances when Marcus was really young. **Max had a hand in that
assassination.** He has never told Marcus, and this — not just abstract knowledge of
the family secret — is the specific thing he's been carrying this whole time.

This resolves the timeline question the first draft of this file left open: Max's
connection to the Cross family predates his and Marcus's shared unit entirely — he
knew the father first. The most direct reading is that Max's guardian-angel
protectiveness toward Marcus, ever since, is **atonement** for the role he played in
orphaning him — whether that's guilt alone or something more deliberate (protecting
the one surviving heir out of obligation, not just remorse) is not yet specified.

**Marcus's father** is established but not yet named — a high-ranking military
officer, killed under suspicious circumstances while Marcus was a child. Not yet
clear whether "suspicious circumstances" means the assassination was disguised as
something else at the time, or whether it was always known to be a killing with the
culprit never identified until now (i.e., until the player learns Max did it).

**Open questions this raises, not yet answered:**
- Was the father's assassination connected to the Cross bloodline/vampire secret at
  all (e.g. he knew or was protecting something, and was eliminated for it), or was it
  unrelated to that thread and Max's guilt is the only connection?
- Was Max ordered to carry out the killing, acting alone, or something else — and if
  ordered, by whom? This could connect to whoever enabled The Madame's rise, but that
  is speculation, not something confirmed.
- Does Max know the family secret (twins, vampire descent) *because* of his role in
  the father's death — did he learn it investigating or covering up what he'd done —
  or did he learn it some other way entirely?

**Writing guidance until the reveal**: Max can be written with subtle protectiveness
that reads as loyalty-from-shared-service on a first pass, but should support a
re-read as guilt-driven atonement once the player knows the truth — the classic
"clues that only make sense in hindsight" approach the story bible calls for. He
should never say or imply anything that gives away either the family secret or his
role in the father's death before the actual reveal scene.

## Relationships

- **Marcus "Echo" Cross** — former unit-mate, current mentor/supplier, and (secret)
  the son of a man Max helped kill. See `marcus-cross.md`.
- **Marcus's father** — a high-ranking military officer Max served under/alongside,
  killed under suspicious circumstances when Marcus was young. Not yet named.
