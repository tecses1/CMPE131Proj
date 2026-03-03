namespace ClientSideWASM;
using System.Dynamic;
using System.Numerics;

class Asteroid : GameObject
{
    public float rotationSpeed;
    public Vector2 velocity;
    public float speed;
    public int deathAnimSpeed = 5;
    int cDeathAnim = 5;
    bool dead = false;

    public int hp = 1;

    public int LifetimeFrames = 90; // how long we live outside of bounds.
    public Asteroid(ref GameManager gm, Transform t, float speed) : base(ref gm, t)
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

                if (!this.CollideWith(gm.GetWorldBounds())) //We are outside of bounds. Start counting down for kill.
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
                    Asteroid newAsteroid = new Asteroid(ref gm, t, this.speed / 3);
                    newAsteroid.SetDirection(randomDirection);
                    gm.AddNewGameObject(newAsteroid);
                }
            }
            }

    }
}