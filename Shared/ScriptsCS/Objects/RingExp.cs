using Shared;
using System;
using System.Numerics;

public class RingExp : Explosion
{
    private float radius = 50f;
    public int ringLifetimeFrames = 300;
    bool holdFrame = false;

    public RingExp(Transform t, Vector2 velocity, float rotationSpeed = 0f) : base(t, velocity, 0f)
    {
        this.disableCollision = false;
        this.damage = 1000;
        this.force = 100f;
        this.maxFrames = 3;
        this.transform.rotation = 0f; 
        this.transform.rotationSpeed = 0f;
        this.transform.velocity = new Vector2(0,0);
    }

    public override void Update()
    {
        if (this.currentFrame < 4){
        base.Update();
        }

        float growthRate = 10f;
        this.transform.rect.Width += growthRate;
        this.transform.rect.Height += growthRate;
        
        this.transform.rect.X -= growthRate / 2;
        this.transform.rect.Y -= growthRate / 2;

        ringLifetimeFrames--;

        if (ringLifetimeFrames <= 0)
        {
            base.Kill();
        }
    }
}