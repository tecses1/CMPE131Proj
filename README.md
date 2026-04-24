Placeholder README for now, NEEDS TO BE UPDATED

Overview:
  - .io game to fly around and shoot stuff
  - 
Prerequisite Requirements for download:
  - .NET 9 SDK  
  https://dotnet.microsoft.com/download
  - Windows OS (required for WPF server)
  - A modern web browser (Google Chrome recommended)
  - MAYBE?? vscode with C#, git


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

How to PLAY:
- WASD: Move
- Mouse: Aim
- Click: Shoot
- r to switch bullet type

Future TODOs:
  - implement score system
