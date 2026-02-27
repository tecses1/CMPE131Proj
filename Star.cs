using Blazorex;

namespace CMPE131Proj;

public class Star : GameObject 
{
    
    public Star (GameManager gm) : base(ref gm)
    {
        Random r = new Random();
        this.sizeX = (int)(4 * r.NextDouble());
        this.sizeY = sizeX;
    }

    public override void Render(IRenderContext ctx)
    {
        
    }

    

}