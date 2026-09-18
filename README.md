# 🕰️ MrDayNight
**by Genesis Project**

> *Take control of Valheim’s time itself — extend the light, embrace the dark.*

---

### 📜 Overview
**MrDayNight** is a simple yet powerful mod that lets you **control the duration of day and night** in Valheim.  
Customize how long the sun shines or how long the shadows last — whether you want endless daylight or a world trapped in eternal night.

Perfect for both **server administrators** and **solo adventurers**, it provides fine-grained control over the day/night cycle while staying fully synced across multiplayer worlds.

---

### ⚙️ Features
- ✅ Customize **day length** and **night length** individually (in seconds) — the
  proportion between them is really changed, not just the total cycle.
- ✅ Fully **synchronized across clients** when used on a server with [Jötunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/).  
- ✅ Automatically enforces **server settings** — players cannot override them.  
- ✅ Works in **single player** (local settings) and **multiplayer** (server-synced).  
- ✅ Config changes apply immediately, without restarting the game.
- ✅ Defaults reproduce vanilla exactly, so installing it changes nothing until
  you configure it.

---

### 🔍 How it works

Valheim has no "night length". It has a total cycle (`EnvMan.m_dayLengthSec`) and
a fixed proportion hidden inside `EnvMan.RescaleDayFraction`: the raw fraction
`0.15..0.85` is remapped to `0.25..0.75`, and `CalculateDay` / `CalculateNight`
compare against `0.25` and `0.75`. That is why vanilla is always **70% day and
30% night**, no matter how long the day is.

MrDayNight sets the total cycle to `DayLength + NightLength` **and** moves those
remapping boundaries so the proportion matches what you asked for. Night stays
centred on midnight — half at the end of the cycle, half at the start — which is
how the game already behaves.

Because the output contract is unchanged (`0.25..0.75` is still daytime), every
other system keeps working untouched: lighting, spawns, `IsDay()`, `IsNight()`,
`IsAfternoon()`. Sleeping is also corrected, so you wake up at the configured
sunrise instead of the hardcoded one.

---

### 🔀 Using it alongside world-tweak mods

Some mods also set the day length — **ZenWorldSettings**, for example, has its own
`Day Length Seconds`. MrDayNight loads after it and takes control of the cycle,
writing its value last. It says so in the log on startup, so you always know who
is in charge:

```
ZenWorldSettings 1.13.0 also controls day length ... MrDayNight takes over
```

If you would rather let the other mod drive, set `Tempo.Enabled = false` here and
MrDayNight steps aside completely — the game goes back to its original cycle.

---

### 🔧 Configuration

After launching the game once, the config file will be created at:
```
BepInEx/config/genesisproj.mrdaynight.cfg
```

Example:
```ini
[Tempo]
## Duração do dia em segundos (default = 1200)
# Setting type: Single
# Default value: 1200
DayLength = 1200

## Duração da noite em segundos (default = 600)
# Setting type: Single
# Default value: 600
NightLength = 600
```

---

### 🧩 Dependencies
| Mod | Required | Purpose |
|------|-----------|----------|
| [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) | ✅ | Core mod loader |
| [Jötunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/) | ✅ | Syncs configurations between server and clients |

---

### 🧠 How It Works
- When a world starts, MrDayNight hooks into the **EnvMan** initialization.
- The mod sets `EnvMan.m_dayLengthSec` based on your chosen values.
- On servers, these values are **propagated to all connected players** automatically.
- In single player, your local config values are used.

---

### 🏰 Server Behavior
| Environment | Behavior |
|--------------|-----------|
| **Dedicated server with Jötunn** | Server config is enforced on all clients. |
| **Single player** | Local configuration applies. |
| **Vanilla server (without Jötunn)** | Each client uses local config independently. |

---

### 🧑‍💻 Technical
- Built using **BepInEx 5.4+**, **HarmonyX**, and **Jötunn 2.26.1**.  
- Written in C# targeting **.NET Framework 4.8**.  
- Thread-safe, no reflection abuse, and uses clean Harmony patches.

---

### 📦 Installation
1. Install **BepInExPack for Valheim**.  
2. Install **Jötunn**.  
3. Extract the contents of this mod to:
   ```
   Valheim/BepInEx/plugins/
   ```
4. Launch the game.  
5. Configure and enjoy your custom day/night balance.

---

### 🧙‍♂️ Credits
- Developed by **BRKiHeL / Genesis Project**  
- Built with [Jötunn Modding Framework](https://github.com/Valheim-Modding/Jotunn)  
- Inspired by Valheim’s mystical world and community creativity

---

### 🐉 License
This project is open source under the MIT License.  
Feel free to modify or redistribute, crediting **Genesis Project**.
