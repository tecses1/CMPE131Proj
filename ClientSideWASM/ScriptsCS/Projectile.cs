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
        public Vector2 Velocity;
        public int LifetimeFrames = 30; 
        public int deathAnimSpeed = 5;
        int cDeathAnim = 20;
        bool dead = false;
        public int damage = 1;
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

        public override string[] Encode()
        {
            string[] baseData = base.Encode();
            string[] data = new string[baseData.Length + 3];
            data[0] = Velocity.X.ToString();
            data[1] = Velocity.Y.ToString();
            data[2] = damage.ToString();
            Array.Copy(baseData, 0, data, 3, baseData.Length);
            return data;
        }
        public override void Decode(string[] args)
        {
            Velocity.X = float.Parse(args[0]);
            Velocity.Y = float.Parse(args[1]);  
            this.damage = int.Parse(args[2]);
            base.Decode(args.Skip(3).ToArray() );
        }
    }
}