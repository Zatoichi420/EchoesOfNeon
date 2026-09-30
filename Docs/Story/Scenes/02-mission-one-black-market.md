# Mission 1 (draft) — The Black Market Augment Scene

Built from Orlando's answers (2026-09-16): Marcus starts out to find information that
will help his revenge; Cookie is with him the entire time; the destination is the
black market augment scene (`Docs/Story/locations.md`). **Everything below the
"Confirmed" line in each section is Claude's proposed structure, not stated fact —
correct freely.** Per `story-bible.md`, each main mission needs a minimum of three
sub-missions; this draft proposes three.

## Confirmed
- Marcus leaves Desi's old lab with Cookie, seeking information to further his
  revenge against The Madame.
- Cookie is present for the whole mission, not just part of it.
- Destination: the black market augment scene.
- **This mission is the movement/Cookie tutorial** (Orlando, 2026-09-16): the player
  learns how to maneuver Marcus and Cookie through the world together, and how to
  use Cookie to scout ahead and help guide Marcus around.
- **Marcus carries his mobility cane openly while traveling** — he does not hide his
  blindness. This is a deliberate representation beat, not incidental: he moves
  through the city as an openly blind man using a cane, not someone passing as
  sighted.

## Open questions this draft raises (need Orlando's answers)
1. **Who is the contact Marcus talks to at the black market?** Nobody named yet
   fills this role. Proposed below as a placeholder character, not a real one.
2. **What does the information actually reveal?** This is the hinge into Mission 2,
   and per `story-bible.md`'s own rule, early material can plant clues that only
   read as clues in hindsight — but must never state or imply the twin/bloodline
   truth outright. Three options proposed at the end; needs your pick or your own
   answer.
3. Does this mission introduce a rival faction/gang at the black market, or does the
   friction come purely from corp security along the way?

---

## Sub-mission 1: Night Streets (tutorial)

**Location:** New Carthage at night, the stretch between Desi's lab and the black
market district.

**Confirmed purpose:** this sub-mission IS the tutorial for basic traversal — moving
Marcus and Cookie together, and learning to use Cookie to scout ahead and get
guided around by her. Not a generic "first level," a deliberate teaching beat.

**Beats:**
1. Marcus and Cookie leave the lab. **Marcus has his mobility cane out and in use**
   as they walk — visibly, openly, the whole way. First chance for the player to
   move through open city streets rather than the enclosed lab.
2. **Teach Cookie-guidance here**: proposed as the first mechanic introduced,
   before combat — e.g., a stretch where the direct route is unclear (an alley,
   a junction) and the player learns to let Cookie lead/guide Marcus toward the
   right path, distinct from her separate scouting-for-threats role. *(This is the
   new mechanic flagged as unbuilt in `cookie.md` — needs design before this beat
   can actually be implemented.)*
3. Aegis-Corvus "controls all elements of life in the city" (per `locations.md`) —
   proposed: this means even reaching an unsanctioned black market means avoiding
   corp patrols/surveillance along the way, not just criminals. This is the natural
   first live combat/stealth encounter of the game, using the already-built
   Acoustic Stealth Loop (`TacticalEnemyAI` investigate/alert states) and the
   player's own noise discipline (footstep `AcousticEmitter`).
4. **Cookie's scouting ability gets its first real gameplay use here**, taught
   separately from her guidance role in beat 2 — she can range ahead (ties to
   `OculusSensorySuite.PulseFrom`, already implemented) and flag a patrol before
   Marcus walks into it.

## Sub-mission 2: Finding the Contact (confirmed, Orlando 2026-09-16)

**Location:** Inside the black market augment scene itself — stalls, vendors,
foot traffic.

**Confirmed beats:**
1. Marcus and Cookie don't know exactly where the contact is — they have to
   navigate the market itself, moving between different stalls and different
   people.
2. Marcus asks people around the market for leads on the contact's location.
   Most won't have the answer outright — this is a search/investigation loop, not
   a single conversation.
3. **This needs a real cast of distinct NPC voices** — vendors, market regulars,
   hangers-on — since the player is meant to hear a genuinely varied market, not
   the same voice repeated. (Casting/voice-note-drafting for these NPCs is new
   work, not yet started — worth tracking alongside the still-open Dialogue intake
   form items.)
4. Proposed, not confirmed: this is also a natural place to keep teaching Cookie's
   scouting role from Sub-mission 1 — she could range into stalls/crowds Marcus
   can't easily push through himself and flag which direction has a real lead
   versus a dead end.

## Sub-mission 3: The Confrontation (confirmed, Orlando 2026-09-16)

**Location:** The contact's spot within the black market.

**Confirmed beats:**
1. Marcus and Cookie find and confront the contact directly — this is not a
   peaceful hand-over of information, it's a fight.
2. **Showcase beat for Marcus's combat kit**: his mobility cane (per
   `marcus-cross.md` — blinding flash + embedded blade) and his hand-to-hand
   martial arts skill both get real use here, not just the Vanguard 9 pistol.
3. **Cookie participates as a combat asset**, sent in to attack when needed — her
   established "lethal up close" ability (`cookie.md`) gets its first real showcase.
4. The confrontation ends with Marcus getting the information by force. **This
   closes Mission 1.**

---

## Confirmed payoff (Orlando, 2026-09-16)

Under pressure, the contact gives up **a name or signal tied to the resistance** —
the group already established in `locations.md` as fighting Aegis-Corvus. This is
Marcus's lead into **Mission 2**: he doesn't know or trust them yet, but he now has
a concrete destination. Mission 1 ends here.
