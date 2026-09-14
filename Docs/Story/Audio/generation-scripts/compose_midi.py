"""
Two MIDI sketches for Echoes of Neon, matching the story bible's tone
(grounded, gritty, minimal humor) and the "Matrix/Oracle apartment,
retro-future-modern" location aesthetic. Both are compositional sketches for
Orlando to import into GarageBand and assign real instruments to via its
own software-instrument library - not final production, and deliberately
NOT an answer to the still-open audio-intake-form question of whether music
plays during exploration vs. only combat. Both are offered as options.

A minor throughout both pieces, for the same reason film scores reuse a key
across a theme and its tension variant: they should feel like two faces of
the same idea, not two unrelated pieces of music.
"""
from midiwriter import (
    MidiFile, Track,
    GM_ELECTRIC_PIANO, GM_SYNTH_PAD_WARM, GM_SYNTH_BASS_1, GM_SYNTH_STRINGS,
)

TPB = 480  # ticks per beat (quarter note)
BAR = TPB * 4  # 4/4 time throughout


def make_theme_new_carthage(path):
    """Slow, sparse, moody exploration theme. 62 BPM, 16-bar loop.
    i - iv - i - V (harmonic-minor dominant) progression, the classic noir
    turnaround - built to sit quietly under exploration, not compete with the
    game's own spatial audio cues (per the audio-intake-form's own note that
    a blind player navigates by sound, so music needs to stay out of the way)."""
    mid = MidiFile(ticks_per_beat=TPB)

    meta = Track("Echoes of Neon - New Carthage Theme")
    meta.tempo(0, 62)
    meta.time_signature(0, 4, 4)
    mid.add_track(meta)

    # Chords: (root, third, fifth) - one chord per 4-bar block, 16 bars total.
    # Am - F - C - E(maj, harmonic-minor V) - back to Am on the loop.
    chords = [
        (57, 60, 64),  # A3 C4 E4  (i)
        (53, 57, 60),  # F3 A3 C4  (VI, sounds like "iv" color under A)
        (48, 52, 55),  # C3 E3 G3  (III)
        (52, 56, 59),  # E3 G#3 B3 (V, harmonic-minor dominant - the noir pull back to i)
    ]

    pad = Track("Pad")
    pad.program_change(0, 0, GM_SYNTH_PAD_WARM)
    for block, chord in enumerate(chords):
        start = block * 4 * BAR
        dur = 4 * BAR - 20  # tiny gap so GarageBand doesn't tie identical
                             # pitches across the loop point into one note
        for pitch in chord:
            pad.add_note(start, dur, channel=0, pitch=pitch, velocity=52)

    bass = Track("Bass")
    bass.program_change(0, 1, GM_SYNTH_BASS_1)
    for block, chord in enumerate(chords):
        root = chord[0] - 12  # one octave down
        start = block * 4 * BAR
        # Two long notes per 4-bar block rather than one, so the bass still
        # breathes without turning into a bassline that competes for attention.
        bass.add_note(start, 2 * BAR - 20, channel=1, pitch=root, velocity=58)
        bass.add_note(start + 2 * BAR, 2 * BAR - 20, channel=1, pitch=root, velocity=54)

    # Sparse melody: one to two notes per 2-bar span, drawn from A natural
    # minor (A B C D E F G), wide-spaced and unhurried - matching Marcus's
    # own "terse, controlled" characterization rather than a busy lead line.
    a_minor = [57, 59, 60, 62, 64, 65, 67]  # A3..G4
    melody_plan = [
        # (bar_offset, beat_offset, pitch_index, duration_beats)
        (0, 2, 4, 2.0),   # E4, bar 1 beat 3
        (2, 0, 6, 3.0),   # G4, bar 3 beat 1
        (4, 2, 3, 2.0),   # D4, bar 5 beat 3
        (6, 0, 2, 4.0),   # C4, bar 7 beat 1 (long)
        (9, 1, 5, 2.0),   # F4, bar 10 beat 2
        (11, 2, 4, 1.5),  # E4, bar 12 beat 3
        (13, 0, 1, 2.0),  # B3, bar 14 beat 1
        (14, 2, 0, 2.0),  # A3, bar 15 beat 3 - resolves toward the loop
    ]
    melody = Track("Melody (Rhodes)")
    melody.program_change(0, 2, GM_ELECTRIC_PIANO)
    for bar, beat, idx, dur_beats in melody_plan:
        start = bar * BAR + int(beat * TPB)
        dur = int(dur_beats * TPB) - 20
        melody.add_note(start, dur, channel=2, pitch=a_minor[idx], velocity=68)

    mid.add_track(pad)
    mid.add_track(bass)
    mid.add_track(melody)

    end_tick = 16 * BAR
    mid.save(path, end_tick)


def make_tension_pursuit(path):
    """Driving, anxious cue for chase/combat moments. 138 BPM, 8-bar loop.
    Same A-minor family as the theme (this is what the theme sounds like
    under threat), but a pulsing ostinato bass and dissonant off-beat stabs
    instead of sustained pads."""
    mid = MidiFile(ticks_per_beat=TPB)

    meta = Track("Echoes of Neon - Pursuit")
    meta.tempo(0, 138)
    meta.time_signature(0, 4, 4)
    mid.add_track(meta)

    EIGHTH = TPB // 2

    # Driving bass ostinato: root-root-fifth-root pattern in eighth notes,
    # low register, the "footsteps/heartbeat" pulse under a chase.
    bass_pattern = [45, 45, 52, 45]  # A2, A2, E3, A2 - repeats each beat pair
    bass = Track("Bass Ostinato")
    bass.program_change(0, 1, GM_SYNTH_BASS_1)
    total_eighths = 8 * BAR // EIGHTH
    for i in range(total_eighths):
        pitch = bass_pattern[i % len(bass_pattern)]
        start = i * EIGHTH
        bass.add_note(start, EIGHTH - 10, channel=1, pitch=pitch, velocity=95)

    # Off-beat dissonant string stabs - minor 2nd/tritone clusters landing on
    # the "and" of each beat, the classic anxious-strings device, built from
    # plain intervals rather than a technique (real tremolo) a General MIDI
    # patch can't actually perform.
    stab_clusters = [
        (57, 58, 63),  # A3 + Bb3 (minor 2nd) + D#4 (tritone from A) - dense/tense
        (53, 54, 59),  # F3 + F#3 + B3
    ]
    strings = Track("String Stabs")
    strings.program_change(0, 2, GM_SYNTH_STRINGS)
    beats_total = 8 * 4  # 8 bars * 4 beats
    for beat_i in range(beats_total):
        # Stab on the "and" (offbeat eighth) of every other beat - leaves
        # room to breathe rather than machine-gunning every single offbeat.
        if beat_i % 2 == 1:
            continue
        start = beat_i * TPB + EIGHTH
        cluster = stab_clusters[(beat_i // 2) % len(stab_clusters)]
        for pitch in cluster:
            strings.add_note(start, EIGHTH - 10, channel=2, pitch=pitch, velocity=80)

    # Sparse high alert "stinger" notes marking bar 1 and bar 5 - a cue point
    # a designer could trigger separately for "enemy spotted you" if this
    # ever gets split into stems, per Docs/action-mapping.md's existing
    # accessibility announcement for that same moment.
    stinger = Track("Stinger Accent")
    stinger.program_change(0, 3, GM_SYNTH_STRINGS)
    for bar in (0, 4):
        start = bar * BAR
        for pitch in (69, 70, 75):  # A4, Bb4, D#5 - same cluster, an octave up
            stinger.add_note(start, TPB - 10, channel=3, pitch=pitch, velocity=100)

    mid.add_track(bass)
    mid.add_track(strings)
    mid.add_track(stinger)

    end_tick = 8 * BAR
    mid.save(path, end_tick)


if __name__ == "__main__":
    import os
    out_dir = os.path.dirname(os.path.abspath(__file__))
    make_theme_new_carthage(os.path.join(out_dir, "theme_new_carthage.mid"))
    make_tension_pursuit(os.path.join(out_dir, "tension_pursuit.mid"))
