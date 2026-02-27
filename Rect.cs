namespace CMPE131Proj;
using Microsoft.AspNetCore.Components;
using Blazorex;
public class Rect
{
    public string fillColor  = Settings.DefaultBackground;
    public string borderColor = Settings.DefaultBorder;
    
    public int borderWidth = 2;


    public float x;
    public float y;
    public int sizeX;
    public int sizeY;

    public Rect(float x, float y, int sizex, int sizeY)
    {
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

            try
            {
                // Draw key background with rounded corners
                ctx.FillStyle = this.fillColor;
                ctx.StrokeStyle = this.borderColor;
                ctx.LineWidth = 2;


                ctx.BeginPath();
                ctx.RoundRect(this.x-this.sizeX/2, this.y-this.sizeY/2, this.sizeX, this.sizeY, 0);
                ctx.Fill();
                ctx.Stroke();

            }
            finally
            {
                // Restore state manually for better performance
                ctx.FillStyle = previousFillStyle;
                ctx.StrokeStyle = previousStrokeStyle;
                ctx.LineWidth = previousLineWidth;
                
            }
    }
}