namespace CMPE131Proj;

using System.ComponentModel;
using Blazorex;
public class Text
{
    public string fillColor  = Settings.DefaultTextBackground;
    public string borderColor = Settings.DefaultTextBorder;
    
    public string font = Settings.DefaultFont;
    public string fontColor = Settings.DefaultFontColor;
    public string text = "Placeholder";
    public int borderWidth = 2;


    public float x;
    public float y;
    public int sizeX;
    public int sizeY;

    public Text(string text, float x, float y, int sizex, int sizeY)
    {
        this.text = text;
        this.x = x;
        this.y = y;
        this.sizeX = sizex;
        this.sizeY = sizeY;
    }
    //HUGE PERFORMANCE HIT. Idk why yet.

    public void Draw(IRenderContext ctx)
    {

            var previousFillStyle = ctx.FillStyle;
            var previousStrokeStyle = ctx.StrokeStyle;
            var previousLineWidth = ctx.LineWidth;
            var previousFont = ctx.Font;
            var previousTextAlign = ctx.TextAlign;
            var previousTextBaseline = ctx.TextBaseline;

            try
            {
                // Draw key background with rounded corners
                ctx.FillStyle = this.fillColor;
                ctx.StrokeStyle = this.borderColor;
                ctx.LineWidth = 2;


                ctx.BeginPath();
                ctx.RoundRect(this.x-sizeX/2, this.y-sizeY/2, sizeX, sizeY, 0);
                ctx.Fill();
                ctx.Stroke();

                // Draw character text
                ctx.Font = this.font;
                ctx.FillStyle = this.fontColor;
                //ctx.TextAlign = TextAlign.Center;
                ctx.TextBaseline = TextBaseline.Middle;

                var textX = this.x;
                var textY = this.y;
                ctx.FillText(text,x,y);
                
                return;
            }
            finally
            {
                // Restore state manually for better performance
                ctx.FillStyle = previousFillStyle;
                ctx.StrokeStyle = previousStrokeStyle;
                ctx.LineWidth = previousLineWidth;
                ctx.Font = previousFont;
                ctx.TextAlign = previousTextAlign ?? TextAlign.Center;
                ctx.TextBaseline = previousTextBaseline ?? TextBaseline.Middle;
                
            }
    }
}