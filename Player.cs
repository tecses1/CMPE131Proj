namespace CMPE131Proj;

using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;
//Handles the local player controller.
public class Player
{
    //move comonly defined fields for classes to the GameObject class.
    //Game object class may be able to handle default rending, image fetching by name, etc.
    int x;
    int y;

    int sizeX = 100;
    int sizeY = 100;
    
    float rotation;
    ElementReference myImage; 

    private readonly List<Projectile> projectiles = new();
    private readonly float bulletSpeed = 12f;
    private readonly double shotCooldownSeconds = 0.12; // ~8 shots/sec
    private DateTime lastShotTime = DateTime.MinValue;
    public Player()
    {
        x = Settings.CanvasWidth /2;
        y = Settings.CanvasHeight / 2;
        myImage = AssetManager._assets["Player"].Image;
    }
    public void Update(InputWrapper e)
    {
        //WORSHIP THY ROTATION
        //THIS WAS THE HARDEST THING I'VE HAD TO DO SO FAR ON THIS PROJECT

    
        float mouseX = (float)e.MouseX;
        float mouseY = (float)e.MouseY;

        Vector2 mousePos = new Vector2(mouseX, mouseY);
        Vector2 myPos = new Vector2(x, y);

        Vector2 viewDirection = (mousePos - myPos);
        if (viewDirection.LengthSquared() == 0f)
            viewDirection = new Vector2(0, -1);

        double angleRadiansView = Math.Atan2(viewDirection.X, viewDirection.Y);
        double angleDegrees = (angleRadiansView) * (180.0 / Math.PI);
        rotation = -(float)angleDegrees + 180;

        if (e.keys[0]) y -= 3;
        if (e.keys[1]) x -= 3;
        if (e.keys[2]) y += 3;
        if (e.keys[3]) x += 3;

        if (x > Settings.CanvasWidth + sizeX / 2) x = -sizeX / 2;
        if (x < -sizeX / 2) x = Settings.CanvasWidth + sizeX / 2;
        if (y > Settings.CanvasHeight + sizeY / 2) y = -sizeY / 2;
        if (y < -sizeY / 2) y = Settings.CanvasHeight + sizeY / 2;
    
        bool shotEdge = e.LeftPressed;
        bool canShoot = (DateTime.UtcNow - lastShotTime).TotalSeconds >= shotCooldownSeconds;

        if (shotEdge && canShoot)
        {
            SpawnProjectileTowards(mousePos);
            lastShotTime = DateTime.UtcNow;
        }

        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            projectiles[i].Update();
            if (projectiles[i].Dead)
                projectiles.RemoveAt(i);
        }
    }

    private void SpawnProjectileTowards(Vector2 target)
    {
        Vector2 myPos = new Vector2(x, y);
        Vector2 dir = target - myPos;
        if (dir.LengthSquared() == 0f) dir = new Vector2(0, -1);
        dir = Vector2.Normalize(dir);

        float spawnOffset = MathF.Max(20, MathF.Min(sizeX, sizeY) / 2f + 4);
        Vector2 spawnPos = myPos + dir * spawnOffset;
        Vector2 velocity = dir * bulletSpeed;

        var proj = new Projectile(spawnPos.X, spawnPos.Y, velocity, rotation, lifetime: 240);
        projectiles.Add(proj);
    }


    public void Render(IRenderContext ctx)
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].Render(ctx);
        }

        ctx.Font = Settings.KeyFont;
        ctx.FillStyle = Settings.KeyText;
        ctx.TextAlign = TextAlign.Center;
        ctx.TextBaseline = TextBaseline.Middle;

        var textX = this.x;
        var textY = this.y -sizeY/2;

        ctx.FillText(Settings.name, textX, textY);
       AssetManager.DrawRotatedImage(ctx,myImage,x,y,sizeX,sizeY,rotation);

        /* DEBUG DOT
        ctx.FillStyle = Settings.KeyBackground;
        ctx.StrokeStyle = Settings.KeyBorder;
        ctx.LineWidth = 2;
        ctx.BeginPath();
        ctx.RoundRect(x, y, 5, 5, 180);
        ctx.Fill();
        ctx.Stroke();
        */
        /*

        SAVE THIS JUNK CODE FOR PROCEDUAL RENDER REFERENCE

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

            ctx.FillText(Settings.name, textX, textY);
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
        */
    }
}
