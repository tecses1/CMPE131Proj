using System.Numerics;

namespace Shared;

[System.Serializable]
public class Transform
{
    public Vector2 position;
    public Vector2 size;
    public float rotation;
    public Transform()
    {
        position = new Vector2(0,0);
        size = new Vector2(0,0);
        rotation = 0;
    }
    public Transform(float x, float y, int sizeX, int sizeY)
    {
        this.position = new Vector2(x,y);
        this.size = new Vector2(sizeX, sizeY);
        this.rotation = 0;
    }
    public Transform(float x, float y, int sizeX, int sizeY, float rotation)
    {
        this.position = new Vector2(x,y);
        this.size = new Vector2(sizeX, sizeY);
        this.rotation = rotation;
    }
    public Vector2 Forward()
    {
        float radians = this.rotation * (float)Math.PI / 180f;
        return new Vector2(
            -(float)Math.Sin(radians),
            (float)Math.Cos(radians)
        );
    }
    public float GetHypotenuse()
    {
        return (float)Math.Sqrt(size.X * size.X + 
                                size.Y * size.Y);
    }
    public Vector2 Left()
    {
        // To rotate 90 degrees counter-clockwise:
        // New X = Old Y
        // New Y = -Old X
        Vector2 forward = Forward();
        Vector2 left = new Vector2(forward.Y , -forward.X);
        return left;
    }

    public void RotateTo(Vector2 pos)
    {
        Vector2 viewDirection = (pos - position);
        if (viewDirection.LengthSquared() == 0f)
            viewDirection = new Vector2(0, -1);
        
        double angleRadiansView = Math.Atan2(viewDirection.X, viewDirection.Y);
        double angleDegrees = (angleRadiansView) * (180.0 / Math.PI);
        rotation = -(float)angleDegrees;
    }
}