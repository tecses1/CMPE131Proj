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

    public virtual void Render(IRenderContext ctx)
    {
        //image failed to load.
        if (myImage.Id == null)
        {
            //
            var previousFillStyle = ctx.FillStyle;
            var previousStrokeStyle = ctx.StrokeStyle;
            var previousLineWidth = ctx.LineWidth;
            var previousFont = ctx.Font;
            var previousTextAlign = ctx.TextAlign;
            var previousTextBaseline = ctx.TextBaseline;

            try
            {
                // Draw key background with rounded corners
                ctx.FillStyle = Settings.ErrorBackground;
                ctx.StrokeStyle = Settings.ErrorBorder;
                ctx.LineWidth = 2;


                ctx.BeginPath();
                ctx.RoundRect(this.x-sizeX/2, this.y-sizeY/2, sizeX, sizeY, 45);
                ctx.Fill();
                ctx.Stroke();

                // Draw character text
                ctx.Font = Settings.KeyFont;
                ctx.FillStyle = Settings.KeyText;
                ctx.TextAlign = TextAlign.Center;
                ctx.TextBaseline = TextBaseline.Middle;

                var textX = this.x;
                var textY = this.y;
                if (sizeX > 50)
                {
                    ctx.FillText("MISSING", textX, textY);
                }
                else
                {
                    ctx.FillText("!", textX, textY);
                }
                
                return;
            }
            finally
            {
                // Restore state manually for better performance
                ctx.FillStyle = previousFillStyle;
                ctx.StrokeStyle = previousStrokeStyle;
                ctx.LineWidth = previousLineWidth;
                ctx.Font = previousFont;
                ctx.TextAlign = previousTextAlign ?? TextAlign.Center;
                ctx.TextBaseline = previousTextBaseline ?? TextBaseline.Middle;
                
            }
        }

        AssetManager.DrawRotatedImage(ctx, myImage, x, y, sizeX,sizeY, rotation);
        
    }


}