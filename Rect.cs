namespace CMPE131Proj;
using Microsoft.AspNetCore.Components;
using Blazorex;
using System.Numerics;

public class Rect
{
    public string fillColor  = Settings.DefaultBackground;
    public string borderColor = Settings.DefaultBorder;
    
    public int borderWidth = 2;


    public Transform transform;


    //for procedual cacheing
    public Rect(ref Transform t)
    {
        this.transform = t;
    }
    //HUGE PERFORMANCE HIT. Idk why yet.
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
                ctx.RoundRect(this.transform.position.X-this.transform.size.X/2, 
                                this.transform.position.Y-this.transform.size.Y/2, 
                                this.transform.size.X, this.transform.size.Y, 10);
                ctx.Fill();
                ctx.Stroke();

            }
            finally
            {
                // Restore state manually for better performance
                ctx.Restore();
                
            }
    }
}