namespace CMPE131Proj;

using System.Dynamic;
using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;
//Handles the local player controller.
public class Player : GameObject
{
    //move comonly defined fields for classes to the GameObject class.
    //Game object class may be able to handle default rending, image fetching by name, etc.

    private readonly List<Projectile> projectiles = new();
    private readonly float bulletSpeed = 12f;
    private readonly double shotCooldownSeconds = 0.12; // ~8 shots/sec
    private DateTime lastShotTime = DateTime.MinValue;
    public Player(ref GameManager gm) : base(ref gm)
    {
        this.sizeX = 100;
        this.sizeY = 100;
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

        var proj = new Projectile(ref gm, spawnPos.X, spawnPos.Y, velocity, rotation, lifetime: 240);
        projectiles.Add(proj);
    }

    
    public override void Render(IRenderContext ctx)
    {
        //game object has built in default rendering.
        base.Render(ctx);
        
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


    }
}
