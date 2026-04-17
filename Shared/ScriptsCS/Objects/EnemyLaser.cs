namespace Shared;
using System.Numerics;

public class EnemyLaser : Projectile
{
    public EnemyLaser(Transform transform, Vector2 velocity, int lifetime = 15) : base(transform, velocity, lifetime)
    {
        this.transform.rect.Width = 20;
        this.transform.rect.Height = 20;
        this.damage = 5;
    }
}