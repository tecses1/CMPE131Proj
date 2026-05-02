namespace ClientSideWASM;
using Microsoft.AspNetCore.Components;

public static class AssetManager
{
    //global variable.
    public static int fps;
    //dictionary of all assets to load. When we add an image, we need a new dict.
    //I'm sorry- there's no better way.

    public static Dictionary<string, GameAsset> _assets = new() {
        { "MissingImage", new GameAsset { Name = "MissingImage", FrameRefs = new[] { "missing.png" } } },
        { "LocalPlayer", new GameAsset { Name = "LocalPlayer", FrameRefs = new [] { "images/spaceship.png" } } },
        { "ClientEnemy", new GameAsset { Name = "ClientEnemy", FrameRefs = new [] { "images/pirateShip.png" } } },
        { "ClientPlayer", new GameAsset { Name = "ClientPlayer", FrameRefs = new [] { "images/spaceship.png" } } },
        { "Player", new GameAsset { Name = "Player", FrameRefs = new [] { "images/spaceship.png"} } },

        { "Projectile", new GameAsset { Name = "Projectile", FrameRefs = new[] {"images/projectile.png" } } },
        { "Star", new GameAsset { Name = "Star", FrameRefs = new[] {"images/star.png"} } },
        { "Asteroid", new GameAsset { Name = "Asteroid", FrameRefs = new[] {"images/asteroid.png" } } },
                                                                                    
        { "Healthpack", new GameAsset { Name = "Healthpack", FrameRefs = new[] { "images/hp.png" } } },

                { "Explosion", new GameAsset { Name = "Explosion", FrameRefs = new[] { "images/projectileExplosion/explode0.png",
                                                                                     "images/projectileExplosion/explode1.png",
                                                                                     "images/projectileExplosion/explode2.png",
                                                                                     "images/projectileExplosion/explode3.png",
                                                                                     "images/projectileExplosion/explode4.png",
                                                                                     "images/projectileExplosion/explode5.png",
                                                                                     "images/projectileExplosion/explode6.png" } } },


        { "Missile", new GameAsset { Name = "Missile", FrameRefs = new[] { "images/Missile.png" } } },
        {"MissileAmmo", new GameAsset {Name = "MissileAmmo", FrameRefs = new[] {"images/MissileAmmo.png"}}},
        {"MineAmmo", new GameAsset {Name = "MineAmmo", FrameRefs = new[] {"images/MineAmmo.png"}}},
        {"SMine", new GameAsset {Name = "SMine", FrameRefs = new[] {"images/LiveScharge.png"}}},
        {"SCExplosion", new GameAsset {Name = "SCExplosion", FrameRefs = new[] {"images/SCExplosion/ScEx0.png",
                                                                                     "images/SCExplosion/ScEx1.png",
                                                                                     "images/SCExplosion/ScEx2.png",
                                                                                     "images/SCExplosion/ScEx3.png",
                                                                                     "images/SCExplosion/ScEx4.png",
                                                                                     "images/SCExplosion/ScEx5.png",
                                                                                     "images/SCExplosion/ScEx6.png",
                                                                                     "images/SCExplosion/ScEx7.png",
                                                                                     "images/SCExplosion/ScEx8.png",
                                                                                     "images/SCExplosion/ScEx9.png"}}},
                                                                                     
        {"RingExp", new GameAsset {Name = "RingExp", FrameRefs = new[] {"images/projectileExplosion/explode0.png",
                                                                                     "images/RingExp/SCRing1.png",
                                                                                     "images/RingExp/SCRing2.png",
                                                                                     "images/RingExp/SCRing3.png",
                                                                                     "images/RingExp/SCRing4.png"} } },
        {"AlienSM", new GameAsset {Name = "AlienSM", FrameRefs = new[] {"images/AlienSmAnim/AlienSM1.png", "images/AlienSmAnim/AlienSM2.png"}}},
        {"EnemyLaser", new GameAsset {Name = "EnemyLaser", FrameRefs = new[]{"images/AlienLaser.png"}}},
        {"ClientUFO", new GameAsset {Name = "ClientUFO", FrameRefs = new[] {"images/AlienSmAnim/AlienSM1.png", "images/AlienSmAnim/AlienSM2.png"}}}
                                
    };


}

