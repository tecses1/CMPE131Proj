namespace Shared;
using System.Numerics;

public class Missile : Projectile
{
    public GameObject target; //the target the missile is tracking. This is set when the missile is spawned, and doesn't change after that. If the target dies before the missile does, the missile will just keep flying in the same direction until it runs out of lifetime or hits something.
    public float speed = 12f;
    public Missile(Transform t, Vector2 velocity) : base(t, velocity)
    {
        damage = 50;
        this.transform.velocity = velocity;

    }
    public override void Update()
    {
        base.Update();
        if (target == null) return; //if we don't have a target, just keep flying in the same direction.
        transform.RotateTo(target.transform.GetPosition());
        this.transform.velocity = this.transform.Forward() * speed; // Set the velocity to the forward direction
    }
    public override void Kill()
    {
        Explosion e = new Explosion(new Transform() { rect = this.transform.rect }, this.transform.velocity / 10, 1f);
        e.transform.rect.Width = 166; //big explosition
        e.transform.rect.Height = 166;
        e.disableCollision = false; //missile explosion should damage things, unlike regular projectile explosion.
        gl.AddGameObject(e);
        base.Kill();
    }
}