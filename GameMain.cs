namespace CMPE131Proj;
using System;
using Blazorex;

//The "MAIN" File of the game. Should stay pretty simple as we use mostly
//classes for all sub functioanlities. But this is where it all starts (kind of)
public class GameMain
{
    public Settings settings;
    Player localPlayer;
    GameManager gameManager;
    

    //Called on intial startup of the game. Put all starting logic here!
    public GameMain(ref Settings s)
    {
        this.settings = s;
        localPlayer = new Player(ref s);
        gameManager = new GameManager(ref s);
    }
    //Called on each frame rendered in the game! 
    public void Update(InputWrapper e)
    {
        //Update game logic goes here.

        gameManager.Update();
        localPlayer.Update(e);


    }

    
    //Called after the update function from the main thread. Logic first, then render!
    public void Render(IRenderContext ctx)
    {

        gameManager.Render(ctx); 
        localPlayer.Render(ctx);
    }
}
