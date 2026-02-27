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
        public int LifetimeFrames = 180; 
        public bool Dead => LifetimeFrames <= 0;

        public Projectile(ref GameManager gm, float x, float y, Vector2 velocity, float rotation = 0f, int lifetime = 180) : base(ref gm)
        {
            
            this.x = x;
            this.y = y;
            Velocity = velocity;
            this.rotation = rotation;
            LifetimeFrames = lifetime;

            this.sizeX = 30;
            this.sizeY = 30;
        }

        public void Update()
        {
            x += Velocity.X;
            y += Velocity.Y;
            LifetimeFrames--;
        }

        public override void Render(IRenderContext ctx)
        {

            base.Render(ctx);

        }
    }
}