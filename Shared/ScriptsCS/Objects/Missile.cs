namespace Shared;
using System.Numerics;

public class Missile : Projectile
{
    public GameObject target; //the target the missile is tracking. This is set when the missile is spawned, and doesn't change after that. If the target dies before the missile does, the missile will just keep flying in the same direction until it runs out of lifetime or hits something.
    public float speed = 22f;
    public float maxTurn = 60f;
    public float currentAngle = 0f;

    public Missile(Transform t, Vector2 velocity) : base(t, velocity)
    {
        damage = 250;
        this.transform.velocity = velocity;

    }
    public override void Update()
    {
        base.Update();
        if (target == null || (target is Enemy e && e.hp <= 0) || 
            (target is Asteroid a && a.hp <= 0) || 
            (target is Player p && p.CurrentHealth <= 0) || 
            (currentAngle >= maxTurn)) {return;} //if we don't have a target, just keep flying in the same direction.
        else {

        Vector2 myPos = this.transform.GetPosition();

        Vector2 targetPos = target.transform.GetPosition();
        Vector2 diff = targetPos - myPos;

        double angle = Math.Atan2(diff.X, diff.Y);
        float targetAngle = -(float)(angle * (180/Math.PI));

        float angleDiff = targetAngle - this.transform.rotation;
        while (angleDiff > 180) angleDiff -= 360;
        while (angleDiff < -180) angleDiff += 360;

        float turnRate = 10.0f;

        float frameTurn = Math.Clamp(angleDiff, -turnRate, turnRate);
        this.transform.rotation += frameTurn;

        currentAngle += Math.Abs(frameTurn);

        this.transform.velocity = this.transform.Forward() * 10f;
        }
        //transform.RotateTo(target.transform.GetPosition());
        //this.transform.velocity = this.transform.Forward() * speed; // Set the velocity to the forward direction
    }
    public override void Kill()
    {
        Explosion e = new Explosion(new Transform() { rect = this.transform.rect }, this.transform.velocity / 10, 1f);
        e.transform.rect.Width = 186; //big explosition
        e.transform.rect.Height = 186;
        e.damage = 20;
        e.disableCollision = false; //missile explosion should damage things, unlike regular projectile explosion.
        gl.AddGameObject(e);
        base.Kill();
    }
}