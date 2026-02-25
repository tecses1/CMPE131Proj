namespace CMPE131Proj;
using System;
using Blazorex;
public  class GameMain
{
//Canvas size info. Determined in home.razor.cs, this is for reference only.
    public const int CanvasWidth = 1024;
    public const int CanvasHeight = 768;
// Styling constants
    private const string CanvasBackground = "#fff";
    private const string KeyBackground = "#2c3e50";
    private const string KeyBorder = "#34495e";
    private const string KeyText = "#ecf0f1";
    private const string KeyFont = "bold 18px 'Segoe UI', Arial, sans-serif";
    
//logic

    Player localPlayer;
    
    public GameMain()
    {
        localPlayer = new Player();
    }
    public void Update(InputWrapper e)
    {
        //Update game logic goes here.
        

        localPlayer.Update(e);


    }

    public void Render(IRenderContext ctx)
    {
        //fil bg.
        ctx.FillStyle = CanvasBackground;
        ctx.FillRect(0, 0, CanvasWidth, CanvasHeight);

        //Render logic.

        localPlayer.Render(ctx);
    }
}
