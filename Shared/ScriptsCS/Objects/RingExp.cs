using Shared;
using System;
using System.Numerics;

public class RingExp : Explosion
{
    private float radius = 50f;

    public RingExp(Transform t, Vector2 velocity, float rotationSpeed = 1f) : base(t, velocity, rotationSpeed)
    {
        this.damage = 1000;
        this.force = 100f;
    }

    public override void Update()
    {
        base.Update();
        float growthRate = 15f;
        this.transform.rect.Width += growthRate;
        this.transform.rect.Height += growthRate;
        
        this.transform.rect.X -= growthRate / 2;
        this.transform.rect.Y -= growthRate / 2;
    }
}