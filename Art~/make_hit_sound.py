# Demo için vuruş sesi sentezler: python make_hit_sound.py <klasör>
import math, os, random, struct, sys, wave
RATE = 44100
out = sys.argv[1] if len(sys.argv) > 1 else "."
rng = random.Random(3)
total = int(0.22 * RATE)
y = 0.0
samples = []
for i in range(total):
    t = i / RATE
    y += 0.25 * (rng.uniform(-1, 1) - y)            # yumuşatılmış gürültü
    thump = math.sin(2 * math.pi * (140 - 300 * t) * t) * math.exp(-t * 30)
    snap = y * math.exp(-t * 60)
    fade = min(1.0, (total - i) / (0.02 * RATE))
    samples.append((0.9 * thump + 0.7 * snap) * fade)
peak = max(abs(s) for s in samples)
with wave.open(os.path.join(out, "Hit.wav"), "wb") as w:
    w.setnchannels(1); w.setsampwidth(2); w.setframerate(RATE)
    w.writeframes(b"".join(struct.pack("<h", int(s / peak * 0.85 * 32767)) for s in samples))
print("wrote Hit.wav")
