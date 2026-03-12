namespace Shared;
using System.Numerics;
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
            go.Store(); //store values for interpolation before update changes them.
            go.Update();
            eventManager.Register(go);

        } 


        // TODO: probably update this when main player updates???
        foreach (Player p in players)
        {
            p.Store();
            p.Update();
            eventManager.Register(p);

            //Check if player is out of bounds, if so, damage the player.
            if (!p.InBounds(this.GetWorldBounds()))
            {
                p.TakeDamage(1);
            }
            
        }


            // process collisions
        eventManager.ProcessCollisions((go, collideGO) =>
        {
            // Console.WriteLine(go.GetType().Name + " and a " + collideGO.GetType().Name +", c=" + go.disableCollision + ", c=" + collideGO.disableCollision);
            
            if (!go.CollideWith(collideGO)) return;
            //if ((go is Projectile && collideGO is Player) ||
            //    (go is Player && collideGO is Projectile)) ADDING PVP BABY
            //    return;
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
            }else if (go is Projectile proj3 && collideGO is Player p2)
            {
                if (proj3.owner != p2.uid)
                {
                    proj3.Kill();
                    p2.TakeDamage(proj3.damage);
                }
            }else if (go is Player p3 && collideGO is Projectile proj4)
            {
                if (proj4.owner != p3.uid)
                {
                    proj4.Kill();
                    p3.TakeDamage(proj4.damage);
                }
            }

        });
            //if obj is in the bounds of the canvas, we can render.


        if ((DateTime.Now - counter).TotalSeconds >= AsteroidSpawnCooldownSeconds)
        {
            Asteroid newAsteroid = Asteroid.GenerateAsteroid();
            
            if (players.Count > 0) newAsteroid.SetTarget(players[(int)Random.Shared.NextInt64(0,players.Count)].transform.position);
            AddGameObject(newAsteroid);
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
        o.RegisterGameLogic(this); //make sure the object has the required reference to this.
        this.objsToAdd.Add(o);
    }

    public void RemoveGameObject(GameObject o)
    {
        this.objsToRemove.Add(o);
    }

    public void RemovePlayer(Player p)
    {
        this.players.Remove(p);
    }

    public void AddPlayer(Player p)
    {
        p.RegisterGameLogic(this);
        this.players.Add(p);
    }
    public List<GameObject> GetActiveObjects()
    {
        return this.activeObjects;
    }

    public List<GameObject> GetPlayers()
    {
        return this.players;
    }

    public GameObject CreateGameObject(string className, string uid)
    {
        GameObject newObj = null;
        
        // Setup common references (GameManager, etc.)
        Transform defaultT = new Transform(0,0,0,0);

        // Factory logic based on the ClassName string at index [0]
        switch (className)
        {
            case "Asteroid":
                newObj = new Asteroid(defaultT, 1);
                break;
            case "Projectile":
                newObj = new Projectile(defaultT, new Vector2(0,0));
                break;
            case "Player":
                newObj = new Player(defaultT);
            break;
            // Add more types here as your game grows
        }
        if (newObj == null)
        {
            Console.WriteLine("Could not find match for instantiate: " + className);
            return null;
        }

        newObj.uid = Guid.Parse(uid);
        return newObj;
    }


    //Header for simplification
    public byte[] GetGameState(DateTime frameStamp)
    {
        return this.GetGameState(frameStamp, players, activeObjects);
    }

    public void LoadGameState(byte[] gameState)
    {
        this.LoadGameState(gameState, players,activeObjects);
    }
    //Go through all groups passed, and, in order, write their meta data and object data.
    byte[] GetGameState(DateTime frameStamp, params List<GameObject>[] groups)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write(frameStamp.Ticks);
            foreach (List<GameObject> group in groups)
            {
                //write the group size.
                writer.Write(group.Count);
                foreach (GameObject go in group)
                {
                    go.WriteMetaData(writer);
                    go.Encode(writer);


                }
            }
            return ms.ToArray();
        }
    }
    void LoadGameState(byte[] stateData, params List<GameObject>[] localGroups)
    {

        using (MemoryStream ms = new MemoryStream(stateData))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            DateTime frameStamp = new DateTime(reader.ReadInt64());

            // Loop through each group list passed in
            for (int i = 0; i < localGroups.Length; i++)
            {
                //Console.WriteLine("[LGS] Working on group " + i);
                List<GameObject> currentGroup = localGroups[i];
                
                // 1. Read how many objects are in this specific group
                int objectCount = reader.ReadInt32();
                //Console.WriteLine("Group has " + objectCount + " objects.");
                // Track UIDs so we know what to delete later
                HashSet<Guid> receivedUids = new HashSet<Guid>();

                for (int j = 0; j < objectCount; j++)
                {
                    // 2. Read Metadata BEFORE instantiation
                    // Note: Ensure your objects write ClassName then UID string in WriteMetaData!
                    object[] metaData = GameObject.ReadMetaData(reader);
                    string uidString = (string)metaData[1];
                    string className = (string)metaData[0];
                    //Console.WriteLine("checking object: " + uidString + " and " + className);
                    Guid uid = Guid.Parse(uidString);
                    
                    receivedUids.Add(uid);

                    // 3. Look for the object in our local group
                    // (Note: For massive lists, a Dictionary is faster than .Find, but this is fine for now)
                    GameObject obj = currentGroup.Find(o => o.uid == uid);

                    // 4. CREATE if it doesn't exist
                    if (obj == null)
                    {
                        //Console.WriteLine("object does not exist, adding object: " + className);
                        obj = CreateGameObject(className, uidString);
                        currentGroup.Add(obj);
                        //decode immediately to set initial values.
                        obj.Decode(reader);

                    }
                    else
                    {
                        //store for interpolation.
                        obj.Store();
                        // 5. UPDATE the state
                        obj.Decode(reader);
                    }
                    //Console.WriteLine("Updating object");
                    //4.5 Store the previous state for interpolation.

                }

                // 6. DELETE (Cleanup) old objects
                // Iterate backwards to safely remove items from the list
                for (int k = currentGroup.Count - 1; k >= 0; k--)
                {
                    if (!receivedUids.Contains(currentGroup[k].uid))
                    {
                        // Call any necessary destroy logic here (e.g., particle effects, physics cleanup)
                        currentGroup.RemoveAt(k);
                    }
                }
            }
        }
    }
    
}
