using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;

namespace CMPE131Proj;


public class GameObject
{


    //sizing
    public Transform transform;

    protected GameManager gm;
    public GameObject(ref GameManager gm, Transform transform)
    {
        this.gm  = gm;
        this.transform = transform;

        
        
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
    public void Render()
    {
        if (this.CollideWith(gm.GetCanvasBounds()))
        {
            gm.objsToRender.Add(this);
        }
    }
    public virtual void Update()
    {
        //if we can SEE the object, add it to render pipeline.

    }
    //used for procedual rendering, or alternate rendering other then image cacheing.
    public void Kill()
    {
        gm.RemoveGameObject(this);
        
    }
}