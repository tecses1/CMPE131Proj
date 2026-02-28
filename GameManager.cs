using System;
using Blazorex;
namespace CMPE131Proj;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class GameManager
{
    //pass by reference settigns object so all objects use the same one.
    Player localPlayer;
    
    int worldSizeX = 2000;
    int worldSizeY = 2000;

    int worldPosX = 0;
    int worldPosY = 0;

    List<GameObject> activeObjects = new List<GameObject>();
    private List<GameObject> objsToRemove = new List<GameObject>();

    public GameManager()
    {
        GameManager reference = this;
        localPlayer = new Player(ref reference, new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight/2, 60,60,0));
    }
    //Add update function to call from main thread.
    public void UpdatePlayer(InputWrapper e)
    {
        localPlayer.Update(e);
    }
    void Stars(IRenderContext ctx)
    {

        
    }
    //Returns Canvas bounds. USeful for seeign if an object moves outside of bounds, within reason.
    public float[] GetBounds()
    {
        float[] bounds = new float[4];
        bounds[0] = 0;
        bounds[1] = 0;
        bounds[2] = Settings.CanvasWidth;
        bounds[3] = Settings.CanvasHeight;

        return bounds;

    }
    //Add render function to call from main thread.
    public void Render(IRenderContext ctx)
    {
                //fil bg.
        ctx.FillStyle = Settings.CanvasBackground;
        ctx.FillRect(0, 0, Settings.CanvasWidth, Settings.CanvasHeight);

        foreach (GameObject go in activeObjects)
        {
            go.UpdateAndRender(ctx);
        }
        foreach (GameObject go in objsToRemove)
        {
            activeObjects.Remove(go);
        }
        objsToRemove.Clear();

        localPlayer.UpdateAndRender(ctx);

        

        
    }

    public void AddNewGameObject(GameObject o)
    {
        this.activeObjects.Add(o);
    }
    public void RemoveGameObject(GameObject o)
    {
        objsToRemove.Add(o);
    }
}
