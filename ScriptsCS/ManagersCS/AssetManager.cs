namespace CMPE131Proj;
using Microsoft.AspNetCore.Components;

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

}

