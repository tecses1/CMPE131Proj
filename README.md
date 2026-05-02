# GALAX.IO

# 🚀 The Best .io Space Shooter Game

A fast-paced browser-based **.io-style multiplayer game** where you fly around, battle enemies, and compete for the highest score.

---

## 🧰 Prerequisites

Before running the project, make sure you have the following installed:

* [.NET 9 SDK](https://dotnet.microsoft.com/download)
* A modern web browser (recommended: [Google Chrome](https://www.google.com/chrome/dr/download/?brand=OZZY&ds_kid=10484928882&gclsrc=aw.ds&gad_source=1&gad_campaignid=21457145903&gbraid=0AAAAAoY3CA7X0mi2E2s91LiaEzeji_ufP&gclid=EAIaIQobChMI15XYuOeZlAMVGQCtBh18ExB9EAAYASAAEgK3EPD_BwE))
* [Visual Studio Code](https://code.visualstudio.com/) with C# extension
* [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
* [BlazorEX](https://github.com/mizrael/Blazorex)

---

## 🛠️ Build & Run Instructions

If pressing **F5** doesn’t work, follow these steps:

### 1. Start the Server

```bash
dotnet restore
dotnet build
dotnet run --project .\ServerSideStandalone\
```

---

### 2. Start the Client (in a separate terminal)

```bash
dotnet restore
dotnet build
dotnet run --project .\ClientSideWASM\
```

---

### 3. Open the Game

Navigate to:

```
http://localhost:5152/
```

---
## 🌐 Playing with Others (Multiplayer Setup)

By default, the game runs on:

```

[http://localhost:5152/](http://localhost:5152/)

```

**Important:**  
`localhost` only works on the same machine running the server. Other devices cannot access it directly.

---

### 🖥️ Play on Another Device (Same Network)

To allow other devices (e.g., friends on the same Wi-Fi) to join:

1. Find your computer’s local IP address  
   Example: `192.168.1.25`

2. Start the server so it listens on your network:

```

dotnet run --project .\ServerSideStandalone\ --urls "[http://0.0.0.0:5152](http://0.0.0.0:5152)"

```

3. Open port **5152** in your firewall (Windows Defender Firewall settings)

4. On another device, open a browser and go to:

```

http://<your-local-ip>:5152/

```

Example:

```

[http://192.168.1.25:5152/](http://192.168.1.25:5152/)

```

## 🎮 How to Start a Game

1. Open **Settings** and set your player name
2. Click **Play** to access the lobby screen
3. Choose one of the following:

   * **Create Lobby**

     * Enter a lobby name *(case-sensitive)*
     * Others can join using the same name
   * **Join Lobby**

     * Enter an existing lobby name
     * You’ll get an error if it doesn’t exist
4. Click **Launch** to begin!

---

## 🕹️ Controls

* **W / A / S / D** → Move
* **Mouse** → Aim
* **Left Click** → Fire standard projectile
* **Spacebar** → Launch missile
* **R** → Switch weapon type:

  * Standard projectile
  * Rapid-fire
  * Tracking missiles

---

## 💥 Gameplay Tips

* Battle **players, enemies, and aliens**
* Avoid going out of bounds
* Manage your health — death = respawn shortly after
* Destroy **asteroids** to collect:
  * ❤️ Health packs
  * 🚀 Missile ammo
* Aim for the **highest score**

---

## 📌 Future Improvements

* Add more weapon types
* Improve matchmaking/lobby system
* Add power-ups and abilities
* Enhance UI/UX
