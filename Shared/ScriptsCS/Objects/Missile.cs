namespace Shared;
using System.Numerics;

public class Missile : GameObject
{
    public Vector2 velocity;
    public Guid owner;
    public int damage = 50;
    public Missile(Transform t) : base(t)
    {
    }

    public override void Update()
    {
        this.transform.position += velocity;

        if (!this.CollideWith(gl.GetWorldBounds()))
        {
            this.Kill();
            return;
        }
        
    }

    public override void Kill()
    {
        base.Kill();
    }
}