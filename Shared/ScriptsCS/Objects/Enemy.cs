namespace shared;
using System.Numerics;
using Shared;

public class Enemy : GameObject
{
    public int HP {get; protected set;} = 50;
    public float speed{get; protected set;} = 2.0f;

    protected Vector2 velocity;
    protected GameObject? target;
    protected bool isDead = false;

    public Enemy(Transform t) : base(t){
        
    }

    public virtual void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }
public override void Update()
    {
        if (isDead) return;

        Move();

        if (!this.CollideWith(gl.GetWorldBounds()))
        {
            this.Kill();
        }
    }

    protected virtual void Move()
    {
        if (target != null)
        {
            Vector2 dir = target.transform.position - transform.position;
            if (dir.LengthSquared() > 0)
            {
                dir = Vector2.Normalize(dir);
                velocity = dir * speed;
                transform.position += velocity;
                transform.RotateTo(target.transform.position);
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        HP -= damage;
        if (HP <= 0)
        {
            this.Kill();
        }
    }

    public override void Kill()
    {
        if (isDead) return;
        isDead = true;
        this.disableCollision = true;
        base.Kill();
    }

}