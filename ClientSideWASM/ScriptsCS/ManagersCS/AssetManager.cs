namespace ClientSideWASM;
using Microsoft.AspNetCore.Components;

public static class AssetManager
{
    //global variable.
    public static int fps;
    //dictionary of all assets to load. When we add an image, we need a new dict.
    //I'm sorry- there's no better way.

    public static Dictionary<string, GameAsset> _assets = new() {
        { "LocalPlayer", new GameAsset { Name = "LocalPlayer", FrameRefs = new [] { "images/spaceship.png" } } },
        { "ClientPlayer", new GameAsset { Name = "ClientPlayer", FrameRefs = new [] { "images/spaceship.png" } } },
        { "Player", new GameAsset { Name = "Player", FrameRefs = new [] { "images/spaceship.png" } } },
        { "Projectile", new GameAsset { Name = "Projectile", FrameRefs = new[] {"images/projectile.png",
                                                                                "images/projectileExplosion/explode0.png",
                                                                                "images/projectileExplosion/explode1.png", 
                                                                                "images/projectileExplosion/explode2.png", 
                                                                                "images/projectileExplosion/explode3.png", 
                                                                                "images/projectileExplosion/explode4.png", 
                                                                                "images/projectileExplosion/explode5.png", 
                                                                                "images/projectileExplosion/explode6.png"  } } },
        { "Star", new GameAsset { Name = "Star", FrameRefs = new[] {"images/star.png"} } },
        { "Asteroid", new GameAsset { Name = "Asteroid", FrameRefs = new[] {"images/asteroid.png",
                                                                                "images/projectileExplosion/explode0.png",
                                                                                "images/projectileExplosion/explode1.png", 
                                                                                "images/projectileExplosion/explode2.png", 
                                                                                "images/projectileExplosion/explode3.png", 
                                                                                "images/projectileExplosion/explode4.png", 
                                                                                "images/projectileExplosion/explode5.png", 
                                                                                "images/projectileExplosion/explode6.png"  } } }
    };

}

