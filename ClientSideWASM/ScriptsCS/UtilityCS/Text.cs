namespace ClientSideWASM;

using System.Drawing;
using System.Numerics;
using Shared;

public class Text : Rect
{

    
    public string font = Settings.DefaultFont;
    public string fontColor = Settings.DefaultFontColor;
    public string text = "Placeholder";
    public int textAlpha = 255;

    public int fontSize = 16;

    public float offsetX;
    public float offsetY;
    
    public bool fillToRect = true; //if true, the text will be stretched to fit the rect. If false, it will be drawn at its normal size with the center of the rect's position.



    public Text(string text, Transform t, float offsetX = 0, float offsetY = 0) : base( t)
    {
        this.transform = t;
        this.text = text;
        this.offsetX = offsetX;
        this.offsetY = offsetY;

        //text by default has no fill!
        this.fillAlpha = 0;
        this.borderAlpha = 0;

    }
    public override void Draw(RenderManager rm)
    {
        rm.AddTextToRender(this);
           
    }

    public void setTextColor(Color c,int alpha)
    {
        this.fontColor = ColorTranslator.ToHtml(c);
        this.textAlpha = alpha;
    }

}