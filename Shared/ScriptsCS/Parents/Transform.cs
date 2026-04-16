using System.Numerics;

namespace Shared;
using System.Drawing;
using System;
using System.Numerics; // Assuming System.Numerics.Vector2

public class Transform
{
    //public Vector2 position;
    //public Vector2 size;

    public Rect rect;
    public float rotation;
    public float rotationSpeed;
    public Vector2 velocity;

    public Transform()
    {
        this.rect = new Rect(0, 0, 0, 0);
        rotation = 0;
    }

    public Transform(float x, float y, float sizeX, float sizeY, float rotation = 0)
    {
        this.rect = new Rect(x, y, sizeX, sizeY);
        this.rotation = rotation;
    }

    public Transform(Transform other)
    {
        this.rect = other.rect;
        rotation = other.rotation;
        velocity = other.velocity;
        rotationSpeed = other.rotationSpeed;
    }

    // This property handles the "Centered to Top-Left" conversion for RectangleF

    public Vector2 Forward()
    {
        float radians = RotationRadians();
        return new Vector2(-(float)Math.Sin(radians), (float)Math.Cos(radians));
    }

    public Vector2 Left()
    {
        Vector2 f = Forward();
        return new Vector2(f.Y, -f.X);
    }

    public float RotationRadians() => rotation * (float)Math.PI / 180f;

    public float GetHypotenuse() => (float)Math.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height);

    public void RotateTo(Vector2 targetPos)
    {
        Vector2 position = new Vector2(this.rect.X, this.rect.Y);
        Vector2 viewDirection = targetPos - position;
        if (viewDirection.LengthSquared() == 0f) viewDirection = new Vector2(0, -1);
        
        double angleRadians = Math.Atan2(viewDirection.X, viewDirection.Y);
        rotation = -(float)(angleRadians * (180.0 / Math.PI));
    }
    public void RotateTo(Transform target)
    {
        Vector2 position = new Vector2(this.rect.X, this.rect.Y);
        Vector2 targetPos = new Vector2(target.rect.X, target.rect.Y);
        Vector2 viewDirection = targetPos - position;
        if (viewDirection.LengthSquared() == 0f) viewDirection = new Vector2(0, -1);
        
        double angleRadians = Math.Atan2(viewDirection.X, viewDirection.Y);
        rotation = -(float)(angleRadians * (180.0 / Math.PI));
    }
    public void Update()
    {
        rect.X += velocity.X;
        rect.Y += velocity.Y;
        rotation += rotationSpeed;
    }
    public Vector2 GetPosition() => new Vector2(rect.X, rect.Y);    
    public void SetPosition(float x, float y)
    {
        rect.X = x;
        rect.Y = y;
    }
    public void SetPosition(Vector2 pos)
    {
        rect.X = pos.X;
        rect.Y = pos.Y;
    }   
    
}