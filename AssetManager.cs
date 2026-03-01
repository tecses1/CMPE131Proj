namespace CMPE131Proj;
using Microsoft.AspNetCore.Components;
using Blazorex;
using System.Numerics;

public static class AssetManager
{
    //global variable.
    public static int fps;
    //dictionary of all assets to load. When we add an image, we need a new dict.
    //I'm sorry- there's no better way.

    public static Dictionary<string, GameAsset> _assets = new() {
        { "Player", new GameAsset { Name = "Player", Url = "images/spaceship.png" } },
        { "Projectile", new GameAsset { Name = "Projectile", Url = "images/projectile.png" } },
        { "Star", new GameAsset { Name = "Star", Url = "images/star.png" } }
    };
    //I wont pretend to understand how or why this works.
    public static void DrawImage(IRenderContext ctx, ElementReference img, Transform t)
    {

        if (t.rotation == 0)
        {
            //no rotation required. just draw normally.
            ctx.DrawImage(img, t.position.X-t.size.X / 2, t.position.Y-t.size.Y/2,t.size.X, t.size.Y);
            return;
        }
        ctx.Save();
        //add width/2 and height/2 for traditonal non center rendering. Fucks up everything though if you try to center it after or before.
        ctx.Translate(t.position.X, t.position.Y);

        double radians = t.rotation * Math.PI / 180.0;
        ctx.Rotate((float)radians);
        ctx.DrawImage(img, -t.size.X/2, -t.size.Y/2, t.size.X, t.size.Y);
        ctx.Restore();
    
    }
    
}

public class GameAsset {
    public string Name { get; set; }
    public string Url { get; set; }
    public ElementReference Image { get; set; } // Blazor will fill this
}