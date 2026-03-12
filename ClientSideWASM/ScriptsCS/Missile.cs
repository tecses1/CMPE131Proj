namespace ClientSideWASM
{
    using System.Dynamic;
    using System.Numerics;

    public class Missile : Projectile
    {
        public Missile(ref GameManager gm, Transform transform, Vector2 velocity) : base (ref gm, transform, velocity)
        {
            this.damage = 5;
            this.Velocity *= 0.5f;
        }
    }
}