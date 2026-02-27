namespace CMPE131Proj
{
    using System.Numerics;
    using System;
    using Blazorex;

    public class Projectile
    {
        public float X;
        public float Y;
        public Vector2 Velocity;
        public float Rotation;
        public int LifetimeFrames = 180; 
        public bool Dead => LifetimeFrames <= 0;

        public Projectile(float x, float y, Vector2 velocity, float rotation = 0f, int lifetime = 180)
        {
            X = x;
            Y = y;
            Velocity = velocity;
            Rotation = rotation;
            LifetimeFrames = lifetime;
        }

        public void Update()
        {
            X += Velocity.X;
            Y += Velocity.Y;
            LifetimeFrames--;
        }

        public void Render(IRenderContext ctx)
        {
            var img = AssetManager._assets["Projectile"].Image;

            AssetManager.DrawRotatedImage(
                ctx,
                img,
                X,
                Y,
                30,     // width
                30,     // height
                Rotation
            );
        }
    }
}