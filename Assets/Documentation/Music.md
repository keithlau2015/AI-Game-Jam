# BLOOD TIES: THE FAMILY LEDGER — Music / Soundtrack Specification

> **Generated with Strudel** (https://strudel.cc) — a live-coding environment for
> pattern-based music. All tracks below are **loopable** and are authored as
> Strudel REPL snippets so they can be dry-run in the browser *before* being
> committed to Unity as baked WAV/OGG assets.

---

## 1. Overview

This document defines the official soundtrack for **BLOOD TIES: THE FAMILY LEDGER**.
All music is procedural in source (Strudel) but is intended to be **rendered to
audio files** (`loopable WAV` or `OGG` recommended by Unity) and imported under
`Assets/Audio/Music/`.

### Tempo / Meter

- **Tempo:** 102 BPM — warm, unhurried, contemplative.
- **Strudel CPS equivalent:** `cps(102 / 60 / 4)` (quarter-note pulse).
- **Time signature:** 4/4 throughout.
- All loops are constructed to **seamlessly wrap** (bar-aligned phrase length).

### Loopability Requirement

Each game-state track below is a **complete looping bar group**. When rendering,
crossfade the tail into the head for a gapless loop. Unity settings that matter:

| Setting | Recommended Value |
| --- | --- |
| `AudioClip.Import Type` | Default |
| `Load Type` | Decompressed On Load (for music) |
| `Compression` | OGG, Quality ~50% |
| Force to mono | No (keep stereo) |
| Loop (Script) | `AudioSource.loop = true` |

---

## 2. Audio Pipeline (Strudel -> Unity)

1. In **Strudel REPL**, uncomment the track you want and click ▶ to audition.
2. Record/render the loop (Strudel can export audio; alternatively capture output).
3. Export to `Assets/Audio/Music/<TrackName>.wav` (or `.ogg`).
4. Apply the import settings from the table above.
5. Assign to the matching scene `AudioSource` (see §5 Mapping).

> **Note:** The Master merge should keep the existing `Assets/Audio/*.wav` SFX
> (Collectable, Death, Hurt, jump, LandOnEnemy, LandOnGround, Walk01, Walk02).
> The single existing `Music.wav` can be **replaced** by the new per-state tracks.

---

## 3. Game-State Tracks

### TRACK A — `Music_MainMenu` (Day Start)
**Mood:** Cozy, warm, gentle pixel-art nostalgia.
**Represents:** Soft paper-card UI, Hope (78/100), Shiba Inu Mascot Ribbon.
**Strudel source** (currently enabled as the default `$` pattern):

```js
$: stack(
  // Warm retro drums
  s("[bd <hh*2 oh>] [bd hh] [bd hh oh] [bd hh]")
    .bank("tr909").dec(0.25).gain(0.75),

  // Soft Marimba / Kalimba Arpeggio (Warmth & Daily Hope)
  n("<[e4 g4 b4 d5] [c4 e4 g4 b4] [a3 c4 e4 g4] [b3 d4 f#4 a4]>")
    .s("marimba")
    .struct("x x x x x x x x")
    .gain(0.65)
    .lpf(2200),

  // Gentle Bassline (Family Stability)
  n("<e2 c2 a1 b1>")
    .s("sawtooth")
    .legato(0.8)
    .lpf(400)
    .gain(0.55),

  // Pastel Chimes / Accent (UI Flourish)
  n("~ e5 ~ g5 ~ b5 ~ d6")
    .s("glockenspiel")
    .gain(0.3)
    .delay(0.25)
)
```

- **Instrument palette:** TR-909 drums, marimba, soft sawtooth bass, glockenspiel.
- **Use in:** Main Menu scene, Day Start banner, low-stress moments.

---

### TRACK B — `Music_DailyManagement` (Core Loop)
**Mood:** Contemplative, clock-ticking.
**Represents:** Household tasks (Water Garden, Read with Kids, Cook Dinner).
**Strudel source:**

```js
$: stack(
  // Ticking Time Widget / Flip Calendar Metronome
  s("[rim*2 hh] [rim hh] [rim*2 oh] [rim hh]")
    .bank("tr808").gain(0.6).lpf(3000),

  // Introspective Electric Piano / Rhodes Chords (The Ledger)
  n("<[e3,g3,b3,d4] [c3,e3,g3,b4] [a2,c3,e3,g3] [f#2,a2,c3,e3]>")
    .s("piano")
    .legato(0.9)
    .gain(0.7)
    .room(0.4),

  // Subtle Counter-Melody
  n("<[~ b4 g4 e4] [~ a4 e4 c4] [~ g4 e4 c4] [~ f#4 d4 b3]>")
    .s("vibraphone")
    .gain(0.4)
    .dec(0.6),

  // Walking Pulse (Daily Progress)
  n("e2 e2 c2 c2 a1 a1 b1 b1")
    .s("triangle")
    .gain(0.5)
)
```

- **Instrument palette:** TR-808 rims/hats, Rhodes-style piano, vibraphone, triangle.
- **Use in:** Daily management / core gameplay loop.

---

### TRACK C — `Music_Crisis` (High Stress)
**Mood:** Pressured, unstable, broken harmony.
**Represents:** Stress Meter (32/100 → spiking), Conflict, Arguments.
**Strudel source:**

```js
$: stack(
  // Urgent Driving Beat
  s("[bd bd] [sn hh*2] [bd <oh oh*2>] [sn hh]")
    .bank("tr909").gain(0.85).dec(0.15),

  // Dissonant Synth Pluck
  n("<[e3 g3 b3 c#4] [f3 g#3 c4 d4] [e3 g3 b3 d#4] [d#3 f#3 a3 c4]>")
    .s("sawtooth")
    .struct("x(5,8)")
    .lpf(1200)
    .gain(0.6)
    .room(0.2),

  // Tense Low End
  n("<e1 f1 e1 d#1>")
    .s("square")
    .gain(0.6)
    .distort(0.2)
)
```

- **Instrument palette:** TR-909 drums, distorted sawtooth plucks, tense square bass.
- **Use in:** High-stress states, conflict/argument scenes, rising danger.

---

### TRACK D — `Music_NightReflection` (Evening / Night Phase)
**Mood:** Intimate, heavy, emotional.
**Represents:** Ledger entries, secrets uncovered, Elena's burdens.
**Strudel source:**

```js
$: stack(
  // Minimal Distant Heartbeat
  s("bd ~ ~ bd ~ ~ ~ ~")
    .bank("tr808").lpf(200).gain(0.8),

  // Solitary Soft Piano
  n("<[e3 g3 b3] [c3 e3 a3] [a2 c3 e3] [b2 d#3 f#3]>")
    .s("piano")
    .slow(2)
    .gain(0.75)
    .room(0.8)
    .sz(0.8),

  // High Whistle / Glass Harmonics
  n("e6 ~ b5 ~ g5 ~ d6 ~")
    .s("sine")
    .slow(2)
    .gain(0.15)
    .delay(0.5)
)
```

- **Instrument palette:** TR-808 kick (heartbeat), soft piano, airy sine whistle.
- **Use in:** Evening/night phase, introspection, heavy narrative beats.

---

## 4. Sound Effects & Transitions (Jingles)

These are short one-shots, not loops. Render each to a single file under
`Assets/Audio/` alongside existing SFX.

### SFX 1 — `Sfx_TaskComplete` (Task Complete / Item Purchased)
```js
$: n("e5 g5 b5 e6")
  .s("glockenspiel")
  .fast(4)
  .gain(0.7)
  .decay(0.3)
  .room(0.5)
```
**Mood:** Bright, satisfying, positive resolution.

---

### SFX 2 — `Sfx_DayAdvance` (Level Up / Day Advance)
```js
$: stack(
  n("c4 e4 g4 c5 e5 g5 c6")
    .s("marimba")
    .fast(6)
    .gain(0.8),
  s("oh")
    .bank("tr909")
    .gain(0.4)
)
```
**Mood:** Ascending victory, new-day fanfare.

---

### SFX 3 — `Sfx_HealthCrisis` (Health Crisis / Resource Exhaustion)
```js
$: stack(
  n("b4 f4 d4 g#3")
    .s("sawtooth")
    .fast(3)
    .lpf(800)
    .gain(0.7)
    .dec(0.8),
  s("sn")
    .bank("tr808")
    .gain(0.8)
    .room(0.6)
)
```
**Mood:** Dissonant alarm, warning, crisis.

---

### SFX 4 — `Sfx_GameOver` (Game Over / Family Dissolution)
```js
$: stack(
  n("e3 c3 a2 e2")
    .s("piano")
    .slow(1.5)
    .gain(0.8)
    .room(0.9),
  n("e2")
    .s("square")
    .legato(2)
    .lpf(200)
    .gain(0.5)
)
```
**Mood:** Heavy, somber, finality.

---

## 5. Unity Scene → Track Mapping

| Unity Scene / State | Track | Clip File |
| --- | --- | --- |
| Main Menu | A — Main Menu / Day Start | `Music_MainMenu.wav` |
| Daily management loop | B — Daily Management | `Music_DailyManagement.wav` |
| High stress / crisis | C — Crisis | `Music_Crisis.wav` |
| Evening / night phase | D — Night Reflection | `Music_NightReflection.wav` |
| Task complete / item purchased | SFX 1 | `Sfx_TaskComplete.wav` |
| Level up / day advance | SFX 2 | `Sfx_DayAdvance.wav` |
| Health crisis / resource exhausted | SFX 3 | `Sfx_HealthCrisis.wav` |
| Game over / family dissolution | SFX 4 | `Sfx_GameOver.wav` |

### Recommended Unity Setup
- Attach a dedicated `AudioSource` per scene for music (persistent across
  scene loads if desired — e.g. a `DontDestroyOnLoad` music manager).
- Crossfade between Track B ↔ C based on the **Stress Meter** value
  (e.g. blend toward `Music_Crisis.wav` as stress exceeds ~60%).
- Use a single `AudioMixer` group per bus: `Master` / `Music` / `SFX` so the
  master can duck music under SFX and vice-versa.

---

## 6. Full Strudel Source (Reference)

The canonical, commented Strudel file (as used to generate all content) is
provided in the PR as `Assets/Documentation/StrudelMusic.txt` (or referenced
directly from this document). To audition any track:

1. Open https://strudel.cc
2. Paste the desired section's code.
3. Uncomment the `$:` / content and press ▶.
4. Toggle `cps(102 / 60 / 4)` as needed.

> **Merge note for Master:** All four game-state tracks share 102 BPM & 4/4 so
> they can be dynamically crossfaded in-engine without tempo mismatches.
