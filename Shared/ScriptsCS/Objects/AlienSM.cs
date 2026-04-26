namespace Shared;
using System.Numerics;
using System.Runtime.CompilerServices;

public class AlienSM : Enemy
{
    private DateTime lastShot = DateTime.MinValue;

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

        EnemyLaser proj = new EnemyLaser(new Transform(this.transform.rect.X, this.transform.rect.Y, 20, 20,this.transform.rotation), this.transform.Forward() * 25f, lifetime: 20);
        proj.owner = this.uid;
        proj.damage = 15; //More is too op lol
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