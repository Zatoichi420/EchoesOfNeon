"""
Cookie's collar jingle for Echoes of Neon - a small metal tag/bell sound, 3 variations, PLACEHOLDER (procedural,
same status as synth_footsteps.py / synth_ambience.py). Guide-dog audio design (Orlando, 2026-09-29): the player
follows Cookie by ear using her 3D footsteps plus this collar sound; a softer rustle plays while she waits so her
position stays findable when she is standing still.

Each clip is a short cluster of 2-3 inharmonic bell "tings" (metal partials at 1x, 2.76x, 5.4x, the classic bell
ratio) with fast exponential decay. 44.1kHz mono 16-bit WAV, one-shot, matching the project's convention.
"""
import numpy as np, wave, os
SR = 44100

def ting(f0, dur, amp):
    t = np.arange(int(dur * SR)) / SR
    y = np.zeros_like(t)
    for ratio, a, decay in ((1.0, 1.0, 16.0), (2.76, 0.5, 24.0), (5.4, 0.25, 40.0)):
        y += a * np.sin(2 * np.pi * f0 * ratio * t) * np.exp(-t * decay)
    y *= np.minimum(t / 0.001, 1.0)          # 1 ms attack, no click
    return amp * y

def jingle(rng):
    out = np.zeros(int(0.55 * SR))
    at = 0.0
    for _ in range(rng.integers(2, 4)):
        f0 = rng.uniform(2300, 3400); d = 0.35; a = rng.uniform(0.6, 1.0)
        s = ting(f0, d, a); i = int(at * SR); out[i:i + len(s)] += s[:len(out) - i]
        at += rng.uniform(0.05, 0.11)
    return out / max(np.max(np.abs(out)), 1e-6) * 0.7

def write_wav(path, x):
    pcm = (np.clip(x, -1, 1) * 32767).astype(np.int16)
    with wave.open(path, 'w') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(pcm.tobytes())
    print("wrote", path, f"{len(x)/SR:.2f}s")

if __name__ == "__main__":
    out = os.path.expanduser("~/Developer/EchoesOfNeon/Assets/Audio/Companion")
    os.makedirs(out, exist_ok=True)
    rng = np.random.default_rng()
    for i in range(1, 4):
        write_wav(os.path.join(out, f"cookie_collar_{i:02d}.wav"), jingle(rng))
