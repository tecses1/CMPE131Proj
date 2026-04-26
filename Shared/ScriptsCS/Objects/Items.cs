using System;
using System.Numerics;
using Shared;
 
public class Items : GameObject
{
    public string itemType;
    public float speed = 0.3f;
    public int lifetimeFrames = 1000;

    public Items(Transform t) : base(t)
    {
        this.transform = t;

        this.transform.rect.Width = 20;
        this.transform.rect.Height = 20;
        this.transform.rotation = (float)(Random.Shared.NextDouble() * 360);
    }

    public override void Update()
    {
        base.Update();

        lifetimeFrames--;
        if(lifetimeFrames <= 0)
        {
            if (this.gl != null){
                this.Kill();
            }
        }
    }

    public virtual void Collect(Player p)
    {
        
    }


        
}