namespace Shared;
using System;
using System.Numerics;

public class SMine : GameObject
{
    private int armingFrames = 60;
    public int hp = 5;
    public int lifetimeFrames = 90;
    //public float detect = 10f;
    public Guid owner;

    public SMine(Transform t, GameLogic gl) : base(t)
    {
        this.gl = gl;
        this.disableCollision = true;
        this.transform.rect.Width = 30;
        this.transform.rect.Height = 30;
    }

    public override void Update()
    {
        base.Update();
        if (armingFrames > 0)
        {
            armingFrames--;
            this.disableCollision = true;
        }else{
            this.disableCollision = false;
        }

        lifetimeFrames--;

        if (lifetimeFrames <= 0)
        {
            this.Kill();
            return;

        }
        if (this.gl == null){return;}
    }

    public override void Kill()
    {
        Transform t = new Transform(this.transform.rect.X, this.transform.rect.Y, 150, 150);

        SCExplosion SC = new SCExplosion(t, this.gl,  new Vector2(0,0), 1f);
        this.gl.AddGameObject(SC);

        base.Kill();
    }

}