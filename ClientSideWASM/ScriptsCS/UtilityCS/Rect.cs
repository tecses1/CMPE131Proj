namespace ClientSideWASM;
using Shared;
using System.Drawing;
public class Rect
{
    public string fillColor  = Settings.DefaultTextBackground;
    public string borderColor = Settings.DefaultTextBorder;
    
    public int borderWidth = 2;
    public int fillAlpha = 255;
    public int borderAlpha = 255;
    public Transform transform;

    public bool worldSpace = true;

    public bool disableRender = false;

    public Rect()
    {
        this.transform = new Transform(0,0,0,0);
    }
    public Rect(int sizex, int sizey)
    {
        this.transform = new Transform(0,0,sizex,sizey);
    }
    public Rect(int posx, int posy, int sizex, int sizey)
    {
        this.transform = new Transform(posx,posy,sizex,sizey);
    }
    public Rect(Transform t)
    {
        this.transform = t;

    }

    public void SetPosition(float x, float y)
    {
        this.transform.position.X = x;
        this.transform.position.Y = y;
    }

    public void SetSize(int x, int y)
    {
        this.transform.size.X = x;
        this.transform.size.Y = y;
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
    public bool InBounds(float[] rect)
    {
        float[] myBounds = this.GetBounds();
        float[] otherBounds = rect;//.GetBounds();
        return myBounds[0] < otherBounds[2] && // Rect1 Left < Rect2 Right
           myBounds[2] > otherBounds[0] && // Rect1 Right > Rect2 Left
           myBounds[1] < otherBounds[3] && // Rect1 Top < Rect2 Bottom
           myBounds[3] > otherBounds[1];   // Rect1 Bottom > Rect2 Top
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
}