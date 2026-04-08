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
    }
    public override void Kill()
    {
        base.Kill();
    }
}