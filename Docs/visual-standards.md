# Echoes of Neon — Visual Standards Reference

Added 2026-09-17 after reviewing an external AI-game-dev workflow that calls out a
formal visual-standards/asset-pipeline doc as its own step — this project didn't have
one yet, even though real visual direction already exists scattered across concept
art notes and character docs. This file exists so anyone doing visual QA (a sighted
collaborator, or a builder/critic subagent per the new protocol item in `MEMORY.md`)
has one place to check consistency against, since Orlando himself can't verify this
by eye. **Not an intake form** — it's a living reference, updated as real visual
decisions get made, not a list of questions for Orlando to answer.

## Established so far (extracted from existing docs/assets, not invented here)

- **Setting/tone**: rain-slicked, neon-lit industrial metropolis (New Carthage),
  monolithic mega-corporations — cyber-noir, not cyberpunk-neon-rainbow. Dark,
  moody, industrial (this exact language was used to direct the game's official
  theme music too — see the Ableton changelog entry).
- **Cybernetics motif**: cyan/blue bioluminescent glow marks anything
  cybernetic/augmented (Cookie's fused limbs and eye, per her concept art and
  `cookie.md`). This is the one explicit, reusable visual rule found so far — worth
  applying consistently to Marcus's own augments and any future augmented character
  or object, not just Cookie.
- **Warm vs. cold lighting contrast**: Max Gomez's concept art explicitly contrasts
  a warm-lit cluttered workshop interior against cold blue city glow through a
  window — a deliberate interior-warm/exterior-cold split worth checking other
  interior locations against.
- **The Madame**: sharp, immaculately tailored dark high-fashion attire; corporate
  tower office overlooking the rain-soaked neon city. Note: her first concept-art
  pass includes an unrequested subtle facial-structure echo (jaw/eye shape) added
  by the AI generator as a nod to the twin-sibling story reveal — flagged as a
  detail to explicitly confirm keeping or removing before treating it as canon
  visual direction, since it wasn't a specified requirement.
- **Character visual references that exist today**: `Docs/Story/Characters/References/
  marcus-cross-visual-reference.png` (orlando-provided) and the three first-pass
  AI concept images (Cookie, Max, The Madame) in `~/Desktop/EchoesOfNeon_ConceptArt/`.
  Marcus has a known, flagged **inconsistency**: his written physical description
  (salt-and-pepper goatee) contradicts his own reference image (heavyset, bald,
  bearded) — unreconciled as of the 2026-09-17 Blender/animation research pass.
- **In-game visual style is currently graybox-only**: no real materials/lighting/art
  pass exists in Unity yet — primitives only (capsules, cubes) for systems testing.
  Nothing below applies to in-engine work yet; it's concept-art-stage direction.

## Open decisions (not yet made — flag here, don't invent)

1. Resolve the Marcus goatee-vs-bald-and-bearded contradiction between his written
   description and his visual reference before more concept art or a 3D model gets
   built from either one.
2. Confirm or drop The Madame's facial-structure echo to Marcus as intentional
   visual foreshadowing.
3. No formal color palette (hex values) or silhouette rules exist yet beyond the
   cyan/blue-cybernetics and warm/cold-lighting patterns above — worth defining
   explicitly once more than three characters have concept art, so new pieces (G,
   environments, props) stay consistent without needing to eyeball-compare against
   scattered references each time.
4. No established rule yet for how "dark industrial" translates to in-engine
   materials/lighting (URP settings, post-processing) versus how it's described in
   concept art prompts — worth deciding before the first real (non-graybox) scene
   gets built.
