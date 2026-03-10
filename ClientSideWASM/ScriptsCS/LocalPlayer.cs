namespace ClientSideWASM;

using Shared;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;


//Handles the local player controller.
public class LocalPlayer : Player 
{
    //move comonly defined fields for classes to the GameObject class.
    //Game object class may be able to handle default rending, image fetching by name, etc.

    public InputWrapper cInput;
    public GameManager gm;
    private readonly List<Projectile> projectiles = new();
    private float bulletSpeed = 12f;
    private double shotCooldownSeconds = 0.12; // ~8 shots/sec
    private DateTime lastShotTime = DateTime.MinValue;
    private DateTime lastChange = DateTime.Now;

    // for health bar
    public int MaxHealth = 1000;
    private Rect healthBarBackground;
    private Rect healthBarFill;
    private int barOffsetX;
    private int barOffsetY;
    private int healthBarWidth = 70;
    private int healthBarHeight = 10;
    private Color currentHealthColor = Color.Green;

    //behavior
    float maxSpeed = 5f;
    float acceleration = 0.33f;

    float drag = 0.05f;
    [Network(0)]

    //UI elements
    public Text playerName;
    Text outOfBoundsText;
    Rect oobScreenFlashRect;
    Text scoreText;

    
    int guntype = 1;
    bool barrel = true;
    Vector2 cVelocity = new Vector2(0,0);
    int alpha = 0;
    int direction = 1;
    int shooting = -1;

    public bool isLocalPlayer = false; // This can be used to differentiate between the local player and other players in the game.
    public LocalPlayer( GameManager gm, Transform transform) : base( transform ) {
        Transform centerTransform = new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight / 2, 100, 25);   
        playerName = new Text(Settings.name, ref centerTransform, 0,-transform.size.Y/2*1.25f);
        playerName.worldSpace = false;
        Transform oobTransform = new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight / 2, Settings.CanvasWidth/2,Settings.CanvasHeight/2);
        outOfBoundsText = new Text(Settings.OutOfBoundsMessage, ref oobTransform, 0,0);
        outOfBoundsText.fontColor = Settings.ErrorText;
        outOfBoundsText.worldSpace = false;

        Transform oobScreenFlashT = new Transform(Settings.CanvasWidth / 2, Settings.CanvasHeight /2,Settings.CanvasWidth,Settings.CanvasHeight);
        oobScreenFlashRect = new Rect(ref oobScreenFlashT);
        oobScreenFlashRect.borderWidth = 50;
        oobScreenFlashRect.worldSpace = false;

        // score system
        Transform scoreTransform = new Transform(Settings.CanvasWidth - 50, Settings.CanvasHeight - 8, 100, 125);
        scoreText = new Text("Score: 0", ref scoreTransform);
        scoreText.worldSpace = false; 

        // offset for health bar relative to player
        barOffsetX = 0;
        barOffsetY = (int)transform.size.Y / 2 + 10;

        // health bar in background
        Transform hbBgTransform = new Transform(
            Settings.CanvasWidth / 2,
            Settings.CanvasHeight / 2 + transform.size.Y,
            healthBarWidth,
            healthBarHeight
        );
        healthBarBackground = new Rect(ref hbBgTransform);
        healthBarBackground.setFillColor(Color.DarkGray);
        healthBarBackground.worldSpace = false;

        Transform hbFillTransform = new Transform(
            Settings.CanvasWidth / 2,
            Settings.CanvasHeight / 2 + transform.size.Y,
            healthBarWidth,
            healthBarHeight
        );
        healthBarFill = new Rect(ref hbFillTransform);
        healthBarFill.setFillColor(Color.Green);
        healthBarFill.worldSpace = false;

        this.gm = gm;
    

    }



    public override void Decode(BinaryReader reader)
    {
        base.Decode(reader);
    }
    /*
    public override void Update() {



        InputWrapper e = cInput;
        if (e == null){
            return;
        }
        
        Vector2 mousePos = gm.CameraToWorldPos((float)e.MouseX, (float)e.MouseY);
        transform.RotateTo(mousePos);      //we're inside the bounds.



        //Find what sides we're colliding with in closwise order from top. 
        if (e.keys[0])
        {
            cVelocity.Y -= acceleration;
        }
        
        if (e.keys[2])
        {
            cVelocity.Y += acceleration;
        }
        if (e.keys[1])
        {
            cVelocity.X -= acceleration;
        }
        if (e.keys[3])
        {
            cVelocity.X += acceleration;
        }

        //clamp velocity.
        if (cVelocity.X >= maxSpeed)
        {
            cVelocity.X = maxSpeed;
        }
        if (cVelocity.X <= -maxSpeed)
        {
            cVelocity.X = -maxSpeed;
        }
        if (cVelocity.Y >= maxSpeed)
        {
            cVelocity.Y = maxSpeed;
        }
        if (cVelocity.Y <= -maxSpeed)
        {
            cVelocity.Y = -maxSpeed;
        }
        if (cVelocity.X > 0)
        {
            cVelocity.X -= drag * cVelocity.X;
        }

        if (cVelocity.X < 0)
        {
            cVelocity.X += drag * Math.Abs(cVelocity.X);
        }

        if (cVelocity.Y > 0)
        {
            cVelocity.Y -= drag * cVelocity.Y;
        }
        if (cVelocity.Y < 0)
        {
            cVelocity.Y += drag * Math.Abs(cVelocity.Y);
        }

        if (IsNearlyZero(cVelocity.X))
        {
            cVelocity.X = 0;
        }
        if (IsNearlyZero(cVelocity.Y))
        {
            cVelocity.Y = 0;
        }
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
                    
                    SpawnGuntype1(mousePos);

                }else if (guntype == 1)
                {
                    SpawnGuntype2(mousePos);
                }

            lastShotTime = DateTime.UtcNow;
        }

        // health bar relative to player
        healthBarBackground.transform.position = transform.position + new Vector2(barOffsetX, barOffsetY);
        healthBarFill.transform.position = transform.position + new Vector2(barOffsetX, barOffsetY);
        UpdateHealthBarVisual();
}
    */
    public override void Render()
    {
        //gm.RenderText(playerName);
        playerName.Draw(gm);
        Console.WriteLine("Playername pos: " + playerName.transform.position.X +"," +playerName.transform.position.Y);
        playerName.text = playerNameString;
        if (!isLocalPlayer)
        {
            return;
        }
        playerNameString = Settings.name;
        Console.WriteLine("Score TExt pos: " + scoreText.transform.position.X +"," +scoreText.transform.position.Y);
        scoreText.Draw(gm);

        healthBarBackground.Draw(gm);
        healthBarFill.Draw(gm);

        //CENTER CAMERA ON PLAYER. MUST BE CALLED IN RENDER FUNCTION OR BIG JITTERS.
        //If we're in the world bounds.
        // added else out of bounds take damage
        bool[] collided = this.GetCollisionSides(gm.gl.GetWorldBounds()); //see if we fall out of world bounds, and what side it is.

        //gm.CenterCameraOn(this.transform);
        if (collided[0] && collided[2]) // if we're inside the bounds on the Y axis
        {
            gm.CenterCameraOn(this.transform,true,false); //cente camera Y axis only.
        } else {
            TakeDamage(1);
        }
        if (collided[1] && collided[3]) // if we're inside the bounds on the X axis
        {
            gm.CenterCameraOn(this.transform,false,true); //center camera X axis only
        } else {
            TakeDamage(1);
        }
        


        if(!this.CollideWith(gm.gl.GetWorldBounds())){
            outOfBoundsText.Draw(gm);
            if (alpha <= 25)
            {
                direction = 1;
            }
            else if (alpha >= 75)
            {
                direction = -1;
            }
            alpha += direction;
            
            oobScreenFlashRect.setBorderColor(Color.Red, alpha);
            oobScreenFlashRect.Draw(gm);


        }
        else
        {
            alpha = 0;
        }
        


        if(!this.CollideWith(gm.gl.GetWorldBounds())){
            outOfBoundsText.Draw(gm);
            if (alpha <= 25)
            {
                direction = 1;
            }
            else if (alpha >= 75)
            {
                direction = -1;
            }
            alpha += direction;
            
            oobScreenFlashRect.setBorderColor(Color.Red, alpha);
            oobScreenFlashRect.Draw(gm);


        }
        else
        {
            alpha = 0;
        }
    }

    



    // ship damage
    public void TakeDamage(int damage)
    {
        UpdateHealthBarVisual(); //damage updates on server.
    
    }

    // health bar
    private void UpdateHealthBarVisual()
    {
        float healthPercent = (float)CurrentHealth / MaxHealth;

        Color newColor;
        bool dead = false;
        if (healthPercent > 0.5f) newColor = Color.Green;
        else if (healthPercent > 0.25f) newColor = Color.Yellow;
        else if (healthPercent > 0f) newColor = Color.Red;
        else {
            dead = true;
            newColor = Color.Red;
        }
        if (dead) {
            healthPercent = 1f;
        }
        // scale health width
        healthBarFill.transform.size.X = healthBarWidth * healthPercent;

        if (newColor != currentHealthColor)
        {
            healthBarFill.setFillColor(newColor);
            currentHealthColor = newColor;
        }
    }

}
