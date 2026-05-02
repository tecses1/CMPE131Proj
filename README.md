# 🚀 .io Space Shooter Game

A fast-paced browser-based **.io-style multiplayer game** where you fly around, battle enemies, and compete for the highest score.

---

## 🧰 Prerequisites

Before running the project, make sure you have the following installed:

* [.NET 9 SDK](https://dotnet.microsoft.com/download)
* Windows OS *(required for WPF server support)*
* A modern web browser (recommended: Google Chrome)
* Visual Studio Code with C# extension
* Blazor (BlazorEX)

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
