namespace Shared;
using System.Dynamic;
using System.Numerics;

public class Healthpack : GameObject
{
    // public float rotationSpeed;
    public Vector2 velocity;
    public float speed = 0.1f;
    // public int deathAnimSpeed = 2;
    // int cDeathAnim = 2;
    // bool dead = false;

    // public int hp = 1;
    public int healAmount = 25;
    public int LifetimeFrames = 90; // how long we live outside of bounds.
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
        int size = 20;
        
        int spawnX, spawnY;
        int edge = r.Next(0,4);
        switch (edge)
        {
            case 0: // top
                spawnX = r.Next(0, GameConstants.worldSizeX);
                spawnY = -size;
                break;
            case 1: // right
                spawnX = GameConstants.worldSizeX + size;
                spawnY = r.Next(0, GameConstants.worldSizeY);
                break;
            case 2: // bottom
                spawnX = r.Next(0, GameConstants.worldSizeX);
                spawnY = GameConstants.worldSizeY + size;
                break;
            case 3: // left
                spawnX = -size;
                spawnY = r.Next(0, GameConstants.worldSizeY);
                break;
            default:
                spawnX = -size;
                spawnY = r.Next(0, GameConstants.worldSizeY);
                break;
        }

        Transform t = new Transform(spawnX, spawnY, size, size);
        HealthPack hp = new HealthPack(t);

        // Set a random float direction
        double randomAngle = 2 * Math.PI * r.NextDouble();
        Vector2 randomDirection = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
        hp.SetDirection(randomDirection);
        
        return hp;
    }
}