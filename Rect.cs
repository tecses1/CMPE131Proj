namespace CMPE131Proj;

using System.Drawing;

public class Rect
{
    public string fillColor  = Settings.DefaultTextBackground;
    public string borderColor = Settings.DefaultTextBorder;
    
    public int borderWidth = 2;
    public int alpha = 255;

    public Transform transform;


    //for procedual cacheing.
    public Rect(ref Transform t)
    {
        this.transform = t;

    }
    public void Draw(GameManager gm)
    {
        gm.AddRectToRender(this);
           
    }
}