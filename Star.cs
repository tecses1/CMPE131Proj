using Blazorex;

namespace CMPE131Proj;

public class Star : GameObject 
{
    
    public Star (ref GameManager gm, Transform t) : base(ref gm,t )
    {
        
    }

    public override void Update()
    {
        Render();
        //Stars do not move, so we do not update their position. We just check if they are on screen to render them.
    }
}