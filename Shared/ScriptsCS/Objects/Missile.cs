namespace Shared;
using System.Numerics;

public class Missile : GameObject
{
    public Vector2 velocity;
    public Guid owner;

    public Missile(Transform t) : base(t)
    {
    }

    public override void Update()
    {
        this.transform.position += velocity;

        if (!this.CollideWith(gl.GetWorldBounds()))
        {
            this.Kill();
        }
    }

    public override void Kill()
    {
        base.Kill();
    }
}