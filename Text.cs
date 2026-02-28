namespace CMPE131Proj;

using System.ComponentModel;
using System.Numerics;
using Blazorex;
public class Text
{
    public string fillColor  = Settings.DefaultTextBackground;
    public string borderColor = Settings.DefaultTextBorder;
    
    public string font = Settings.DefaultFont;
    public string fontColor = Settings.DefaultFontColor;
    public string text = "Placeholder";
    public int borderWidth = 2;


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
    //HUGE PERFORMANCE HItransform. Idk why yetransform. [FIXED]
    public void Draw(IRenderContext ctx)
    {

            ctx.Save();

            try
            {
                // Draw key background with rounded corners
                ctx.FillStyle = this.fillColor;
                ctx.StrokeStyle = this.borderColor;
                ctx.LineWidth = 2;


                ctx.BeginPath();
                ctx.RoundRect(this.transform.position.X-transform.size.X/2+offsetX, 
                                this.transform.position.Y-transform.size.Y/2+offsetY, 
                                transform.size.X, transform.size.Y, 0);
                ctx.Fill();
                ctx.Stroke();

                // Draw character text
                ctx.Font = this.font;
                ctx.FillStyle = this.fontColor;
                ctx.TextAlign = TextAlign.Center;
                ctx.TextBaseline = TextBaseline.Middle;

                ctx.FillText(text,transform.position.X+offsetX,transform.position.Y+offsetY);
                
                return;
            }
            finally
            {
                // Restore state manually for better performance
                ctx.Restore();
                
            }
    }
}