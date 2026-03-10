namespace Shared;

//replace game manager so we can run the game logic on a server machine.
public class GameLogic
{

    EventManager eventManager = new EventManager();
    List<GameObject> activeObjects = new List<GameObject>();

    List<GameObject> players = new List<GameObject>();


    List<GameObject> objsToRemove = new List<GameObject>();

    List<GameObject> objsToAdd = new List<GameObject>();


     DateTime counter = DateTime.Now;
    float AsteroidSpawnCooldownSeconds = 2f;
    public void Update()
    {
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


        eventManager.Clear();//clear before they update on this frame.
        foreach (GameObject go in activeObjects)
        {
            //Update and register in same lop. No need to run twice. On.
            go.Update();
            eventManager.Register(go);

        } 


        // TODO: probably update this when main player updates???
        foreach (Player p in players)
        {
            eventManager.Register(p);
        }


            // process collisions
        eventManager.ProcessCollisions((go, collideGO) =>
        {
            // Console.WriteLine(go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
            
            if (!go.CollideWith(collideGO)) return;
            if ((go is Projectile && collideGO is Player) ||
                (go is Player && collideGO is Projectile))
                return;
            if (go is Projectile proj && collideGO is Asteroid asteroid)
            {
                proj.Kill();
                asteroid.hp -= proj.damage;
                if (asteroid.hp <= 0)
                {
                    getPlayerWithUID(proj.owner).AddScore(10);
                    // player.AddScore(10);
                    asteroid.Kill();
                }
            }
            else if (go is Asteroid asteroid2 && collideGO is Projectile proj2)
            {
                proj2.Kill();
                asteroid2.hp -= proj2.damage;
                if (asteroid2.hp <= 0)
                {
                   getPlayerWithUID(proj2.owner).AddScore(10);
                    asteroid2.Kill();
                }
            }
            else if (go is Asteroid a && collideGO is Player p)
            {
                p.TakeDamage(10);
                a.Kill();
            }

        });
            //if obj is in the bounds of the canvas, we can render.


        if ((DateTime.Now - counter).TotalSeconds >= AsteroidSpawnCooldownSeconds)
        {
            AddGameObject(Asteroid.GenerateAsteroid());
            counter = DateTime.Now;
        }
    }
    public float[] GetWorldBounds()
    {
        float[] bounds = new float[4];
        bounds[0] = 0; //Top left corner X
        bounds[1] = 0; //Top left corner Y
        bounds[2] = GameConstants.worldSizeX; //Bottom right corner X
        bounds[3] = GameConstants.worldSizeY; //Bottom right corner Y

        return bounds;
    }
    public Player getPlayerWithUID(Guid uid)
    {
        foreach (Player p in players)
        {
            if (p.uid == uid) return p;
        }
        Console.WriteLine("player " + uid.ToString() + " not found!");
        return null;
    }

    public void AddGameObject(GameObject o)
    {
        
    }

    public void RemoveGameObject(GameObject o)
    {
        
    }


}


/*
    public InputWrapper[] DecodeInputs(byte[] data)
    {

        byte[][] playerInputs = NetworkModel.DeserializeJagged(data);

        InputWrapper[] inputs = new InputWrapper[playerInputs.Length];

        for (int i = 0; i < playerInputs.Length; i++)
        {
            if (playerInputs[i] == null)
            {
                Console.WriteLine("Warning: Null player input at index " + i);
                continue;
            }

            if (playerInputs[i].Length == 0)
            {
                Console.WriteLine("Warning: Empty player input at index " + i);
                continue;
            }
            inputs[i] = InputWrapper.Decode(playerInputs[i]);
            
        }

        return inputs;
    }
*/