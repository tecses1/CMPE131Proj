namespace Shared;
using System.Dynamic;
using System.Numerics;

public class Asteroid : GameObject
{
    public float speed;
    bool dead = false;

    public int hp = 1;

    public int LifetimeFrames = 90; // how long we live outside of bounds.
    public Asteroid(Transform t, float speed) : base( t)
    {
        this.speed = speed;
        this.transform.rotationSpeed = (float)Random.Shared.NextDouble() * 5;
        hp = (int)this.transform.rect.Width ;
    }
    public void SetDirection(Vector2 v)
    {
        Vector2 normalized = Vector2.Normalize(v);
        this.transform.velocity = normalized * speed;
    }

    public void SetTarget(Vector2 v)
    {
        Vector2 direction = v - transform.GetPosition();
        direction = Vector2.Normalize(direction);
        this.transform.velocity = direction * speed;  
    }
    public void SetTarget(Transform t)
    {
        SetTarget(t.GetPosition());
    }

    public override void Update()
    {


        transform.Update();
        
    }

    public override void Kill()
    {

                
        if (Random.Shared.NextInt64(0,20) == 1) //5% chance to drop health pack on death. lots of asteroids. makes sense.
        {
            Transform hpTrans = new Transform(this.transform.rect.X, this.transform.rect.Y, 20, 20);
            Healthpack hp = new Healthpack(hpTrans);
            hp.velocity = this.transform.velocity / 4; // float away from explosion a bit.
            gl.AddGameObject(hp);
        }
        float hypo = this.transform.GetHypotenuse();
        if (hypo > 40) // if we're big enough to split.
        {

            for (int i = 0; i < 3; i++)
            {


                float randomAngle = (float)(Math.PI * 2 * Random.Shared.NextDouble());
                Vector2 randomDirection = new Vector2((float)Math.Cos(randomAngle),(float)Math.Sin(randomAngle));
                randomDirection = Vector2.Normalize(randomDirection + this.transform.velocity);
                Transform t = new Transform(this.transform.rect.X, this.transform.rect.Y, (int)hypo / 3, (int)hypo / 3);
                Asteroid newAsteroid = new Asteroid(t, this.speed / 3);
                newAsteroid.SetDirection(randomDirection);
                gl.AddGameObject(newAsteroid);
            }
        
        }
        Explosion e = new Explosion(new Transform() { rect=this.transform.rect }, this.transform.velocity / 20, 1f);
        gl.AddGameObject(e);
        base.Kill();
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
                spawnX = r.Next(-GameConstants.worldSizeX/2, GameConstants.worldSizeX/2);
                spawnY = -size;
                break;
            case 1: //right
                spawnX = GameConstants.worldSizeX + size;
                spawnY = r.Next(-GameConstants.worldSizeY/2, GameConstants.worldSizeY/2);
                break;
            case 2: //bottom
                spawnX = r.Next(-GameConstants.worldSizeX/2, GameConstants.worldSizeX/2);
                spawnY = GameConstants.worldSizeY + size;
                break;
            case 3: //left
                spawnX = -size;
                spawnY = r.Next(-GameConstants.worldSizeY/2, GameConstants.worldSizeY/2);
                break;
            default:
                spawnX = -size;
                spawnY = r.Next(-GameConstants.worldSizeY/2, GameConstants.worldSizeY/2);
                break;

        }
        Transform t = new Transform(spawnX, spawnY, size, size);
        Asteroid a = new Asteroid(t,r.Next(3,6));
        a.SetTarget(new Vector2(GameConstants.worldSizeX/2, GameConstants.worldSizeY/2));//toggle center of screen for now.
        return a;
        //gl.AddGameObject(a):
    }
}