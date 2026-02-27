using System;
using Blazorex;
namespace CMPE131Proj;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class GameManager
{
    //pass by reference settigns object so all objects use the same one.
    public GameManager()
    {

    }
    //Add update function to call from main thread.
    public void Update()
    {
        
    }

    void Stars(IRenderContext ctx)
    {
        
    }
    //Add render function to call from main thread.
    public void Render(IRenderContext ctx)
    {
                //fil bg.
        ctx.FillStyle = Settings.CanvasBackground;
        ctx.FillRect(0, 0, Settings.CanvasWidth, Settings.CanvasHeight);

        
    }
}
