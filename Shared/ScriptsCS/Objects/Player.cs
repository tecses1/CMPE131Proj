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
    private DateTime lastShotTime = DateTime.MinValue;
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
    Vector2 cVelocity = new Vector2(0,0);
    int shooting = -1;

    public Player(Transform transform) : base(transform ) {

    }

    public static bool IsNearlyZero(float value, float epsilon = 0.0001f)
    {
        return Math.Abs(value) < epsilon;
    }



    public override void Update() {


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
        cVelocity.Y += (e.keys[2] ? acceleration : 0) - (e.keys[0] ? acceleration : 0);
        cVelocity.X += (e.keys[3] ? acceleration : 0) - (e.keys[1] ? acceleration : 0);

        // 2. Clamp Velocity (Native C# function)
        cVelocity.X = Math.Clamp(cVelocity.X, -maxSpeed, maxSpeed);
        cVelocity.Y = Math.Clamp(cVelocity.Y, -maxSpeed, maxSpeed);

        // 3. Apply Drag (Reduces velocity by a percentage)
        //Only when keys are not being pressed (slow to stop)
        if ((e.keys[0] || e.keys[2]) == false) cVelocity.Y *= (1 - drag);
        if ((e.keys[1] || e.keys[3]) == false) cVelocity.X *= (1 - drag);


        // 4. Zero-out and Move
        if (Math.Abs(cVelocity.X) < 0.01f) cVelocity.X = 0;
        if (Math.Abs(cVelocity.Y) < 0.01f) cVelocity.Y = 0;

        this.transform.position += cVelocity;




    
        if (e.keys[4] && (DateTime.Now - lastChange).Seconds > 1)
        {
            guntype++;
            if (guntype > 1)
            {
                guntype = 0;
            }
            lastChange = DateTime.Now;
        }

        bool shotEdge = e.LeftDown;
        bool canShoot = (DateTime.UtcNow - lastShotTime).TotalSeconds >= shotCooldownSeconds;
        this.shooting = -1;
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
                }

            lastShotTime = DateTime.UtcNow;
        }

    }

    private Projectile[] SpawnGuntype1(Vector2 target)
    {
        shotCooldownSeconds = 0.09f;
        Vector2 dir = target - transform.position;
        if (dir.LengthSquared() == 0f) dir = new Vector2(0, -1);
        dir = Vector2.Normalize(dir);

        float spawnOffset = MathF.Max(20, MathF.Min(transform.size.X, transform.size.Y) / 2f - 10f);
        
        Vector2 spawnPos = transform.position - dir;// * spawnOffset;
        Vector2 velocity = dir * bulletSpeed * 1.25f; //boost speed for smaller bullets. 

        Transform proj1t = new Transform(spawnPos.X-transform.Left().X*16,spawnPos.Y-transform.Left().Y*16, 7,7, transform.rotation);
        Transform proj2t = new Transform(spawnPos.X+transform.Left().X*16,spawnPos.Y+transform.Left().Y*16, 7,7, transform.rotation);
        var proj = new Projectile( proj1t, velocity, lifetime: 20);
        var proj2 = new Projectile(proj2t, velocity, lifetime: 20);

        proj.owner = this.uid;
        proj2.owner = this.uid;

        
        proj.damage = 6;
        proj2.damage = 6;

        
        return new Projectile[] {proj,proj2};
    }

    private Projectile SpawnGuntype2(Vector2 target)
    {
        shotCooldownSeconds = 0.24f;
        Vector2 dir = target - transform.position;
        if (dir.LengthSquared() == 0f) dir = new Vector2(0, -1);
        dir = Vector2.Normalize(dir);

        
        Vector2 spawnPos = transform.position - dir;// * spawnOffset;
        int offset = 16;
        if (barrel)
        {
            offset = -16;
        }
        Transform proj1t = new Transform(spawnPos.X-transform.Left().X*offset,spawnPos.Y-transform.Left().Y*offset, 15,15);
        proj1t.RotateTo(target);
        Vector2 velocity = proj1t.Forward() * bulletSpeed;

        var proj = new Projectile(proj1t, velocity, lifetime: 56);
        proj.owner = this.uid;
        proj.damage = 18;

        
        barrel = !barrel;
        return proj;
    }

    //score
    public void AddScore(int points)
    {
        Score += points;
    }

    // ship damage
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth < 0) CurrentHealth = 0;

        // check death
        if (CurrentHealth == 0)
        {  
            this.Kill();
            Console.WriteLine("Should be dead but no respawn yet");
        }
    }


}
