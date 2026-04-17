using System.Numerics;

namespace Shared;

public class Explosion : GameObject
{

    public int deathAnimSpeed = 2;
    int cDeathAnim = 2;
    public int damage = 3; //applies over the course of the explosion animation, so that things that stay in the explosion longer take more damage.
    public float force = 20f;
    public Explosion(Transform t, Vector2 velocity, float rotationSpeed = 1f) : base(t)
    {
        this.currentFrame = 0;
        this.transform.velocity = velocity;
        this.transform.rotationSpeed = rotationSpeed;
        this.disableCollision = true; // Explosions don't collide with anything, unless explcitily told to.
    }

    public override void Update()
    {

        cDeathAnim -= 1;
        if (cDeathAnim <= 0)
        {

                currentFrame += 1;
                if (currentFrame > 5)
                {
                    if (this.gl == null){ Console.WriteLine("GL is null in explosion!"); return; }
                     base.Kill();

                }
            

            cDeathAnim = deathAnimSpeed;
        }
        
    }

}