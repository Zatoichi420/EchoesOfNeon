"""
Footstep-on-wet-surface synthesis for Echoes of Neon, per Orlando's request
(2026-09-28): outdoor navigation footsteps for Marcus and Cookie, on rain-
slicked concrete/pavement in New Carthage.

PLACEHOLDER, not a licensed-asset substitute. Real recorded footstep libraries
(e.g. the Nox Sound "Essentials Series" pack identified in the sfx-resource-
directory memory) were the first choice, but every scriptable path to them
was genuinely blocked this session (itch.io needs an interactive claim click,
Freesound needs an API key, OpenGameArt/Kenney are JS-rendered) - see
MEMORY.md's 2026-09-28 entry. This script exists so there's SOMETHING real
and testable in the meantime, matching this project's established fallback
pattern (synth_ambience.py did the same for the ambient beds before any
recorded library was sourced).

Two footstep characters, 4 variations each (randomized seed per run, so
re-running produces fresh variety rather than identical output):
- Marcus: heavier human step. A short low-frequency "weight" thump (the sole
  meeting pavement) layered under a broadband "splash" transient (water
  displaced on a wet/concrete surface). Longer decay, more low end.
- Cookie: light paw-on-wet-pavement tap. Same splash-transient idea, much
  shorter and higher-pitched, no low thump - four legs means lighter
  per-contact weight even though she moves faster.

Output: 44.1kHz mono WAV, one-shot (not looped), matching this project's
existing procedural-audio convention (AcousticEventSystem.cs, synth_ambience.py).
"""
import numpy as np
from scipy import signal
import wave
import os

SR = 44100

def write_wav(path, samples, sr=SR):
    samples = np.clip(samples, -1.0, 1.0)
    pcm = (samples * 32767).astype(np.int16)
    with wave.open(path, 'w') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes(pcm.tobytes())
    print(f"wrote {path} ({len(samples)/sr:.3f}s, {os.path.getsize(path)} bytes)")

def bandpass_noise(duration, sr, low, high, order=4):
    n = max(int(duration * sr), 1)
    noise = np.random.randn(n)
    sos = signal.butter(order, [low, high], btype='band', fs=sr, output='sos')
    return signal.sosfilt(sos, noise)

def lowpass_noise(duration, sr, cutoff, order=4):
    n = max(int(duration * sr), 1)
    noise = np.random.randn(n)
    sos = signal.butter(order, cutoff, btype='low', fs=sr, output='sos')
    return signal.sosfilt(sos, noise)

def envelope(n, attack, decay, curve=2.5):
    """Fast attack, exponential-ish decay - a percussive transient shape,
    not a sustained tone. attack/decay in samples."""
    env = np.ones(n)
    a = min(attack, n)
    d = min(decay, n)
    if a > 0:
        env[:a] = np.linspace(0, 1, a) ** 1.5
    if d > 0:
        tail = np.linspace(1, 0, d) ** curve
        env[-d:] = np.minimum(env[-d:], tail)
    return env

def normalize(x, peak=0.85):
    m = np.max(np.abs(x))
    return x * (peak / m) if m > 0 else x


def make_marcus_footstep_wet():
    """Heavier human step on wet concrete/pavement: a low thump (weight/
    heel strike) plus a broadband splash transient (water displacement)."""
    dur = 0.28
    n = int(dur * SR)

    # Weight thump - low end, short.
    thump = lowpass_noise(dur, SR, 180)
    thump *= envelope(n, attack=int(0.003 * SR), decay=int(0.12 * SR), curve=2.0)

    # Splash - broadband, brighter, slightly longer tail than the thump so
    # it reads as water scattering after the foot lands, not simultaneous.
    splash = bandpass_noise(dur, SR, 800, 9000)
    splash *= envelope(n, attack=int(0.001 * SR), decay=int(0.18 * SR), curve=3.0)

    out = normalize(thump * 0.8 + splash * 0.6)
    return out

def make_cookie_pawstep_wet():
    """Light paw tap on wet pavement - no low thump, short bright splash
    only, roughly a third the duration of Marcus's step."""
    dur = 0.10
    n = int(dur * SR)

    tap = bandpass_noise(dur, SR, 1200, 12000)
    tap *= envelope(n, attack=int(0.0005 * SR), decay=int(0.07 * SR), curve=3.5)

    out = normalize(tap * 0.7)
    return out


if __name__ == "__main__":
    out_dir = os.path.expanduser(
        "~/Developer/EchoesOfNeon/Assets/Audio/Footsteps")
    os.makedirs(out_dir, exist_ok=True)

    np.random.seed()  # fresh randomness per run, not a fixed reproducible seed
    for i in range(1, 5):
        write_wav(os.path.join(out_dir, f"marcus_footstep_wet_{i:02d}.wav"),
                   make_marcus_footstep_wet())
    for i in range(1, 5):
        write_wav(os.path.join(out_dir, f"cookie_pawstep_wet_{i:02d}.wav"),
                   make_cookie_pawstep_wet())
