namespace Shared;
using System;
using System.Numerics;

public class SCExplosion : Explosion

{
    private  float radius = 100f;

    public SCExplosion (Transform t, Vector2 velocity, float rotationSpeed = 1f) : base(t, velocity, rotationSpeed)
    {
        this.damage = 250;
        this.force = 10f;
    }


    public override void Update()
    {
        base.Update();
    }
    
    public override void Kill()
    {
        Transform t = new Transform(this.transform);
        RingExp SC = new RingExp(this.transform, new Vector2(0,0), 1f);
        gl.AddGameObject(SC);

        base.Kill();
    }
}