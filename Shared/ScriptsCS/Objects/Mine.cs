    
namespace Shared;
using System;
using System.Numerics;  

public class Mine : Items
{
    public float speed = 0.3f;
    public int amount = 1; // How many mines
    public int LifetimeFrames = 3600; 
    public int damage = 500;
    public Mine(Transform t) : base(t) 
    {
    }

    public static Mine GenerateMine(Transform sourceTransform)
    {
    
        Transform t = new Transform(sourceTransform.rect.X, sourceTransform.rect.Y, 20, 20);
        Mine m = new Mine(t);

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