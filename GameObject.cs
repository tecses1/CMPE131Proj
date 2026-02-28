using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;

namespace CMPE131Proj;


public class GameObject
{


    //sizing
    public Transform transform;

    private ElementReference myImage;

    protected GameManager gm;

    protected Rect drawRect;
    protected Text drawText;

    protected bool hasImage;
    public GameObject(ref GameManager gm, Transform transform)
    {
        this.gm  = gm;
        this.transform = transform;
        GameAsset myImageAsset;
        hasImage = AssetManager._assets.TryGetValue(this.GetType().Name, out myImageAsset);
        Console.WriteLine("NAME = " + this.GetType().Name);

        
        if(hasImage)
        {
            myImage = myImageAsset.Image;
        }else
        {
            Console.WriteLine("Image returned null. " + this.GetType().Name + " was not able to find its image.");

            
            this.drawRect = new Rect(ref transform);
            drawRect.borderColor = Settings.ErrorBorder;
            drawRect.fillColor = Settings.ErrorBackground;

            this.drawText = new Text("Missing", ref transform);
            drawText.fontColor = Settings.ErrorText;
            if (transform.size.X < 50)
            {
                drawText.text = "!";
            }

            Console.WriteLine("Procedual error images generated...");
        }

        
        
    }
    public ElementReference getImage()
    {
        return this.myImage;
    }
    public bool CollideWith(GameObject two)
    {//Collid with other gameobject.
        float[] myBounds = this.GetBounds();
        float[] otherBounds = two.GetBounds();
        return myBounds[0] < otherBounds[2] && // Rect1 Left < Rect2 Right
           myBounds[2] > otherBounds[0] && // Rect1 Right > Rect2 Left
           myBounds[1] < otherBounds[3] && // Rect1 Top < Rect2 Bottom
           myBounds[3] > otherBounds[1];   // Rect1 Bottom > Rect2 Top
    }
    
    public bool CollideWith(float[] two)
    {//Collide with rect
        float[] myBounds = this.GetBounds();
        float[] otherBounds = two;//.GetBounds();
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
        bounds[0] = transform.position.X - transform.size.X/2;
        bounds[1] = transform.position.Y - transform.size.Y/2;
        bounds[2] = transform.position.X + transform.size.X/2;
        bounds[3] = transform.position.Y+ transform.size.Y/2;

        return bounds;

    }

    // Instead of drawing, it just packs its data into the array
    public void PackData(float[] buffer, int offset)
    {
        buffer[offset] = transform.position.X;
        buffer[offset + 1] = transform.position.Y;
        buffer[offset + 2] = transform.size.X;
        buffer[offset + 3] = transform.size.Y;
        buffer[offset + 4] = transform.rotation * (float)Math.PI / 180f; // Convert to radians here
    }
    public virtual void Update()
    {

        
    }
    //used for procedual rendering, or alternate rendering other then image cacheing.
    public virtual void Render(IRenderContext ctx)
    {

        
    }
    public void Kill()
    {
        gm.RemoveGameObject(this);
        
    }
}