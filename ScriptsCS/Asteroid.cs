using System.Dynamic;
using System.Numerics;
using CMPE131Proj;

class Asteroid : GameObject
{
    public float rotationSpeed;
    public Vector2 velocity;
    public float speed;
    public int deathAnimSpeed = 5;
    int cDeathAnim = 5;
    bool dead = false;

    public int hp = 1;
    public Asteroid(ref GameManager gm, Transform t, float speed) : base(ref gm, t)
    {
        this.speed = speed;
        this.rotationSpeed = (float)Random.Shared.NextDouble() * 5;
        hp = (int)this.transform.size.X ;
    }

    public void SetTarget(Transform t)
    {
        Vector2 direction = t.position - transform.position;
        direction = Vector2.Normalize(direction);
        velocity = direction * speed;  
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
            }
    }
}