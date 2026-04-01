namespace Shared;
using System.Numerics;
using System.Runtime.CompilerServices;
using shared;

public class Squid : Enemy
{
    private double fireRate = 0.8;
    private DateTime lastShot = DateTime.MinValue;
    private float detectionRange = 450f;
    private DateTime lastSeen = DateTime.MinValue;
    private double chaseTime = 5.0;


    public Squid(Transform t) : base(t)
    {
        this.HP = 650; 
        this.speed = 3f;
        this.transform.size = new Vector2(60,60);
       // this.ScoreValue = 100;

    }

    public override void Update()
    {
        if (isDead) return;

       // var player = gl.GetNearestPlayer(transform.position);
        bool detect = false;
        base.Update();
    }
    public override void Kill()
    {
        base.Kill();
    }
}