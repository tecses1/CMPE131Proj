namespace ClientSideWASM;
using Shared;
using System.Drawing;
public class Rect
{
    // Keeping capitalization exactly as requested
    public string fillColor = Settings.DefaultTextBackground;
    public string borderColor = Settings.DefaultTextBorder;
    
    public int borderWidth = 2;
    public int fillAlpha = 255;
    public int borderAlpha = 255;
    public Transform transform;

    public bool worldSpace = true;
    public bool disableRender = false;

    public Rect()
    {
        this.transform = new Transform(0, 0, 0, 0);
    }

    public Rect(int sizex, int sizey)
    {
        this.transform = new Transform(0, 0, sizex, sizey);
    }

    public Rect(int posx, int posy, int sizex, int sizey)
    {
        this.transform = new Transform(posx, posy, sizex, sizey);
    }

    public Rect(Transform t)
    {
        this.transform = t;
    }
    public bool InteresectsWith(RectangleF rect)
    {
        return rect.IntersectsWith(transform.rect);
    }
    public void SetPosition(float x, float y)
    {
        this.transform.SetPosition(x,y);
    }


    public virtual void Register(RenderManager rm)
    {
        rm.RegisterRectToRender(this);
    }

    public virtual void Unregister(RenderManager rm)
    {
        rm.UnregisterRect(this);
    }

    public void setBorderColor(Color c)
    {
        this.borderColor = ColorTranslator.ToHtml(c);
    }

    public void setBorderColor(Color c, int alpha)
    {
        this.borderColor = ColorTranslator.ToHtml(c);
        this.borderAlpha = alpha;
    }

    public void setFillColor(Color c)
    {
        this.fillColor = ColorTranslator.ToHtml(c);
    }

    public void setFillColor(Color c, int alpha)
    {
        this.fillColor = ColorTranslator.ToHtml(c);
        this.fillAlpha = alpha;
    }

    // REFACTORED: Now uses RectangleF for clean intersection checks
    public bool CollideWith(RectangleF other)
    {
        return this.transform.rect.IntersectsWith(other);
    }



    public float[] GetBounds()
    {
        // Leveraging the Transform.Bounds property we created earlier
        RectangleF b = transform.rect;
        return new float[] { b.Left, b.Top, b.Right, b.Bottom };
    }
}