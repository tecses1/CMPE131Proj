namespace CMPE131Proj
{
    using System.Numerics;
    using System;
    using Blazorex;

    public class Projectile : GameObject
    {
        //public float X;
        //public float Y;
        public Vector2 Velocity;
        public int LifetimeFrames = 30; 

        public Projectile(ref GameManager gm, Transform transform, Vector2 velocity, int lifetime = 30) : base(ref gm, transform)
        {
            Velocity = velocity;
            LifetimeFrames = lifetime;
        }

        public override void Update()
        {
            transform.position += Velocity;

            if (!this.CollideWith(gm.GetBounds())) //We are outside of bounds. Start counting down for kill.
            {
                Console.WriteLine("OUT OF BOUNDS");
                LifetimeFrames--;
                if (LifetimeFrames < 0)
                {
                    this.Kill();
                }
            }

            

        }
    }
}