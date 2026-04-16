namespace Shared;
using System.Dynamic;
using System.Numerics;

public class SpaceRacerPack : GameObject
{
    // public float rotationSpeed;
    public Vector2 velocity;
    public float speed = 0.3f;

    public int GunCount = 0;
    // public int deathAnimSpeed = 2;
    // int cDeathAnim = 2;
    // bool dead = false;

    // public int hp = 1;
    public int LifetimeFrames = 1000; 
    bool collected = false;
    
    public SpaceRacerPack(Transform t) : base( t)
    {
        // this.speed = speed;
        transform.rotation = (float)(Random.Shared.NextDouble() * 360);
        // hp = (int)this.transform.size.X ;
        this.transform.rect.Width = 20;
        this.transform.rect.Height = 20;
    }
    public void SetDirection(Vector2 v)
    {
        Vector2 normalized = Vector2.Normalize(v);
        this.velocity = normalized * speed;
    }

    public override void Update()
    {
        transform.Update();
        /*if (collected)
        {
            // Slowly float away if collected (optional effect)
        }
        else
        {
            transform.position += velocity;

        }*/
    }

    public override void Kill()
    {
        base.Kill();
    }

    public static SpaceRacerPack GenerateSpaceRacerPack()
    {
        Random r = new Random();
        int size = 20; // pack size

        // Spawn fully inside world bounds
        int spawnX = r.Next(size, GameConstants.worldSizeX - size);
        int spawnY = r.Next(size, GameConstants.worldSizeY - size);

        Transform t = new Transform(spawnX, spawnY, size, size);
        SpaceRacerPack pack = new SpaceRacerPack(t);

        // Random direction
        double randomAngle = 2 * Math.PI * r.NextDouble();
        Vector2 randomDirection = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
        pack.SetDirection(randomDirection);

        // Long lifetime for testing
        pack.LifetimeFrames = 3600; // e.g., 1 minute at 60 updates/sec

        return pack;
    }

    public void Collect(Player player)
    {
        GunCount++;
        //TODO: ADD SpaceGun effect here (OP GUN that does infinite damage for like 5 secs)
        // Console.WriteLine($"[SpaceRacerGun] Collected by {player.playerNameString} (UID: {player.uid}), +{healAmount} HP, now {player.CurrentHealth}");
        this.Kill(); // remove the gun Pack
    }
}