    
namespace Shared;
using System;
using System.Numerics;  

public class MineAmmo : Items
{
    public float speed = 0.3f;
    public int amount = 1; // How many mines
    public int LifetimeFrames = 3600; 
    public int damage = 500;
    public MineAmmo(Transform t) : base(t) 
    {
    }

    public static MineAmmo GenerateMineAmmo(Transform sourceTransform)
    {
    
        Transform t = new Transform(sourceTransform.rect.X, sourceTransform.rect.Y, 30, 30);
        MineAmmo m = new MineAmmo(t);

        double randomAngle = 2 * Math.PI * Random.Shared.NextDouble();
        Vector2 randDir = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

        m.transform.velocity = randDir * m.speed;

        return m;
    }

    public override void Collect(Player p)
    {
        p.mines += 1;
    }
}