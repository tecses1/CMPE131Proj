using Blazorex;
using Microsoft.AspNetCore.Components;

namespace CMPE131Proj;


public class GameObject
{
    //positions.
    protected float x;
    protected float y;

    //sizing
    protected int sizeX;
    protected int sizeY;

    protected float rotation;
    private ElementReference myImage;

    protected GameManager gm;

    public GameObject(ref GameManager gm)
    {
        this.gm  = gm;
    
        GameAsset myImageAsset;
        Console.WriteLine("NAME = " + this.GetType().Name);
        if(AssetManager._assets.TryGetValue(this.GetType().Name, out myImageAsset))
        {

            myImage = myImageAsset.Image;
        }
        else
        {
            Console.WriteLine("Image returned null. " + this.GetType().Name + " was not able to find its image.");
        }
        
        
    }
    public bool CollideWith(GameObject two)
    {
        float[] myBounds = this.GetBounds();
        float[] otherBounds = two.GetBounds();
        return myBounds[0] < otherBounds[2] && // Rect1 Left < Rect2 Right
           myBounds[2] > otherBounds[0] && // Rect1 Right > Rect2 Left
           myBounds[1] < otherBounds[3] && // Rect1 Top < Rect2 Bottom
           myBounds[3] > otherBounds[1];   // Rect1 Bottom > Rect2 Top
    }

    public float[] GetBounds()
    {
        //because all images draw cenetered, we need a bounds rect.
        //We get the top left point, top right point, and the bottom left point, bottom right point. 
        float[] bounds = new float[4];
        bounds[0] = x - sizeX/2;
        bounds[1] = y - sizeY/2;
        bounds[2] = x + sizeX/2;
        bounds[3] = y + sizeY/2;

        return bounds;

    }


    public virtual void Render(IRenderContext ctx)
    {
        //image failed to load.
        if (myImage.Id == null)
        {

            Rect drawRect = new Rect(this.x, this.y, sizeX, sizeY);
            drawRect.borderColor = Settings.ErrorBorder;
            drawRect.fillColor = Settings.ErrorBackground;
            drawRect.Draw(ctx);

            Text drawText = new Text("Missing",this.x, this.y, this.sizeX,this.sizeY);
            drawText.fontColor = Settings.ErrorText;
            if (sizeX < 50)
            {
                drawText.text = "!";
            }

            drawText.Draw(ctx);
            return;
        

        }

        AssetManager.DrawRotatedImage(ctx, myImage, x, y, sizeX,sizeY, rotation);
        
    }


}