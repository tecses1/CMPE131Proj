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
    public void setBorderColor(Color c)
    {
        this.borderColor = ColorTranslator.ToHtml(c);
    }
    public void setBorderColor(Color c, int alpha)
    {
        this.borderColor = ColorTranslator.ToHtml(c);
        this.alpha = alpha;
    }
    public void setFillColor(Color c)
    {
        this.fillColor = ColorTranslator.ToHtml(c);
    }
    public void setFillColor(Color c, int alpha)
    {
        this.fillColor = ColorTranslator.ToHtml(c);
        this.alpha = alpha;
    }
}