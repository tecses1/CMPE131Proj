namespace Shared;
using System.Numerics;
using System.Runtime.CompilerServices;

public class AlienSM : Enemy
{
    private double fireRate = 0.8;
    private DateTime lastShot = DateTime.MinValue;
    private float detectionRange = 450f;
    private float SMspeed = 3f;
    private float frameTimer = 0.0f;
    private float timePerFrame = 0.25f;
    
    public int totalFrames = 2;

    public AlienSM(Transform t) : base(t)
    {
        this.hp = 650; 
        this.transform.rect.Width = 60;
        this.transform.rect.Height = 60;

    }
    public override void Fire()
    {

        //takje target tsf
        //create direction vector to that
        //take tangent of the vector
        //set rotation tpo that angle

        Projectile proj = new Projectile(new Transform(this.transform.rect.X, this.transform.rect.Y, 10, 10,this.transform.rotation), this.transform.Forward() * 20f, lifetime: 60);
        proj.owner = this.uid;
        proj.damage = 45;
        gl.AddGameObject(proj);
        lastShot = DateTime.Now;
    }
    public override void Update()
    {
        if (hp <= 0) return;

        frameTimer += 1.0f/GameConstants.updateRate;
        if (frameTimer >= timePerFrame)
        {
            currentFrame++;
            if (currentFrame >= totalFrames) currentFrame = 0;
            frameTimer = 0;
        }

        base.Update();
        transform.rotation = 0;

    }
    public override void Kill()
    {
        base.Kill();
    }

}