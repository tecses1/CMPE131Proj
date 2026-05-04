using Shared;
using System;
using System.Numerics;
using System.Collections.Generic;

public class RingExp : Explosion
{
    private float radius = 50f;
    public int ringLifetimeFrames = 100;
    int localAnimtime = 2;
    public float growthRate = 40f;

    public HashSet<GameObject> alreadyHit = new HashSet<GameObject>(); //to prevent the same object from being calculated as hit more than once

    public RingExp(Transform t, Vector2 velocity, float rotationSpeed = 0f) : base(t, velocity, 0f)
    {
        this.disableCollision = false;
        this.damage = 2000;
        this.force = 0f;
        this.maxFrames = 3;
        this.transform.rotationSpeed = Random.Shared.NextInt64(-20,20) / 10f;
        this.transform.velocity = new Vector2(0,0);
    }

    public override void Update()
    {
        this.transform.Update();
        if (this.currentFrame < 3)
        {
            localAnimtime--;
            if (localAnimtime <= 0)
            {
                this.currentFrame++;
                localAnimtime = this.deathAnimSpeed;

            }
        }else
        {
            this.currentFrame = 3;
        }
        this.transform.rect.Width += growthRate;
        this.transform.rect.Height += growthRate;
        growthRate *= 0.9f;

        //this.transform.rect.X -= this.transform.rect.X / 2;
       // this.transform.rect.Y -= this.transform.rect.Y / 2;

        if (growthRate <= 5f)
        {
            growthRate = 5f;
        }

        if (ringLifetimeFrames <= 60)
        {
            this.disableCollision = true;
        }

        ringLifetimeFrames--;

        if (ringLifetimeFrames <= 0)
        {
            this.Kill();
        }
    }
}