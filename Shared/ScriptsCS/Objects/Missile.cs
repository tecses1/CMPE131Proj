namespace Shared
{
    using System.Dynamic;
    using System.Numerics;

    public class Missile : Projectile
    {
        public Missile(Transform transform, Vector2 velocity) : base (transform,velocity)
        {
            this.damage = 5;
            this.Velocity *= 0.5f;
        }
    }
}