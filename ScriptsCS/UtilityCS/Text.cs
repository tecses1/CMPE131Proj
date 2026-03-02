namespace CMPE131Proj;

using System.Drawing;
using System.Numerics;

public class Text : Rect
{

    
    public string font = Settings.DefaultFont;
    public string fontColor = Settings.DefaultFontColor;
    public string text = "Placeholder";

    public int rectAlpha = 0;
    public int textAlpha = 255;

    public float offsetX;
    public float offsetY;



    //for procedual cacheing.
    public Text(string text, ref Transform t, float offsetX = 0, float offsetY = 0) : base(ref t)
    {
        this.transform = t;
        this.text = text;
        this.offsetX = offsetX;
        this.offsetY = offsetY;

    }
    public override void Draw(GameManager gm)
    {
        gm.AddTextToRender(this);
           
    }

    public void setTextColor(Color c,int alpha)
    {
        this.fontColor = ColorTranslator.ToHtml(c);
        this.textAlpha = alpha;
    }

}