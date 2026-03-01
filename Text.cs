namespace CMPE131Proj;

using System.Drawing;

public class Text
{
    public string fillColor  = Settings.DefaultTextBackground;
    public string borderColor = Settings.DefaultTextBorder;
    
    public string font = Settings.DefaultFont;
    public string fontColor = Settings.DefaultFontColor;
    public string text = "Placeholder";
    public int borderWidth = 2;

    public int rectAlpha = 0;
    public int textAlpha = 255;
    public Transform transform;

    public float offsetX;
    public float offsetY;


    //for procedual cacheing.
    public Text(string text, ref Transform t, float offsetX = 0, float offsetY = 0)
    {
        this.transform = t;
        this.text = text;
        this.offsetX = offsetX;
        this.offsetY = offsetY;

    }
    public void Draw(GameManager gm)
    {
        gm.AddTextToRender(this);
           
    }
}