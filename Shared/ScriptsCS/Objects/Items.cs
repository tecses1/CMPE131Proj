using System;
using System.Numerics;
using Shared;
 
public class Items : GameObject
{
    public string itemType;
    public float speed = 0.3f;
    public int lifetimeFrames = 1000;

    public Items(Transform t, string s): base(t)
    {
        this.transform = t;
        this.itemType = s;

        this.transform.rect.Width = 20;
        this.transform.rect.Height = 20;

        this.transform.rotation = (float)(Random.Shared.NextDouble() * 360);
    }

    public void Update()
    {
        base.Update();

        lifetimeFrames--;
        if(lifetimeFrames <= 0)
        {
            this.Kill();
        }
    }

    public static Items GenerateItem(Transform sourceTransform, string s)
    {
    
        Transform t = new Transform(sourceTransform.rect.X, sourceTransform.rect.Y, 20, 20);
        Items i = new Items(t,s);

        double randomAngle = 2 * Math.PI * Random.Shared.NextDouble();
        Vector2 randDir = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

        i.transform.velocity = randDir * i.speed;

        return i;
    }
        
}