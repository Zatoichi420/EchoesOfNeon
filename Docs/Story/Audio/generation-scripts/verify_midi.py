"""
Independent MIDI reader - written separately from midiwriter.py, deliberately
not importing or reusing any of its code, so this actually catches writer
bugs instead of just re-confirming the same logic. Reports per-track note
counts, unmatched note-on/off pairs, and total duration - the things a magic-
byte check like `file` can't see.
"""
import struct
import sys

def read_vlq(data, i):
    value = 0
    while True:
        byte = data[i]
        i += 1
        value = (value << 7) | (byte & 0x7F)
        if not (byte & 0x80):
            break
    return value, i

def parse(path):
    with open(path, "rb") as f:
        data = f.read()

    assert data[0:4] == b"MThd", "missing MThd header"
    header_len, fmt, ntracks, division = struct.unpack(">IHHH", data[4:14])
    assert header_len == 6
    print(f"{path}: format={fmt} tracks={ntracks} ticks_per_beat={division}")

    pos = 14
    for track_i in range(ntracks):
        assert data[pos:pos+4] == b"MTrk", f"track {track_i}: missing MTrk"
        track_len = struct.unpack(">I", data[pos+4:pos+8])[0]
        track_end = pos + 8 + track_len
        i = pos + 8
        abs_tick = 0
        note_ons = {}  # (channel, pitch) -> count
        note_offs = {}
        max_tick = 0
        track_name = None
        running_status = None
        while i < track_end:
            delta, i = read_vlq(data, i)
            abs_tick += delta
            max_tick = max(max_tick, abs_tick)
            status = data[i]
            if status < 0x80:
                # running status: reuse previous status byte, this byte is data
                status = running_status
            else:
                i += 1
                running_status = status if status < 0xF0 else running_status

            if status == 0xFF:
                meta_type = data[i]; i += 1
                length, i = read_vlq(data, i)
                payload = data[i:i+length]; i += length
                if meta_type == 0x03 and track_name is None:
                    track_name = payload.decode("ascii", "replace")
                if meta_type == 0x2F:
                    pass  # end of track
            elif status in (0xF0, 0xF7):
                length, i = read_vlq(data, i)
                i += length
            elif 0x80 <= status <= 0xEF:
                kind = status & 0xF0
                channel = status & 0x0F
                if kind in (0x80, 0x90, 0xA0, 0xB0, 0xE0):
                    d1, d2 = data[i], data[i+1]; i += 2
                    if kind == 0x90 and d2 > 0:
                        note_ons[(channel, d1)] = note_ons.get((channel, d1), 0) + 1
                    elif kind == 0x80 or (kind == 0x90 and d2 == 0):
                        note_offs[(channel, d1)] = note_offs.get((channel, d1), 0) + 1
                elif kind in (0xC0, 0xD0):
                    i += 1
                else:
                    raise ValueError(f"unhandled status {status:#x} at track {track_i}")
            else:
                raise ValueError(f"unexpected byte {status:#x} at track {track_i}, pos {i}")

        unmatched = {k: note_ons.get(k, 0) - note_offs.get(k, 0)
                     for k in set(note_ons) | set(note_offs)
                     if note_ons.get(k, 0) != note_offs.get(k, 0)}
        total_notes = sum(note_ons.values())
        name_str = f'"{track_name}"' if track_name else "(unnamed)"
        status_str = "OK" if not unmatched else f"MISMATCH {unmatched}"
        print(f"  track {track_i} {name_str}: {total_notes} notes, "
              f"ends at tick {max_tick} -> {status_str}")
        pos = track_end
    print()

if __name__ == "__main__":
    for path in sys.argv[1:]:
        parse(path)
