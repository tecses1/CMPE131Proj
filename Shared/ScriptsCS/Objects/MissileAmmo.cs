    
namespace Shared;
using System;
using System.Numerics;  

public class MissileAmmo : Items
{
    public float speed = 0.3f;
    public int amount = 1; // How many missiles it gives
    public int LifetimeFrames = 3600; 
    
    public MissileAmmo(Transform t) : base(t) 
    {
    }

    public static MissileAmmo GenerateMissileAmmo(Transform sourceTransform)
    {
    
        Transform t = new Transform(sourceTransform.rect.X, sourceTransform.rect.Y, 20, 20);
        MissileAmmo m = new MissileAmmo(t);

        double randomAngle = 2 * Math.PI * Random.Shared.NextDouble();
        Vector2 randDir = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

        m.transform.velocity = randDir * m.speed;

        return m;
    }

    public override void Collect(Player p)
    {
        p.MissileAmmo += 1;
    }
}