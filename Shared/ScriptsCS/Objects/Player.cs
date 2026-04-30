namespace Shared;

using System.Diagnostics;
using System.Drawing;
using System.Numerics;


//Handles the local player controller.
public class Player : GameObject
{
    //move comonly defined fields for classes to the GameObject class.
    //Game object class may be able to handle default rending, image fetching by name, etc.

    public InputWrapper cInput;
    private float bulletSpeed = 30f;
    private double shotCooldownSeconds = 0.12; // ~8 shots/sec
    private double missleCooldownSeconds = 1.2f; // ~8 shots/sec
    private DateTime lastShotTime = DateTime.MinValue;
    private DateTime lastMissleTime = DateTime.MinValue;    
    private DateTime lastChange = DateTime.Now;
    [Network(2)]
    public int Score { get;  set; } = 0;
    [Network(1)]
    public string playerNameString {get; set;} = "Player";
    //behavior
    float maxSpeed = 6f;
    float acceleration = 2f;

    float drag = 0.11f;
    [Network(0)]
    public int CurrentHealth { get;  set; }= 1000;
    
    int guntype = 1;
    bool barrel = true;
    int shooting = -1;
    [Network(4)]
    public int MissileAmmo {get; set;} = 3;
    public int mines = 0;

    // death animation
    [Network(3)] // Add _isDead to the network.
    public bool IsDead {get; set;} = false;
    private DateTime deathTime;
    private TimeSpan respawnDelay = TimeSpan.FromSeconds(5);

    private Vector2 spawnPoint = new Vector2(0, 0); // center of map
    private Vector2 defaultSize = new Vector2(50, 50);
    private int defaultHealth = 1000;

    public Player(Transform transform) : base(transform ) {

    }

    public static bool IsNearlyZero(float value, float epsilon = 0.0001f)
    {
        return Math.Abs(value) < epsilon;
    }



    public override void Update() {

        if (IsDead)
        {
            // Check if respawn time has passed
            if (DateTime.Now - deathTime >= respawnDelay)
            {
                Respawn();
            }
            return; // skip input & movement while dead
        }


        InputWrapper e = cInput;
        if (e == null){
            Console.WriteLine("warning, Null input.");
            return;
        }
        //Only update player rotation when the input is valid.
        Vector2 mousePos = new Vector2((float)cInput.MouseXWorld, (float)cInput.MouseYWorld);
        transform.RotateTo(mousePos); 
        //Console.WriteLine("Player pos: " + transform.position.X+"," + transform.position.Y + " lookat: " + mousePos.X + ", " + mousePos.Y + ", ROTIATION: " + transform.rotation);
        



        //Find what sides we're colliding with in closwise order from top. 
        // 1. Movement Input (Calculates direction * acceleration)
        transform.velocity.Y += (e.IsKeyDown("s",true) ? acceleration : 0) - (e.IsKeyDown("w",true) ? acceleration : 0);
        transform.velocity.X += (e.IsKeyDown("d",true) ? acceleration : 0) - (e.IsKeyDown("a",true) ? acceleration : 0);

        // 2. Clamp Velocity (Native C# function)
        transform.velocity.X = Math.Clamp(transform.velocity.X, -maxSpeed, maxSpeed);
        transform.velocity.Y = Math.Clamp(transform.velocity.Y, -maxSpeed, maxSpeed);

        // 3. Apply Drag (Reduces velocity by a percentage)
        //Only when keys are not being pressed (slow to stop)
        if ((e.IsKeyDown("w",true) || e.IsKeyDown("s",true)) == false) transform.velocity.Y *= (1 - drag);
        if ((e.IsKeyDown("a",true) || e.IsKeyDown("d")) == false) transform.velocity.X *= (1 - drag);


        // 4. Zero-out and Move
        if (Math.Abs(transform.velocity.X) < 0.01f) transform.velocity.X = 0;
        if (Math.Abs(transform.velocity.Y) < 0.01f) transform.velocity.Y = 0;
        
        //this.transform.position += cVelocity;

        this.transform.Update();


    
        if (e.IsKeyDown("r",true) && (DateTime.Now - lastChange).Seconds > 1)
        {
            guntype++;
            if (guntype > 1) //to incorporate the missile
            {
                guntype = 0;
            }
            lastChange = DateTime.Now;
        }

        bool shotEdge = e.LeftDown;
        bool canShoot = (DateTime.UtcNow - lastShotTime).TotalSeconds >= shotCooldownSeconds;
        bool canShootMissle = (DateTime.UtcNow - lastMissleTime).TotalSeconds >= missleCooldownSeconds;

        //this.shooting = -1;
        if (shotEdge && canShoot)
        {        

                if (guntype == 0)
                {
                    GameObject[] shots = SpawnGuntype1(mousePos);
                    this.gl.AddGameObject(shots[0]);
                    this.gl.AddGameObject(shots[1]);


                }else if (guntype == 1)
                {
                    GameObject shot = SpawnGuntype2(mousePos);
                    this.gl.AddGameObject(shot);
                }else if (guntype == 2) // added for misile implementation
                {
                }

            lastShotTime = DateTime.UtcNow;
        }


        if (canShootMissle && e.IsKeyDown(" ", true)){
            if (this.MissileAmmo > 0){
                GameObject[] targets = this.gl.collisionManager.GetNearby(mousePos,400);
                GameObject target = null;
                if (targets.Length != 0)
                {
                    foreach (GameObject go in targets)
                    {
                        if (go != this && (go is Player || go is Asteroid || go is Enemy)) // only target other players or asteroids for now.
                        {
                            target = go;
                            break;
                        }
                    }
                }
                GameObject shot = SpawnMissile(target, mousePos);
                this.gl.AddGameObject(shot);
                this.MissileAmmo--;
                lastMissleTime = DateTime.UtcNow;
                //Console.WriteLine("Missile: " + this.MissileAmmo);
            }
          
        }


    }



    private Projectile[] SpawnGuntype1(Vector2 target)
    {
        shotCooldownSeconds = 0.09f;
        Vector2 dir = target - transform.GetPosition();
        if (dir.LengthSquared() == 0f) dir = new Vector2(0, -1);
        dir = Vector2.Normalize(dir);

        float spawnOffset = MathF.Max(20, MathF.Min(transform.rect.Width, transform.rect.Height) / 2f - 10f);
        
        Vector2 spawnPos = transform.GetPosition() - dir;// * spawnOffset;
        Vector2 velocity = dir * bulletSpeed * 1.25f; //boost speed for smaller bullets. 

        Transform proj1t = new Transform(spawnPos.X-transform.Left().X*16,spawnPos.Y-transform.Left().Y*16, 7,7, transform.rotation);
        Transform proj2t = new Transform(spawnPos.X+transform.Left().X*16,spawnPos.Y+transform.Left().Y*16, 7,7, transform.rotation);
        var proj = new Projectile( proj1t, velocity, lifetime: 12);
        var proj2 = new Projectile(proj2t, velocity, lifetime: 12);

        proj.owner = this.uid;
        proj2.owner = this.uid;

        
        proj.damage = 8;
        proj2.damage = 8;

        
        return new Projectile[] {proj,proj2};
    }

    private Projectile SpawnGuntype2(Vector2 target)
    {
        shotCooldownSeconds = 0.24f;
        Vector2 dir = target - transform.GetPosition();
        if (dir.LengthSquared() == 0f) dir = new Vector2(0, -1);
        dir = Vector2.Normalize(dir);

        
        Vector2 spawnPos = transform.GetPosition() - dir;// * spawnOffset;
        int offset = 16;
        if (barrel)
        {
            offset = -16;
        }
        Transform proj1t = new Transform(spawnPos.X-transform.Left().X*offset,spawnPos.Y-transform.Left().Y*offset, 15,15);
        proj1t.RotateTo(target);
        Vector2 velocity = proj1t.Forward() * bulletSpeed / 8;

        var proj = new Projectile(proj1t, velocity, lifetime: 24);
        
        proj.owner = this.uid;
        proj.damage = 36;
        proj.transform.acceleration = Vector2.Normalize(velocity) * 2.85f; //gradually increase speed over time for the big bullet.    

        
        barrel = !barrel;
        return proj;
    }

        private GameObject SpawnMissile(GameObject target, Vector2 mousePos)
    {
        //shotCooldownSeconds = 2.0f;

        //string newUid = Guid.NewGuid().ToString(); 
        //GameObject shot = this.gl.CreateGameObject("Missile", newUid); 
        // 1. Force position to player
        //shot.transform.rect.X = this.transform.rect.X;
        //shot.transform.rect.Y = this.transform.rect.Y;

        // 2. Fix the veer
        //Vector2 dir = mousePos - this.transform.GetPosition();
        //if (dir.LengthSquared() == 0f) dir = new Vector2(0, -1);
        //dir = Vector2.Normalize(dir);

        Transform t = new Transform(transform.rect.X, transform.rect.Y, 15,20);

        t.RotateTo(mousePos);
        Missile m = new Missile(t, t.Forward());
        
        m.target = target;
        m.LifetimeFrames = 120;
        m.owner = this.uid;

        return m;
    }

    //score
    public void AddScore(int points)
    {
        Score += points;
    }

    // ship damage
    public void Heal(int heal)
    {
        if (IsDead) return;
        CurrentHealth = Math.Min(CurrentHealth + heal, defaultHealth);
        // if (CurrentHealth <= 0)
        // {
        //     CurrentHealth = 0;
        //     Kill();
        // }
    }
    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Kill();
        }
    }
    public void Kill()
    {
        if (IsDead) return;
        IsDead = true;
        deathTime = DateTime.Now;
        disableCollision = true;
        transform.velocity = Vector2.Zero;
        Console.WriteLine($"{playerNameString} died!");
    }
    private void Respawn()
    {
        transform.SetPosition(spawnPoint);
        //transform.size = defaultSize;
        CurrentHealth = defaultHealth;
        IsDead = false;
        disableCollision = false;
        Console.WriteLine($"{playerNameString} respawned at {spawnPoint.X},{spawnPoint.Y}");
    }

}
