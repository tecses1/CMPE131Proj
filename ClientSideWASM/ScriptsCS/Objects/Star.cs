

namespace Shared;

public class Star : GameObject 
{
    
    public Star (Transform t) : base(t )
    {
        //render is skipped on gameobjects without previous trasnforms... even if they're static.
        this.previousTransform = t;
    }

    public override void Update()
    {
        //Stars do not move, so we do not update their position. We just check if they are on screen to render them.
    }
}