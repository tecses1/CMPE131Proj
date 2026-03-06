
namespace ClientSideWASM;

using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
public class GameObject
{

    public Guid uid {get; set;}
    //sizing
    [Network(-1)]
    public Transform transform {get;set;}
    //game manager reference. This class makes many frequent callbacks.
    protected GameManager gm;

    public Text missingText;

    [Network(-2)]
    public int currentFrame {get;set;} = 0;

    public bool disableCollision = false;


    public GameObject(ref GameManager gm, Transform transform)
    {
        this.gm  = gm;
        this.transform = transform;
        this.uid =  Guid.NewGuid();
        this.missingText = new Text("MISSING",ref transform);
        this.missingText.setFillColor(Color.LightYellow,255);
        this.missingText.setBorderColor(Color.Red);
        this.missingText.borderWidth = 5;
        
    }
    public bool InBounds(float[] rect)
    {
        float[] myBounds = this.GetBounds();
        float[] otherBounds = rect;//.GetBounds();
        return myBounds[0] < otherBounds[2] && // Rect1 Left < Rect2 Right
           myBounds[2] > otherBounds[0] && // Rect1 Right > Rect2 Left
           myBounds[1] < otherBounds[3] && // Rect1 Top < Rect2 Bottom
           myBounds[3] > otherBounds[1];   // Rect1 Bottom > Rect2 Top
    }
    public bool CollideWith(GameObject two)
    {//Collid with other gameobject.
        return this.CollideWith(two.GetBounds());
    }
    
    public bool CollideWith(float[] rect)
    {//Collide with rect
        if (disableCollision) {
            return false;
        }
        //if (disableCollision) return false;
        return this.InBounds(rect);

    }
    //returns specific collision information.
    public bool[] GetCollisionSides(GameObject obj)
    {
        return this.GetCollisionSides(obj.GetBounds());
    }
    public bool[] GetCollisionSides(float[] rect2)
    {
        // Index mapping: 0:Top, 1:Right, 2:Bottom, 3:Left
        // Default to true (assume we are inside)
        bool[] sides = new bool[4] { true, true, true, true };
        float[] rect1 = this.GetBounds();
        // 1. Is my Top edge still below the container's Top?
        // In Canvas/Y-Down: rect1.Top must be >= rect2.Top
        if (rect1[1] < rect2[1]) sides[0] = false;

        // 2. Is my Right edge still to the left of the container's Right?
        if (rect1[2] > rect2[2]) sides[1] = false;

        // 3. Is my Bottom edge still above the container's Bottom?
        if (rect1[3] > rect2[3]) sides[2] = false;

        // 4. Is my Left edge still to the right of the container's Left?
        if (rect1[0] < rect2[0]) sides[3] = false;

        return sides;
    }
    public float[] GetBounds()
    {
        //because all images draw cenetered, we need a bounds rect.
        //We get the top left point, top right point, and the bottom left point, bottom right point. 
        float[] bounds = new float[4];
        bounds[0] = transform.position.X - transform.size.X/2;
        bounds[1] = transform.position.Y - transform.size.Y/2;
        bounds[2] = transform.position.X + transform.size.X/2;
        bounds[3] = transform.position.Y+ transform.size.Y/2;

        return bounds;

    }
    public virtual void Update()
    {
        
    }

    public virtual void Render()
    {
        
    }
    public virtual void Encode(BinaryWriter writer)
    {
        var properties = NetworkMemberCache.GetSyncProperties(this.GetType());
        
        
        foreach (var prop in properties)
        {
            NetworkMemberCache.WriteProperty(writer, prop.GetValue(this), prop.PropertyType);
        }


        
    }

    public virtual void Decode(BinaryReader reader)
    {
        var properties = NetworkMemberCache.GetSyncProperties(this.GetType());
        
        foreach (var prop in properties)
        {
            object value = NetworkMemberCache.ReadProperty(reader, prop.PropertyType);
            prop.SetValue(this, value);
        }
    }
    public virtual void Kill()
    {
        gm.RemoveGameObject(this);
        
        
    }
}