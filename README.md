Placeholder README for now, NEEDS TO BE UPDATED

Overview:
  - .io game to fly around and shoot stuff

Prerequisite Requirements for download:
  - .NET 9 SDK  
  https://dotnet.microsoft.com/download
  - Windows OS (required for WPF server)
  - A modern web browser (Google Chrome recommended)
  - MAYBE?? vscode with C#, git
  - BlazorEX


How to Build:
  - try f5, if does not work
  - step 1: start server

   dotnet restore                                   
   dotnet build
   dotnet run --project .\ServerSideStandalone\

  - step 2 in separate terminal: start the client

    dotnet restore  
    dotnet build
    dotnet run --project .\ClientSideWASM\

  - step 3: open browser to http://localhost:5152/

How to Start a game:
- Open Settings to set your name
- Click play to navigate to choosing lobby
- Can either create lobby or join lobby
   - For both options enter the desired lobby name
   - Create will create a lobby of that name that others on the server can join by entering the same lobby name (case sensative)
   - Join will join the lobby of that name if it exists, or say failed otherwise
   - Launch to start the game!!

How to Play:
- WASD key: Move
- Mouse: Aim
- Left Click: Shoot regular projectile
- Space Bar: Missile
- r key: switch bullet type (regular projectile, rapid fire projectiles, tracking missiles)
- Fight other players, enemies, and aliens
- Avoid out of bounds and taking too much damage or you will die (but respwan soon to jump back in the action)
- Destroy asteroids to drop health packs and missile ammo!
- Get a high score!

Future TODOs:
- 