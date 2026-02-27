namespace CMPE131Proj;
using Blazorex;
//Handles the local player controller.
public class Player
{
    int x;
    int y;
    //pass by reference settigns object so all objects use the same one.
    public Player()
    {
    }
    public void Update(InputWrapper e)
    {

        if (e.keys[0])
        {
            y -= 3;
        }
        if (e.keys[1])
        {
            x -= 3;
        }
        if (e.keys[2])
        {
            y += 3;
        }
        if (e.keys[3])
        {
            x += 3;
        }
        //hard code stinks. will fix later. allows teleporting to other side of screen.
        if (x > 1024)
        {
            x = -50;
        }
        if (x < -50)
        {
            x = 1024;
        }

        if (y > 768)
        {
            y = -50;
        }
        if (y < -50)
        {
            y = 768;
        }
    }

    public void Render(IRenderContext ctx)
    {
                // Performance: Avoid Save/Restore for better performance
        // Store current state
        var previousFillStyle = ctx.FillStyle;
        var previousStrokeStyle = ctx.StrokeStyle;
        var previousLineWidth = ctx.LineWidth;
        var previousFont = ctx.Font;
        var previousTextAlign = ctx.TextAlign;
        var previousTextBaseline = ctx.TextBaseline;

        try
        {
            // Draw key background with rounded corners
            ctx.FillStyle = Settings.KeyBackground;
            ctx.StrokeStyle = Settings.KeyBorder;
            ctx.LineWidth = 2;

            var keyLeft = this.x;
            var keyTop = this.y;

            ctx.BeginPath();
            ctx.RoundRect(keyLeft, keyTop, 50, 50, 180);
            ctx.Fill();
            ctx.Stroke();

            // Draw character text
            ctx.Font = Settings.KeyFont;
            ctx.FillStyle = Settings.KeyText;
            ctx.TextAlign = TextAlign.Center;
            ctx.TextBaseline = TextBaseline.Middle;

            var textX = this.x + 25;
            var textY = this.y + 25;

            ctx.FillText("Player", textX, textY);
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
