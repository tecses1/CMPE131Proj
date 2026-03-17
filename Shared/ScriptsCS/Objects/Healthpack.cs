namespace Shared;
using System.Dynamic;
using System.Numerics;

public class Healthpack : GameObject
{
    // public float rotationSpeed;
    public Vector2 velocity;
    public float speed = 0.3f;
    // public int deathAnimSpeed = 2;
    // int cDeathAnim = 2;
    // bool dead = false;

    // public int hp = 1;
    public int healAmount = 200;
    public int LifetimeFrames = 1000; 
    bool collected = false;
    
    public Healthpack(Transform t) : base( t)
    {
        // this.speed = speed;
        transform.rotation = (float)(Random.Shared.NextDouble() * 360);
        // hp = (int)this.transform.size.X ;
    }
    public void SetDirection(Vector2 v)
    {
        Vector2 normalized = Vector2.Normalize(v);
        this.velocity = normalized * speed;
    }

    public override void Update()
    {
        if (collected)
        {
            // Slowly float away if collected (optional effect)
            transform.position += velocity / 2;
        }
        else
        {
            transform.position += velocity;

            if (!this.CollideWith(gl.GetWorldBounds()))
            {
                // Outside of bounds, start countdown
                LifetimeFrames--;
                if (LifetimeFrames < 0)
                {
                    base.Kill();
                }
            }
        }
    }

    public override void Kill()
    {
        base.Kill();
    }

    public static Healthpack GenerateHealthPack()
    {
        Random r = new Random();
        int size = 20; // health pack size

        // Spawn fully inside world bounds
        int spawnX = r.Next(size, GameConstants.worldSizeX - size);
        int spawnY = r.Next(size, GameConstants.worldSizeY - size);

        Transform t = new Transform(spawnX, spawnY, size, size);
        Healthpack hp = new Healthpack(t);

        // Random direction
        double randomAngle = 2 * Math.PI * r.NextDouble();
        Vector2 randomDirection = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
        hp.SetDirection(randomDirection);

        // Long lifetime for testing
        hp.LifetimeFrames = 3600; // e.g., 1 minute at 60 updates/sec

        return hp;
    }

    public void Collect(Player player)
    {
        player.Heal(this.healAmount); // use the Player's Heal() method
        // Console.WriteLine($"[HealthPack] Collected by {player.playerNameString} (UID: {player.uid}), +{healAmount} HP, now {player.CurrentHealth}");
        this.Kill(); // remove the health pack
    }
}