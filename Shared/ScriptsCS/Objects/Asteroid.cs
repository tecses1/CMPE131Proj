namespace Shared;
using System.Dynamic;
using System.Numerics;

public class Asteroid : GameObject
{
    public float rotationSpeed;
    public Vector2 velocity;
    public float speed;
    public int deathAnimSpeed = 5;
    int cDeathAnim = 5;
    bool dead = false;

    public int hp = 1;

    public int LifetimeFrames = 90; // how long we live outside of bounds.
    public Asteroid(Transform t, float speed) : base( t)
    {
        this.speed = speed;
        this.rotationSpeed = (float)Random.Shared.NextDouble() * 5;
        hp = (int)this.transform.size.X ;
    }
    public void SetDirection(Vector2 v)
    {
        Vector2 normalized = Vector2.Normalize(v);
        this.velocity = normalized * speed;
    }

    public void SetTarget(Vector2 v)
    {
        Vector2 direction = v - transform.position;
        direction = Vector2.Normalize(direction);
        velocity = direction * speed;  
    }
    public void SetTarget(Transform t)
    {
        SetTarget(t.position);
    }

    public override void Update()
    {

        if (dead)
        {
            this.transform.rotation += 1;
            cDeathAnim -= 1;
            if (cDeathAnim <= 0)
            {
                
                if (currentFrame > 6)
                {   
                    base.Kill();
                }
                else
                {
                    currentFrame += 1;
                }
                cDeathAnim = deathAnimSpeed;
            }
            transform.position += velocity / 20;
        }
        else
        {
            transform.rotation += rotationSpeed;
            transform.position += velocity;

                if (!this.CollideWith(gl.GetWorldBounds())) //We are outside of bounds. Start counting down for kill.
                {
                    //Console.WriteLine("OUT OF BOUNDS");
                    LifetimeFrames--;
                    if (LifetimeFrames < 0)
                    {
                        this.Kill();
                    }
                }
        }
    }

    public override void Kill()
    {
            if (!dead)
            {
                currentFrame = 1;
                dead = true;
                this.disableCollision = true;
                cDeathAnim = deathAnimSpeed;
            float hypo = this.transform.GetHypotenuse();
            if (hypo > 40)
            {
                for (int i = 0; i < 3; i++)
                {
                    float randomAngle = (float)(Math.PI * 2 * Random.Shared.NextDouble());
                    Vector2 randomDirection = new Vector2((float)Math.Cos(randomAngle),(float)Math.Sin(randomAngle));
                    randomDirection = Vector2.Normalize(randomDirection + this.velocity);
                    Transform t = new Transform(this.transform.position.X, this.transform.position.Y, (int)hypo / 3, (int)hypo / 3);
                    Asteroid newAsteroid = new Asteroid(t, this.speed / 3);
                    newAsteroid.SetDirection(randomDirection);
                    gl.AddGameObject(newAsteroid);
                }
            }
            }

    }
    //Helper function to spawn an asteroid on world bounds.
    public static Asteroid GenerateAsteroid()
    {
        Random r = new Random();
        int size = (int)(20 + r.NextDouble() * 30);
        if (r.NextInt64(0,15) == 8)
        {
            size = size * 5;
        }
                
        int spawnX,spawnY;
        int edge = r.Next(0,4);
        switch (edge)
        {
            case 0: //top
                spawnX = r.Next(0, GameConstants.worldSizeX);
                spawnY = -size;
                break;
            case 1: //right
                spawnX = GameConstants.worldSizeX + size;
                spawnY = r.Next(0, GameConstants.worldSizeY);
                break;
            case 2: //bottom
                spawnX = r.Next(0, GameConstants.worldSizeX);
                spawnY = GameConstants.worldSizeY + size;
                break;
            case 3: //left
                spawnX = -size;
                spawnY = r.Next(0, GameConstants.worldSizeY);
                break;
            default:
                spawnX = -size;
                spawnY = r.Next(0, GameConstants.worldSizeY);
                break;

        }
        Transform t = new Transform(spawnX, spawnY, size, size);
        Asteroid a = new Asteroid(t,r.Next(1,3));
        a.SetTarget(new Vector2(GameConstants.worldSizeX/2, GameConstants.worldSizeY/2));//toggle center of screen for now.
        return a;
        //gl.AddGameObject(a):
    }
}