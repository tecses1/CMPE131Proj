namespace CMPE131Proj;

using System.Numerics;

//Handles the local player controller.
public class Player : GameObject
{
    //move comonly defined fields for classes to the GameObject class.
    //Game object class may be able to handle default rending, image fetching by name, etc.

    public InputWrapper cInput;
    private readonly List<Projectile> projectiles = new();
    private float bulletSpeed = 12f;
    private double shotCooldownSeconds = 0.12; // ~8 shots/sec
    private DateTime lastShotTime = DateTime.MinValue;
    private DateTime lastChange = DateTime.Now;
    
    //UI elements
    Text playerName;
    Text outOfBoundsText;
    
    int guntype = 1;
    bool barrel = true;
    bool[] allowedMove = {true, true, true, true}; //top, left, bottom, right
    bool[] centered = {false, false}; //x, y


    public Player(ref GameManager gm, Transform transform) : base(ref gm,transform ) {
        Transform centerTransform = new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight / 2, 100, 25);   
        playerName = new Text(Settings.name, ref centerTransform, 0,-transform.size.Y/2*1.25f);
        Transform oobTransform = new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight / 2, 200, 50);
        outOfBoundsText = new Text(Settings.OutOfBoundsMessage, ref oobTransform, 0,0);
        outOfBoundsText.fontColor = Settings.ErrorText;
    }

    public override void Update() {
        Render();
        playerName.Draw(gm);

        InputWrapper e = cInput;
        if (e == null){
            return;
        }
        //WORSHIP THY ROTATION
        //THIS WAS THE HARDEST THING I'VE HAD TO DO SO FAR ON THIS PROJECT
        //don't really like offsetting the mouse, because then its not pointing where it actually is.
        //would rather offset objects, but that hurts my brain.
        float mouseX = (float)e.MouseX + gm.worldOffsetX;
        float mouseY = (float)e.MouseY - gm.worldOffsetY;

        Vector2 mousePos = new Vector2(mouseX, mouseY);
        transform.RotateTo(mousePos);      //we're inside the bounds.


        bool[] boundsCollided = gm.GetBoundCollided(this);
        float[] center = gm.GetCanvasCenter();
        if (e.keys[0])
        {
            if (this.transform.position.Y > center[1])
            {
                transform.position.Y -= 3;
            }
            else
            {
                transform.position.Y -= 3;
                if (!boundsCollided[0]) gm.worldOffsetY = gm.worldOffsetY + 3;
            }
        }
        if (e.keys[2])
        {
            if (this.transform.position.Y < center[1])
            {
                transform.position.Y += 3;
            }
            else
            {
                transform.position.Y += 3;
                if (!boundsCollided[2]) gm.worldOffsetY = gm.worldOffsetY - 3;
            }
        }

        if (e.keys[1])
        {
            if (this.transform.position.X > center[0])
            {
                transform.position.X -= 3;
            }
            else
            {
                transform.position.X -= 3;
                if (!boundsCollided[1]) gm.worldOffsetX = gm.worldOffsetX - 3;
            }
        }
        if (e.keys[3])
        {
            if (this.transform.position.X < center[0])
            {
                transform.position.X += 3;
            }
            else
            {
                transform.position.X += 3;
                if (!boundsCollided[3]) gm.worldOffsetX = gm.worldOffsetX + 3;
            }
        }

        if(!this.CollideWith(gm.GetWorldBounds())){
            outOfBoundsText.Draw(gm);
        }


    
        if (e.keys[4] && (DateTime.Now - lastChange).Seconds > 1)
        {
            guntype++;
            if (guntype > 1)
            {
                guntype = 0;
            }
            lastChange = DateTime.Now;
        }
        /*
        if (transform.position.X > Settings.CanvasWidth + transform.size.X / 2) transform.position.X = -transform.size.X / 2;
        if (transform.position.X  < -transform.size.X / 2) transform.position.X  = Settings.CanvasWidth + transform.size.X / 2;
        if (transform.position.Y > Settings.CanvasHeight + transform.size.Y / 2) transform.position.Y = -transform.size.Y / 2;
        if (transform.position.Y < -transform.size.Y / 2) transform.position.Y = Settings.CanvasHeight + transform.size.Y / 2;
        */
        bool shotEdge = e.LeftDown;
        bool canShoot = (DateTime.UtcNow - lastShotTime).TotalSeconds >= shotCooldownSeconds;

        if (shotEdge && canShoot)
        {
            if (guntype == 0)
            {
                shotCooldownSeconds = 0.12f;
                SpawnGuntype1(mousePos);

            }else if (guntype == 1)
            {
                shotCooldownSeconds = 0.24f;
                SpawnGuntype2(mousePos);
            }
            
            lastShotTime = DateTime.UtcNow;
        }



    }


    private void SpawnGuntype1(Vector2 target)
    {
    
        Vector2 dir = target - transform.position;
        if (dir.LengthSquared() == 0f) dir = new Vector2(0, -1);
        dir = Vector2.Normalize(dir);

        float spawnOffset = MathF.Max(20, MathF.Min(transform.size.X, transform.size.Y) / 2f - 10f);
        
        Vector2 spawnPos = transform.position - dir;// * spawnOffset;
        Vector2 velocity = dir * bulletSpeed;

        Transform proj1t = new Transform(spawnPos.X-transform.Left().X*16,spawnPos.Y-transform.Left().Y*16, 7,7, transform.rotation);
        Transform proj2t = new Transform(spawnPos.X+transform.Left().X*16,spawnPos.Y+transform.Left().Y*16, 7,7, transform.rotation);
        var proj = new Projectile(ref gm, proj1t, velocity, lifetime: 30);
        var proj2 = new Projectile(ref gm, proj2t, velocity,  lifetime: 30);
        gm.AddNewGameObject(proj);
        gm.AddNewGameObject(proj2);
    }
    private void SpawnGuntype2(Vector2 target)
    {

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

        var proj = new Projectile(ref gm, proj1t, velocity, lifetime: 30);
        gm.AddNewGameObject(proj);
        barrel = !barrel;

    }
    
}
