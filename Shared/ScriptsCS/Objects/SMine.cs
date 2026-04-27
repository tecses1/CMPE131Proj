namespace Shared;
using System;
using System.Numerics;

public class SMine : GameObject
{
    public int lifetimeFrames = 3600;
    public float detect = 100f;
    public Guid owner;

    public SMine(Transform t) : base(t)
    {
        this.transform.rect.Width = 25;
        this.transform.rect.Height = 25;
    }

    public override void Update()
    {
        transform.Update();
        lifetimeFrames--;
        if (lifetimeFrames <= 0)
        {
            this.Kill();
            return;

        }
        if (this.gl == null){return;}

        Vector2 myPos = this.transform.GetPosition();
        foreach(GameObject obj in this.gl.GetActiveObjects())
        {
            if (obj is Enemy || obj is Player || obj is Asteroid)
            {
                if (Vector2.Distance(myPos, obj.transform.GetPosition()) <= detect)
                {
                    this.Kill();
                    break;
                }
            }
        }
    }

    public override void Kill()
    {
        Transform t = new Transform(this.transform.rect.X - 40, this.transform.rect.Y-40, 100, 100);

        Explosion SC = new Explosion(t, new Vector2(0,0), 1f);
        gl.AddGameObject(SC);

        base.Kill();
    }

}