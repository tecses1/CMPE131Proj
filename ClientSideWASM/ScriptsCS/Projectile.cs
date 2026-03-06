namespace ClientSideWASM
{
    using System.Numerics;
    using System;
    using Blazorex;
    using System.ComponentModel;

    public class Projectile : GameObject
    {
        //public float X;
        //public float Y;
        [Network]
        public Vector2 Velocity {get; set;}
        [Network]
        public int LifetimeFrames {get; set;} = 30; 
        public int deathAnimSpeed = 5;
        int cDeathAnim = 20;
        bool dead = false;
        [Network]
        public int damage {get; set;} = 1;
        public Projectile(ref GameManager gm, Transform transform, Vector2 velocity, int lifetime = 30) : base(ref gm, transform)
        {
            Velocity = velocity;
            LifetimeFrames = lifetime;
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
                    
                }
                transform.position += Velocity / 20;
            }
            else
            {
                transform.position += Velocity;

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
            }

            
        }

    }
}