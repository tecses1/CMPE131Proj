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
        [Network(0)]
        public Vector2 Velocity {get; set;}
        [Network(1)]
        public int LifetimeFrames {get; set;} = 30; 
        public int deathAnimSpeed = 5;
        int cDeathAnim = 20;
        bool dead = false;
        [Network(2)]
        public int damage {get; set;} = 1;
        [Network(3)]
        public Guid owner;
        public Projectile(ref GameManager gm, Transform transform, Vector2 velocity, int lifetime =5) : base(ref gm, transform)
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

                if (!this.CollideWith(gm.GetWorldBounds())) //We are outside of bounds. \\
                {
                    this.Kill();
                    

                }
                LifetimeFrames--;
                Console.WriteLine("Counting down: " + LifetimeFrames);
                if (LifetimeFrames < 0)
                {
                    this.Kill();
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