
namespace Shared;
using System.Drawing;
using System.Numerics;

public class GameObject : NetworkObject
{
    [Network(-1)]
    public Transform transform { get; set; }
    public Transform previousTransform { get; set; }
    
    protected GameLogic gl;

    [Network(-2)]
    public int currentFrame { get; set; } = 0;
    [Network(-3)]
    public int spriteOverrideIndex { get; set; } = -1;
    public bool disableCollision = false;
    public bool disableRender = false;

    public GameObject(Transform transform, int spriteOverrideIndex = -1)
    {
        this.transform = transform;
        this.spriteOverrideIndex = spriteOverrideIndex;
    }

    public void RegisterGameLogic(GameLogic gl) => this.gl = gl;

    // REPLACED: Manual index checking is gone. RectangleF handles it now.
    public bool CollideWith(GameObject other)
    {
        if (disableCollision || other.disableCollision) return false;
        return transform.rect.IntersectsWith(other.transform.rect);
    }

    public bool CollideWith(Rect rect)
    {
        if (disableCollision) return false;
        return transform.rect.IntersectsWith(rect);
    }

    public bool InteresectsWith(Rect rect)
    {
        return rect.IntersectsWith(transform.rect);
    }

    public bool[] GetCollisionSides(Rect container)
    {
        // 0:Top, 1:Right, 2:Bottom, 3:Left
        Rect b = transform.rect;
        return new bool[] {
            b.Top >= container.Top,
            b.Right <= container.Right,
            b.Bottom <= container.Bottom,
            b.Left >= container.Left
        };
    }

    public virtual void Update() { }

    public virtual void Store() => previousTransform = new Transform(transform);

    public virtual void Render(float deltaTime) { }

    public virtual void Kill() => gl.RemoveGameObject(this);

}