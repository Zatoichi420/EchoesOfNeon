"""
Offline stereo preview of Cookie as a guide dog (Orlando, 2026-09-29). NOT a recording from Unity - Unity cannot capture
audio in batch mode - but an approximation built from the game's own pieces: the same guide-dog state machine
(CompanionGuide.cs: Leading/Waiting/Returning/Arrived), the same clips (Assets/Audio/Footsteps/cookie_pawstep_wet_*.wav,
Assets/Audio/Companion/cookie_collar_*.wav), the same cadence rules (CompanionAudio.cs), and a simple listener model:
equal-power panning, a small interaural time difference, head-shadow and behind-the-head darkening, and distance
rolloff. Spoken labels come from macOS `say`. Output: 44.1 kHz stereo WAV + AAC m4a in the concept-ideas Sounds folder.
"""
import numpy as np, wave, os, subprocess, tempfile, math, random
SR = 44100
ROOT = os.path.expanduser("~/Developer/EchoesOfNeon/Assets/Audio")
OUT = os.path.expanduser("~/Desktop/EchoesOfNeon_ConceptIdeas/Sounds")
random.seed(7); np.random.seed(7)

def load(path):
    with wave.open(path) as w:
        assert w.getnchannels() == 1
        x = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16).astype(np.float32) / 32768.0
    return x

STEPS = [load(f"{ROOT}/Footsteps/cookie_pawstep_wet_{i:02d}.wav") for i in range(1, 5)]
COLLAR = [load(f"{ROOT}/Companion/cookie_collar_{i:02d}.wav") for i in range(1, 4)]

def say(text):
    with tempfile.TemporaryDirectory() as d:
        a, w = f"{d}/s.aiff", f"{d}/s.wav"
        subprocess.run(["say", "-o", a, text], check=True)
        subprocess.run(["afconvert", "-f", "WAVE", "-d", "LEI16@44100", "-c", "1", a, w], check=True)
        return load(w)

def lowpass(x, cutoff):
    a = 1 - math.exp(-2 * math.pi * cutoff / SR); y = np.empty_like(x); s = 0.0
    for i, v in enumerate(x): s += a * (v - s); y[i] = s
    return y

def spatialize(clip, lx, lz, volume):
    """Return (left, right) for a source at listener-local right=lx, forward=lz metres."""
    d = max(math.hypot(lx, lz), 0.3)
    gain = volume * min(1.0, 1.5 / d)                 # Unity-style logarithmic rolloff, min distance 1.5 m
    pitch = random.uniform(0.95, 1.05)
    x = np.interp(np.arange(0, len(clip) - 1, pitch), np.arange(len(clip)), clip).astype(np.float32)
    x = lowpass(x, max(2500.0, 16000.0 / (1.0 + d * 0.25)))          # distance darkening
    if lz < 0: x = lowpass(x, 3500.0)                                   # behind the head is darker
    side = max(-1.0, min(1.0, lx / d)); ang = (side + 1) * math.pi / 4
    gl, gr = math.cos(ang) * gain, math.sin(ang) * gain
    itd = int(abs(side) * 0.0007 * SR)                                   # far ear hears it up to 0.7 ms later
    xl, xr = x.copy(), x.copy()
    if side > 0: xl = np.concatenate([np.zeros(itd, np.float32), lowpass(x, 5000.0)])[:len(x)]   # source on right: left ear late + shadowed
    elif side < 0: xr = np.concatenate([np.zeros(itd, np.float32), lowpass(x, 5000.0)])[:len(x)]
    return xl * gl, xr * gr

class Guide:                                                              # mirrors CompanionGuide.cs
    def __init__(self, c, obj): self.c, self.obj, self.state, self.speed = np.array(c, float), np.array(obj, float), "Leading", 0.0
    def tick(self, m, dt):
        dP = np.linalg.norm(m - self.c); dO = np.linalg.norm(self.obj - self.c); sp, face = 0.0, None
        if self.state == "Leading":
            if dO <= 1.2: self.state = "Arrived"
            elif dP >= 6: self.state = "Waiting"
            else: sp, face = 3.2, self.obj - self.c
        elif self.state == "Waiting":
            if dP >= 12: self.state = "Returning"
            elif dP <= 3.5: self.state = "Leading"
        elif self.state == "Returning":
            if dP <= 3.5: self.state = "Leading"
            else: sp, face = 4.5, m - self.c
        elif self.state == "Arrived":
            if dP >= 12: self.state = "Returning"
        if sp > 0 and face is not None:
            n = np.linalg.norm(face); self.c += face / n * min(sp * dt, n)
        self.speed = sp

def render_scene(m, yaw, guide, seconds, marcus_policy, mix, t0, timers):
    """Advance the simulation, adding Cookie's sounds into `mix` starting at sample offset t0."""
    dt = 1 / 60
    for k in range(int(seconds / dt)):
        m, yaw = marcus_policy(m, yaw, guide, dt)
        guide.tick(m, dt); t = k * dt
        d = guide.c - m
        right = np.array([math.cos(yaw), -math.sin(yaw)]); fwd = np.array([math.sin(yaw), math.cos(yaw)])
        lx, lz = float(d @ right), float(d @ fwd)
        def emit(clips, vol):
            l, r = spatialize(random.choice(clips), lx, lz, vol); i = t0 + int(t * SR)
            mix[i:i + len(l), 0] += l[:len(mix) - i]; mix[i:i + len(r), 1] += r[:len(mix) - i]
        if guide.speed > 0.1:
            timers["step"] += dt; timers["col"] += dt
            if timers["step"] >= np.interp(guide.speed / 4.5, [0, 1], [0.5, 0.26]): timers["step"] = 0; emit(STEPS, 0.7)
            if timers["col"] >= 1.0: timers["col"] = 0; emit(COLLAR, 0.4)
        else:
            timers["step"] = 0; timers["col"] += dt
            if timers["col"] >= 2.5: timers["col"] = 0; emit(COLLAR, 0.22)
    return m, yaw

def idle(m, yaw, g, dt): return m, yaw
def follow(m, yaw, g, dt):                                                # the automated player used in GuideDogPlayTest
    d = g.c - m; dist = np.linalg.norm(d)
    if g.state != "Arrived" and (g.state == "Waiting" or dist > 2.2):
        yaw = math.atan2(d[0], d[1]); m = m + d / dist * min(4.0 * dt, dist)
    return m, yaw

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    m, yaw = np.array([0.0, 0.0]), 0.0
    g = Guide([0.8, 1.5], [0.0, 15.0]); timers = {"step": 0.0, "col": 0.0}
    plan = [
        ("Audio preview of Cookie as a guide dog. This is an offline mix using the game's real footstep and collar sounds and its real guide rules, with a simple stereo model, so use headphones.", 0, idle),
        ("Scene one. Marcus stands still, facing forward. Cookie walks ahead, then stops and waits for him. Listen for her collar rustling in front of you.", 8, idle),
        ("Scene two. Marcus follows her. She leads again whenever he catches up, until she reaches the objective.", 22, follow),
        ("Scene three. Cookie has arrived, and waits at the objective.", 5, idle),
        ("Scene four. Marcus wanders far away, to the left and behind. Cookie comes back for him, then leads again.", 9, None),
    ]
    chunks = []; pos = 0
    total = SR * 100; mix = np.zeros((total, 2), np.float32)
    for text, secs, policy in plan:
        v = say(text); mix[pos:pos + len(v), 0] += v * 0.9; mix[pos:pos + len(v), 1] += v * 0.9; pos += len(v) + int(0.6 * SR)
        if secs == 0: continue
        if policy is None:                                                # the teleport for the wandering scene
            m, yaw = np.array([-8.0, -2.0]), 0.0
            policy = idle
        m, yaw = render_scene(m, yaw, g, secs, policy, mix, pos, timers)
        print(f"scene ends: cookie={g.c.round(1)} marcus={m.round(1)} state={g.state}")
        pos += int(secs * SR) + int(1.2 * SR)
    mix = mix[:pos]; mix *= 0.9 / max(np.max(np.abs(mix)), 1e-6)
    wav = f"{OUT}/Cookie_guide_dog_audio_preview.wav"
    with wave.open(wav, "w") as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR); w.writeframes((mix * 32767).astype(np.int16).tobytes())
    subprocess.run(["afconvert", "-f", "m4af", "-d", "aac", "-b", "128000", wav, wav.replace(".wav", ".m4a")], check=True)
    print("wrote", wav, f"{pos / SR:.1f}s")
