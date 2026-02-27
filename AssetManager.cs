namespace CMPE131Proj;
using Microsoft.AspNetCore.Components;
using Blazorex;
public static class AssetManager
{
    //dictionary of all assets to load. When we add an image, we need a new dict.
    //I'm sorry- there's no better way.

    public static Dictionary<string, GameAsset> _assets = new() {
        { "Player", new GameAsset { Name = "Player", Url = "images/spaceship.png" } },
        { "Projectile", new GameAsset { Name = "Projectile", Url = "images/projectile.png" } }
    };
    //I wont pretend to understand how or why this works.
    public static void DrawRotatedImage(IRenderContext ctx, ElementReference img, float x, float y, float width, float height, float degrees)
    {
        ctx.Save();
        //add width/2 and height/2 for traditonal non center rendering. Fucks up everything though if you try to center it after or before.
        ctx.Translate(x, y);

        double radians = degrees * Math.PI / 180.0;
        ctx.Rotate((float)radians);
        ctx.DrawImage(img, -width/2, -height/2, width, height);
        ctx.Restore();
    
    }
}

public class GameAsset {
    public string Name { get; set; }
    public string Url { get; set; }
    public ElementReference Image { get; set; } // Blazor will fill this
}