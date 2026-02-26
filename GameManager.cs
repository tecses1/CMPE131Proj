using System;
using Blazorex;
namespace CMPE131Proj;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class GameManager
{
    public Settings settings;

    //pass by reference settigns object so all objects use the same one.
    public GameManager(ref Settings s)
    {
        this.settings = s;
    }
    //Add update function to call from main thread.
    public void Update()
    {
        
    }
    //Add render function to call from main thread.
    public void Render(IRenderContext ctx)
    {
                //fil bg.
        ctx.FillStyle = settings.CanvasBackground;
        ctx.FillRect(0, 0, settings.CanvasWidth, settings.CanvasHeight);
    }
}
