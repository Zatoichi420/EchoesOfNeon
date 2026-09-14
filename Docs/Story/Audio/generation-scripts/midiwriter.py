"""
Minimal, correct Standard MIDI File (SMF) Format 1 writer. No external
dependency (mido/midiutil aren't installed, and installing one onto Orlando's
system for a one-off script isn't worth the footprint) - this implements just
enough of the spec to be useful: multi-track, tempo, time signature, track
names, program change, note on/off. Verified against the SMF spec's own
field layout, not guessed at.
"""
import struct

def _vlq(value):
    """MIDI variable-length quantity encoding for delta-times."""
    buf = [value & 0x7F]
    value >>= 7
    while value:
        buf.insert(0, (value & 0x7F) | 0x80)
        value >>= 7
    return bytes(buf)

class Track:
    def __init__(self, name=None):
        self.events = []  # list of (abs_tick, event_bytes)
        if name:
            self.meta(0, 0x03, name.encode("ascii", "replace"))

    def meta(self, tick, meta_type, data: bytes):
        self.events.append((tick, bytes([0xFF, meta_type]) + _vlq(len(data)) + data))

    def tempo(self, tick, bpm):
        usec_per_qn = int(60_000_000 / bpm)
        data = struct.pack(">I", usec_per_qn)[1:]  # 3 bytes
        self.meta(tick, 0x51, data)

    def time_signature(self, tick, numerator=4, denominator=4):
        denom_pow2 = {1: 0, 2: 1, 4: 2, 8: 3, 16: 4}[denominator]
        data = bytes([numerator, denom_pow2, 24, 8])
        self.meta(tick, 0x58, data)

    def program_change(self, tick, channel, program):
        self.events.append((tick, bytes([0xC0 | channel, program])))

    def note_on(self, tick, channel, pitch, velocity):
        self.events.append((tick, bytes([0x90 | channel, pitch, velocity])))

    def note_off(self, tick, channel, pitch, velocity=0):
        self.events.append((tick, bytes([0x80 | channel, pitch, velocity])))

    def add_note(self, start_tick, duration_ticks, channel, pitch, velocity=90):
        self.note_on(start_tick, channel, pitch, velocity)
        self.note_off(start_tick + duration_ticks, channel, pitch, 0)

    def to_bytes(self, end_tick):
        self.meta(end_tick, 0x2F, b"")  # End of Track, always last
        # Sort by absolute tick; stable sort preserves insertion order for
        # same-tick events (e.g. a note-off and the next note-on at t=0).
        ordered = sorted(self.events, key=lambda e: e[0])
        out = bytearray()
        prev_tick = 0
        for abs_tick, ev in ordered:
            delta = abs_tick - prev_tick
            out += _vlq(delta)
            out += ev
            prev_tick = abs_tick
        return b"MTrk" + struct.pack(">I", len(out)) + bytes(out)


class MidiFile:
    def __init__(self, ticks_per_beat=480):
        self.ticks_per_beat = ticks_per_beat
        self.tracks = []

    def add_track(self, track: Track):
        self.tracks.append(track)

    def save(self, path, end_tick):
        header = b"MThd" + struct.pack(">IHHH", 6, 1, len(self.tracks), self.ticks_per_beat)
        body = b"".join(t.to_bytes(end_tick) for t in self.tracks)
        with open(path, "wb") as f:
            f.write(header + body)
        print(f"wrote {path} ({len(header) + len(body)} bytes, {len(self.tracks)} tracks)")


# General MIDI program numbers used by the compositions, named for clarity
# when Orlando reassigns instruments in GarageBand.
GM_ELECTRIC_PIANO = 4
GM_SYNTH_PAD_WARM = 89
GM_SYNTH_BASS_1 = 38
GM_STRING_ENSEMBLE = 48
GM_SYNTH_STRINGS = 50
