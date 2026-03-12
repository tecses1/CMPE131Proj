using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Numerics;
using Blazorex;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace ClientSideWASM;
//Handles the background, houses then etwork manager, and updates other players and objects.
public class GameManager : RenderManager
{

    //Intiialzie the rendre manager
    //List of different game objects.
    GameObject player;
    List<GameObject> activeObjects = new List<GameObject>();
    List<GameObject> backgroundStars = new List<GameObject>();
    //Remove objects after tehy die. Can not happen during the frame, so we save waht dies during the frame to remove after..
    private List<GameObject> objsToRemove = new List<GameObject>();

    private List<GameObject> objsToAdd = new List<GameObject>();

     DateTime counter = DateTime.Now;
    float AsteroidSpawnCooldownSeconds = 2f;

    NetworkManager nm;

    Text isLocal;
    


    public GameManager(IJSRuntime JSRuntime,  NetworkManager nm) : base(JSRuntime)
    {
        this.nm = nm;
        GameManager reference = this;
         player  = new Player(ref reference, new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight/2, 60,60,0));

        GenerateStars();
        Transform t = new Transform(Settings.CanvasWidth/2, 25, 300,50);
        isLocal = new Text("Playing Locally ", ref t);
        isLocal.worldSpace = false;
    }
    
    void GenerateStars()
    {
        //iterate through canvas coordinates.
        Random r = new Random();
        for (int i = 0; i < worldSizeX; i++)
        {
            for (int j = 0; j < worldSizeY; j++)
            {   
                double chance = r.NextDouble();
                double sizeModifier = Math.Sqrt(worldSizeX * worldSizeY);
                if (chance < Settings.Sparseness / sizeModifier)
                {
                    int size = (int)Math.Clamp(Settings.minSize + r.NextDouble() * Settings.maxSize,Settings.minSize, Settings.maxSize);
                    Transform t = new Transform(i,j,size,size);
                    GameManager reference = this;
                    Star s = new Star(ref reference, t);
                    
                    backgroundStars.Add(s);
                }

            }
        }
        
    }


    public void UpdatePlayer(InputWrapper e)
    {
        if (e.keys[5]) //Escape key pressed, exit game.
        {
            Environment.Exit(0);
        }
        ((Player)player).cInput = e;

    }

    public void SpawnAsteroid()
    {
        Random r = new Random();
        int size = (int)(20 + r.NextDouble() * 30);
        if (r.NextInt64(0,15) == 8)
        {
            size = size * 5;
        }
                
        int spawnX,spawnY;
        int edge = r.Next(0,4);
        switch (edge)
        {
            case 0: //top
                spawnX = r.Next(0, worldSizeX);
                spawnY = -size;
                break;
            case 1: //right
                spawnX = worldSizeX + size;
                spawnY = r.Next(0, worldSizeY);
                break;
            case 2: //bottom
                spawnX = r.Next(0, worldSizeX);
                spawnY = worldSizeY + size;
                break;
            case 3: //left
                spawnX = -size;
                spawnY = r.Next(0, worldSizeY);
                break;
            default:
                spawnX = -size;
                spawnY = r.Next(0, worldSizeY);
                break;

        }
        Transform t = new Transform(spawnX, spawnY, size, size);
        GameManager reference = this;
        Asteroid a = new Asteroid(ref reference, t,r.Next(1,3));
        a.SetTarget(player.transform);
        activeObjects.Add(a);
    }
    public override async Task Render()
    {
        if (nm == null)
        {
            isLocal.Draw(this);
        }
        else
        {
            if (nm.client == null)
            {
                Console.WriteLine("warning... client is null?");
            }
            if (!nm.client.isConnected()) isLocal.Draw(this);
        }

        foreach (GameObject other in backgroundStars)
        {
            AddObjToRender(other);//tell RenderManager to Render the object.
            other.Render(); //Call custom render, if it has one. (Syncs text and rect draw calls)
        }

        foreach (GameObject go in activeObjects)
        {
            AddObjToRender(go); //tell RenderManager to Render the object.
            go.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
        }   
        AddObjToRender(player);//tell RenderManager to Render the object.
        player.Render();//Call custom render, if it has one. (Syncs text and rect draw calls)
        await base.Render(); //Do whatever the RenderManager wants to do by itself. probably the official render calls.

    }
    public override async Task Update()
    {
        await base.Update();

        foreach (GameObject go in objsToRemove)
        {
            activeObjects.Remove(go);
        }
        objsToRemove.Clear();

        foreach (GameObject go in objsToAdd)
        {
            activeObjects.Add(go);
        }
        objsToAdd.Clear();

        //Update the player.
        player.Update();
        //Update stars.
        /*
        foreach (GameObject other in backgroundStars)
        {
            other.Update();
        }*/
        //Update active objects. Check for collision withj stars.
        foreach (GameObject go in activeObjects)
        {
            go.Update();
            if (go.disableCollision) continue; //If the object is already dead, skip collision.
            
            foreach (GameObject collideGO in activeObjects)
            {
            if (collideGO.disableCollision) continue;
                
            if (go.CollideWith(collideGO))
            {
                //Console.WriteLine("We detected a collision between a " + go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
                if (go is Projectile && collideGO is Asteroid)
                {
                    go.Kill();
                    ((Asteroid)collideGO).hp -= ((Projectile)go).damage;

                    if (((Asteroid)collideGO).hp <= 0)
                    {
                        collideGO.Kill();
                        ((Player)player).AddScore(10); // score adding
                    }
                }
            }
        }
            

            //if obj is in the bounds of the canvas, we can render.
        }


        if ((DateTime.Now - counter).TotalSeconds >= AsteroidSpawnCooldownSeconds)
        {
            SpawnAsteroid();
            counter = DateTime.Now;
        }

        
        
    }
    public void AddNewGameObject(GameObject o)
    {
        objsToAdd.Add(o);
    }
    public void RemoveGameObject(GameObject o)
    {
        objsToRemove.Add(o);
    }
}
