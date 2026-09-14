"""
Ambient bed synthesis for Echoes of Neon, per Docs/Story/locations.md.
Only synthesizes the two locations that already have real descriptive detail
written down (New Carthage night rain, Desi's lab) - not inventing texture
for locations that don't have one yet (Aegis-Corvus Tower, resistance base,
black market, builder's workshop).

Output: 44.1kHz mono WAV, loop-safe (loop point crossfaded), matching this
project's existing procedural-audio convention (AcousticEventSystem.cs also
generates mono clips). WAV chosen over mp3 deliberately - it's the lossless
source; Unity re-encodes at import/build time regardless, so starting lossy
would be lossy-on-lossy for no reason (same caveat already noted for the
ElevenLabs dialogue, which had no WAV option there - this does, so use it).
"""
import numpy as np
from scipy import signal
import wave
import struct
import os

SR = 44100

def write_wav(path, samples, sr=SR):
    """samples: float array in [-1, 1], mono."""
    samples = np.clip(samples, -1.0, 1.0)
    pcm = (samples * 32767).astype(np.int16)
    with wave.open(path, 'w') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes(pcm.tobytes())
    print(f"wrote {path} ({len(samples)/sr:.1f}s, {os.path.getsize(path)} bytes)")

def crossfade_loop(samples, fade_secs, sr=SR):
    """Makes samples loop seamlessly by crossfading the tail into the head,
    so AudioSource.loop=true in Unity doesn't click/pop at the seam."""
    fade_n = int(fade_secs * sr)
    if fade_n * 2 >= len(samples):
        return samples
    head = samples[:fade_n].copy()
    tail = samples[-fade_n:].copy()
    fade_in = np.linspace(0, 1, fade_n)
    fade_out = 1 - fade_in
    blended = head * fade_in + tail * fade_out
    # Replace the head with the blend, drop the old tail - the loop point is
    # now seamless (end of array flows into what is now the start).
    out = samples.copy()
    out[:fade_n] = blended
    out = out[:-fade_n]
    return out

def bandpass_noise(duration, sr, low, high, order=4):
    n = int(duration * sr)
    noise = np.random.randn(n)
    sos = signal.butter(order, [low, high], btype='band', fs=sr, output='sos')
    return signal.sosfilt(sos, noise)

def lowpass_noise(duration, sr, cutoff, order=4):
    n = int(duration * sr)
    noise = np.random.randn(n)
    sos = signal.butter(order, cutoff, btype='low', fs=sr, output='sos')
    return signal.sosfilt(sos, noise)

def highpass_noise(duration, sr, cutoff, order=2):
    n = int(duration * sr)
    noise = np.random.randn(n)
    sos = signal.butter(order, cutoff, btype='high', fs=sr, output='sos')
    return signal.sosfilt(sos, noise)

def normalize(x, peak=0.9):
    m = np.max(np.abs(x))
    return x * (peak / m) if m > 0 else x


# ---------------------------------------------------------------------------
# New Carthage at night (rain, distant city, occasional low rumble)
# Per locations.md: "Marcus tries to move at night as much as possible" -
# this is the exterior city bed, rain-slicked per the story bible's premise.
# ---------------------------------------------------------------------------
def make_new_carthage_rain(duration=30.0):
    # Rain "hiss" layer - broadband, mid-high frequency, the classic rain-noise
    # register.
    rain = bandpass_noise(duration, SR, 1500, 9000, order=4)
    # Slow amplitude wobble so it doesn't read as a static, obviously-looped hiss.
    t = np.linspace(0, duration, int(duration * SR), endpoint=False)
    wobble = 0.85 + 0.15 * np.sin(2 * np.pi * 0.07 * t + np.random.rand() * 6.28)
    rain = rain * wobble
    rain = normalize(rain, 0.5)

    # Distant city rumble - very low frequency, city-at-night register (traffic,
    # distant machinery, the city's own bass hum).
    rumble = lowpass_noise(duration, SR, 90, order=4)
    rumble_wobble = 0.7 + 0.3 * np.sin(2 * np.pi * 0.03 * t + 1.7)
    rumble = rumble * rumble_wobble
    rumble = normalize(rumble, 0.35)

    # Occasional distant "thunder/traffic-swell" events - a handful of slow
    # low-frequency swells scattered through the duration, not on a fixed grid
    # (fixed intervals would read as an obvious loop artifact).
    swell_bed = np.zeros_like(rain)
    n_events = max(1, int(duration / 9))
    for _ in range(n_events):
        start = np.random.uniform(0, duration - 3.0)
        swell_dur = np.random.uniform(1.5, 3.0)
        swell = lowpass_noise(swell_dur, SR, 60, order=4)
        env_n = len(swell)
        env = np.hanning(env_n)
        swell = swell * env * np.random.uniform(0.25, 0.45)
        start_i = int(start * SR)
        end_i = min(start_i + env_n, len(swell_bed))
        swell_bed[start_i:end_i] += swell[: end_i - start_i]

    mix = rain * 0.6 + rumble * 0.5 + swell_bed
    mix = normalize(mix, 0.85)
    mix = crossfade_loop(mix, fade_secs=2.0)
    return mix


# ---------------------------------------------------------------------------
# Desi's lab - per locations.md: "dark and dingy... Matrix-style aesthetic...
# older but modernistic, retro-future-modern... not sterile, not purely
# mechanical." Per the opening scene doc: "equipment hums."
# ---------------------------------------------------------------------------
def make_desi_lab_ambience(duration=30.0):
    t = np.linspace(0, duration, int(duration * SR), endpoint=False)

    # Dual-tone electrical hum (mains-hum register plus its first harmonic) -
    # the "old fluorescent-lit equipment" texture, not a clean synth tone.
    hum = (
        0.5 * np.sin(2 * np.pi * 60.0 * t)
        + 0.3 * np.sin(2 * np.pi * 120.0 * t)
        + 0.15 * np.sin(2 * np.pi * 180.3 * t)  # slightly detuned - avoids a
                                                  # too-clean/synthetic beat
    )
    # Very slow amplitude drift so the hum breathes rather than sitting dead flat.
    hum *= 0.6 + 0.08 * np.sin(2 * np.pi * 0.05 * t + 0.4)
    hum = normalize(hum, 0.22)

    # Broadband low mechanical bed - the "not purely mechanical, not sterile"
    # texture underneath the hum, like old climate/cooling equipment.
    mech_bed = lowpass_noise(duration, SR, 220, order=3)
    mech_bed *= 0.75 + 0.25 * np.sin(2 * np.pi * 0.11 * t + 2.1)
    mech_bed = normalize(mech_bed, 0.3)

    # Sparse, irregular metallic ticks/clicks and short mid-frequency whirs -
    # equipment doing something, occasionally, not a rack of servers idling.
    clicks_bed = np.zeros_like(hum)
    n_clicks = max(2, int(duration / 4))
    for _ in range(n_clicks):
        start = np.random.uniform(0, duration - 0.4)
        kind = np.random.choice(["click", "whir"])
        if kind == "click":
            click_dur = np.random.uniform(0.01, 0.03)
            n = int(click_dur * SR)
            click = highpass_noise(click_dur, SR, 3000, order=2)
            env = np.linspace(1, 0, n) ** 2
            click = click * env * np.random.uniform(0.15, 0.3)
            start_i = int(start * SR)
            end_i = min(start_i + n, len(clicks_bed))
            clicks_bed[start_i:end_i] += click[: end_i - start_i]
        else:
            whir_dur = np.random.uniform(0.3, 0.7)
            n = int(whir_dur * SR)
            freq = np.random.uniform(300, 700)
            wt = np.linspace(0, whir_dur, n, endpoint=False)
            whir = np.sin(2 * np.pi * freq * wt) * np.hanning(n)
            whir = whir * np.random.uniform(0.08, 0.15)
            start_i = int(start * SR)
            end_i = min(start_i + n, len(clicks_bed))
            clicks_bed[start_i:end_i] += whir[: end_i - start_i]

    mix = hum + mech_bed + clicks_bed
    mix = normalize(mix, 0.8)
    mix = crossfade_loop(mix, fade_secs=1.5)
    return mix


if __name__ == "__main__":
    out_dir = os.path.dirname(os.path.abspath(__file__))
    np.random.seed(20260914)

    rain = make_new_carthage_rain(30.0)
    write_wav(os.path.join(out_dir, "new_carthage_night_rain.wav"), rain)

    lab = make_desi_lab_ambience(30.0)
    write_wav(os.path.join(out_dir, "desi_lab_ambience.wav"), lab)
