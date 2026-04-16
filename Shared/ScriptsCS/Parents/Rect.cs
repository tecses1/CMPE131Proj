namespace Shared;
using System.Drawing; // Assuming you are using System.Drawing.RectangleF for the camera
using System.Numerics;

public struct Rect : IEquatable<Rect>
{
    // The core data: representing the CENTER position and the full size.
    public float X; 
    public float Y;
    public float Width;
    public float Height;

    public Rect(float centerX, float centerY, float width, float height)
    {
        X = centerX;
        Y = centerY;
        Width = width;
        Height = height;
    }

    public Rect(PointF centerLocation, SizeF size)
    {
        X = centerLocation.X;
        Y = centerLocation.Y;
        Width = size.Width;
        Height = size.Height;
    }

    // --- Computed Edge Properties ---
    public float Left => X - (Width / 2f);
    public float Right => X + (Width / 2f);
    public float Top => Y - (Height / 2f);
    public float Bottom => Y + (Height / 2f);

    // --- Common RectangleF Properties ---
    public PointF Location
    {
        get => new PointF(X, Y);
        set { X = value.X; Y = value.Y; }
    }

    public SizeF Size
    {
        get => new SizeF(Width, Height);
        set { Width = value.Width; Height = value.Height; }
    }

    public bool IsEmpty => Width <= 0 || Height <= 0;

    // --- Contains Methods ---
    /// <summary>Checks if a specific point is inside the rectangle.</summary>
    public bool Contains(float x, float y)
    {
        return x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    public bool Contains(PointF pt) => Contains(pt.X, pt.Y);

    /// <summary>Checks if another rectangle is entirely contained inside this one.</summary>
    public bool Contains(Rect rect)
    {
        return (rect.Left >= Left && rect.Right <= Right &&
                rect.Top >= Top && rect.Bottom <= Bottom);
    }

    // --- Intersects Methods ---
    public bool IntersectsWith(Rect other)
    {
        return Left < other.Right && Right > other.Left &&
               Top < other.Bottom && Bottom > other.Top;
    }

    public bool IntersectsWith(RectangleF topLefRect)
    {
        return Left < topLefRect.Right && Right > topLefRect.Left &&
               Top < topLefRect.Bottom && Bottom > topLefRect.Top;
    }

    // --- Manipulation Methods ---
    /// <summary>Moves the rectangle by the specified amounts.</summary>
    public void Offset(float x, float y)
    {
        X += x;
        Y += y;
    }

    public void Offset(PointF pos) => Offset(pos.X, pos.Y);

    /// <summary>Expands the rectangle evenly from the center.</summary>
    public void Inflate(float x, float y)
    {
        Width += x * 2f; // Multiply by 2 because it expands on both sides
        Height += y * 2f;
    }

    public void Inflate(SizeF size) => Inflate(size.Width, size.Height);

    // --- Conversion Helpers ---
    /// <summary>Converts this to a standard C# top-left RectangleF.</summary>
    public RectangleF ToRectangleF()
    {
        return new RectangleF(Left, Top, Width, Height);
    }

    /// <summary>Creates a CenteredRectangleF from a standard C# top-left RectangleF.</summary>
    public static Rect FromRectangleF(RectangleF rect)
    {
        return new Rect(
            rect.X + (rect.Width / 2f), 
            rect.Y + (rect.Height / 2f), 
            rect.Width, 
            rect.Height
        );
    }

    // --- Equality (Good practice for custom structs) ---
    public bool Equals(Rect other)
    {
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }
}