namespace Shared;
using System.Numerics;
using System.Drawing;    
//replace game manager so we can run the game logic on a server machine.
public class GameLogic
{

    //EventManager eventManager = new EventManager();
    public CollisionManager collisionManager = new CollisionManager(GameConstants.worldSizeX, GameConstants.worldSizeY);
    List<GameObject> activeObjects = new List<GameObject>();

    List<GameObject> players = new List<GameObject>();


    List<GameObject> objsToRemove = new List<GameObject>();

    List<GameObject> objsToAdd = new List<GameObject>();

    List<ObjChangeWrapper> newObjectsLoaded = new List<ObjChangeWrapper>();
    List<ObjChangeWrapper> destroyedObjectsLoaded = new List<ObjChangeWrapper>();

    DateTime counter = DateTime.Now;
    float AsteroidSpawnCooldownSeconds = 2f;

    public Rect worldBounds;
    public GameLogic()
    {
        this.collisionManager.Register(activeObjects);
        this.collisionManager.Register(players);
        this.worldBounds = new Rect(0, 0, GameConstants.worldSizeX, GameConstants.worldSizeY);
    }
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


        //eventManager.Clear();//clear before they update on this frame.
        foreach (GameObject go in activeObjects)
        {
            //Update and register in same lop. No need to run twice. On.
            go.Store(); //store values for interpolation before update changes them.
            go.Update();
            //eventManager.Register(go);

        } 


        // TODO: probably update this when main player updates???
        foreach (Player p in players)
        {
            p.Store();
            p.Update();
            //eventManager.Register(p);
            
        }

        collisionManager.UpdateTree(); //update the tree with new positions before we check for collisions.

        foreach (Collision collision in collisionManager.CheckCollisions())
        {
            GameObject A = collision.ObjectA;
            GameObject B = collision.ObjectB;
            switch (A, B)
            {
                case (Projectile, Asteroid):
                case (Asteroid, Projectile):
                    Projectile proj = A is Projectile ? (Projectile)A : (Projectile)B;
                    Asteroid asteroid = A is Asteroid ? (Asteroid)A : (Asteroid)B;
                    proj.Kill();
                    asteroid.hp -= proj.damage;
                    if (asteroid.hp <= 0)
                    {
                        //getPlayerWithUID(proj.owner).AddScore(10); returns null for enemies. standby.
                        asteroid.Kill();
                    }
                    break;
                case (Asteroid, Player):
                case (Player, Asteroid):
                    Asteroid asteroid2 = A is Asteroid ? (Asteroid)A : (Asteroid)B;
                    Player player = A is Player ? (Player)A : (Player)B;
                    player.TakeDamage(asteroid2.hp);
                    asteroid2.Kill();
                    break;
                case (Projectile, Player):
                case (Player, Projectile):  
                    Projectile proj2 = A is Projectile ? (Projectile)A : (Projectile)B;
                    Player player2 = A is Player ? (Player)A : (Player)B;
                    if (proj2.owner != player2.uid)
                    {
                        proj2.Kill();
                        player2.TakeDamage(proj2.damage);
                    }
                    break;
                case (Player, Healthpack):
                case (Healthpack, Player):
                    Healthpack hp = A is Healthpack ? (Healthpack)A : (Healthpack)B;
                    Player player3 = A is Player ? (Player)A : (Player)B;
                    player3.Heal(hp.healAmount);
                    hp.Kill();
                    break;
                case (Explosion, GameObject):
                case(GameObject, Explosion):
                    Explosion explosion = A is Explosion ? (Explosion)A : (Explosion)B;
                    GameObject other = A is Explosion ? (GameObject)B : (GameObject)A;
                    if (other is Player)
                    {
                        Player player4 = (Player)other;
                        player4.TakeDamage(explosion.damage);
                    }

                    if (other is Asteroid)
                    {
                        Asteroid asteroid3 = (Asteroid)other;
                        asteroid3.hp -= explosion.damage;
                        if (asteroid3.hp <= 0)
                        {
                            asteroid3.Kill();
                        }
                    }
                    
                    break;
                case (Enemy, Player):
                case (Player, Enemy):
                    Enemy enemy = A is Enemy ? (Enemy)A : (Enemy)B;
                    Player player5 = A is Player ? (Player)A : (Player)B;
                    player5.TakeDamage(100);
                    enemy.hp -= 100;
                    break;
                case (Enemy, Projectile):
                case (Projectile, Enemy):
                    Enemy enemy2 = A is Enemy ? (Enemy)A : (Enemy)B;
                    Projectile proj3 = A is Projectile ? (Projectile)A : (Projectile)B;
                    if (proj3.owner != enemy2.uid)
                    {
                        proj3.Kill();
                        enemy2.hp -= proj3.damage;
                        if (enemy2.hp <= 0)
                        {
                            //getPlayerWithUID(proj3.owner).AddScore(50); returns null for enemies. standby.
                            enemy2.Kill();
                        }
                    }
                    break;
            }   
        }

        foreach (GameObject go in collisionManager.GetOutOfBoundsObjects())
        {
            if (go is Player) continue; //players don't die from going out of bounds, they just can't move further in that direction. This is handled in the player update logic.
            if (go is Asteroid)
            {
                Asteroid asteroid = (Asteroid)go;
                asteroid.LifetimeFrames--;
                if (asteroid.LifetimeFrames <= 0)
                {
                    asteroid.Kill();
                }
                continue;
            }
            if (go is Enemy)
            {
                Enemy enemy = (Enemy)go;
                enemy.TargetCenter();
                continue;
            }
            go.Kill();
        }

        
        if ((DateTime.Now - counter).TotalSeconds >= AsteroidSpawnCooldownSeconds)
        {
            if (Random.Shared.NextInt64(0,10) >= 7) { // 40% chance to spawn an enemy every 2 seconds. Adjust as needed.
                Enemy e = Enemy.GenerateEnemy();
                AddGameObject(e);
            }else{
                Asteroid newAsteroid = Asteroid.GenerateAsteroid();
            
                if (players.Count > 0) newAsteroid.SetTarget(players[(int)Random.Shared.NextInt64(0,players.Count)].transform.GetPosition());
                AddGameObject(newAsteroid);
            }
            if(Random.Shared.NextInt64(0,10) >= 8)
            {
                //Enemy a = Enemy.GenerateEnemy();

                AlienSM UFO = new AlienSM(Transform.GenerateTransform(50));
                AddGameObject(UFO);
            }

            counter = DateTime.Now;
        }

        // Healthpack spawning
        /*
        if ((DateTime.Now - healthPackCounter).TotalSeconds >= HealthPackSpawnCooldownSeconds)
        {
            Healthpack hp = Healthpack.GenerateHealthPack();
            AddGameObject(hp);

            healthPackCounter = DateTime.Now;
        }
        mvoe to asteroid death! :D */
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
        Console.WriteLine("Removing player: " + p.playerNameString);
        this.players.Remove(p);
    }

    public void AddPlayer(Player p)
    {
        Console.WriteLine("adding player: " + p.playerNameString);
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
            case "Healthpack":
                newObj = new Healthpack(defaultT);
                break;
            case "Missile":
                newObj = new Missile(defaultT, new Vector2(0,0));
                break;
            case "Explosion":
                newObj = new Explosion(defaultT, new Vector2(0,0), 0f);
                break;
            case "Enemy":
                newObj = new Enemy(defaultT);
                break;
            // Add more types here as your game grows
            case "AlienSM":
                newObj = new AlienSM(defaultT);
                break;
            case "EnemyLaser":
                newObj = new EnemyLaser(defaultT, new Vector2(0, 0));
                break;
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
    public byte[] GetGameState(long timestamp)
    {
        return this.GetGameState(timestamp, players, activeObjects);
    }
    public byte[] GetGameStateCulled(long timestamp, Guid playerID)
    {
        return this.GetGameStateCulled(timestamp, playerID, players, activeObjects);
    }

    public long LoadGameState(byte[] gameState, List<ObjChangeWrapper> newObjects = null, List<ObjChangeWrapper> destroyedObjects = null)
    {
        long r = this.loadGameState(gameState, players, activeObjects);

        if (newObjects != null)
        {
            newObjects.AddRange(this.newObjectsLoaded);
        }

        if (destroyedObjects != null)
        {
            destroyedObjects.AddRange(this.destroyedObjectsLoaded);
        }
        this.newObjectsLoaded.Clear();
        this.destroyedObjectsLoaded.Clear();
        return r;
    }
    byte[] GetGameStateCulled(long timestamp, Guid playerID, params List<GameObject>[] groups)
    {
        Player p = getPlayerWithUID(playerID);
        if (p == null)        {
            return null;
        }


        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write(timestamp);
            foreach (List<GameObject> group in groups)
            {
                //write the group size.
                long countPosition = ms.Position;
                int objectsWrote = 0;
                writer.Write((int)0);
                int playerViewX = 1024;
                int playerViewY = 768;
                Rect playerView = new Rect(p.transform.rect.X, p.transform.rect.Y, playerViewX * 2, playerViewY * 2);
                foreach (GameObject go in group)
                {
                    //Add a basic culling. Can be optimized later. Server side is not starved for resources...
                    if (go.InteresectsWith(playerView))
                    {
                        objectsWrote++;
                        go.WriteMetaData(writer);
                        go.Encode(writer);
                    }


                }
                long endPosition = ms.Position; // Save the end of the stream
                ms.Position = countPosition;    // Jump back to the placeholder
                writer.Write(objectsWrote);     // Overwrite with the real count
                ms.Position = endPosition;      // Jump back to the end to continue writing
      
            }
            return ms.ToArray();
        }
    }
    //Go through all groups passed, and, in order, write their meta data and object data.
    byte[] GetGameState(long timestamp, params List<GameObject>[] groups)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write(timestamp);
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
    //give all the new objects on return! 
    long loadGameState(byte[] stateData, params List<GameObject>[] localGroups)
    {

        using (MemoryStream ms = new MemoryStream(stateData))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            long frameStamp = reader.ReadInt64();

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
                    GameObject obj = currentGroup.Find(o => o.uid.Equals(uid));

                    // 4. CREATE if it doesn't exist
                    if (obj == null)
                    {
                        //Console.WriteLine("Object with UID " + uidString + " not found, creating new " + className);
                        //Console.WriteLine("object does not exist, adding object: " + className);
                        obj = CreateGameObject(className, uidString);
                        //decode immediately to set initial values.
                        obj.Decode(reader);

                        newObjectsLoaded.Add(new ObjChangeWrapper(obj, currentGroup));
                        //currentGroup.Add(obj); let client handle the adding. Might need to change objects on creation.



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
                        destroyedObjectsLoaded.Add(new ObjChangeWrapper(currentGroup[k], currentGroup));
                        //currentGroup.RemoveAt(k); while we're at it, let the client handle removing too.
                        
                    }
                }
            }
            return frameStamp;
        }
    }
    
}
