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

    public GameManager gm;

    // for health bar
    public int MaxHealth = 1000;
    private Rect healthBarBackground;
    private Rect healthBarFill;
    private int healthBarWidth = 70;
    private int healthBarHeight = 10;
    private Color currentHealthColor = Color.Green;

    [Network(0)]

    //UI elements
    public Text playerName;
    Text outOfBoundsText;
    Rect oobScreenFlashRect;
    Text scoreText;
    int alpha = 0;
    int direction = 1;

    public bool isLocalPlayer = false; // This can be used to differentiate between the local player and other players in the game.
    public LocalPlayer( GameManager gm, Transform transform) : base( transform ) {
        Transform centerTransform = new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight / 2, 100, 25);   
        playerName = new Text(playerNameString, centerTransform, 0,-transform.size.Y/2*1.25f);
        playerName.setTextColor(Color.White,200);
        playerName.worldSpace = false;
        Transform oobTransform = new Transform(Settings.CanvasWidth/2, Settings.CanvasHeight / 2, Settings.CanvasWidth/2,Settings.CanvasHeight/2);
        outOfBoundsText = new Text(Settings.OutOfBoundsMessage, oobTransform, 0,0);
        outOfBoundsText.fontColor = Settings.ErrorText;
        outOfBoundsText.worldSpace = false;

        Transform oobScreenFlashT = new Transform(Settings.CanvasWidth / 2, Settings.CanvasHeight /2,Settings.CanvasWidth,Settings.CanvasHeight);
        oobScreenFlashRect = new Rect( oobScreenFlashT);
        oobScreenFlashRect.borderWidth = 50;
        oobScreenFlashRect.worldSpace = false;

        // score system
        Transform scoreTransform = new Transform(Settings.CanvasWidth - 50, Settings.CanvasHeight - 8, 100, 125);
        scoreText = new Text("Score: 0",  scoreTransform);
        scoreText.worldSpace = false; 


        // health bar in background
        Transform hbBgTransform = new Transform(
            Settings.CanvasWidth / 2,
            Settings.CanvasHeight / 2 + transform.size.Y,
            healthBarWidth,
            healthBarHeight
        );
        healthBarBackground = new Rect( hbBgTransform);
        healthBarBackground.setFillColor(Color.DarkGray,100);
        healthBarBackground.worldSpace = false;
        healthBarBackground.borderWidth = 0;

        Transform hbFillTransform = new Transform(
            Settings.CanvasWidth / 2,
            Settings.CanvasHeight / 2 + transform.size.Y,
            healthBarWidth,
            healthBarHeight
        );
        healthBarFill = new Rect( hbFillTransform);
        healthBarFill.setFillColor(Color.Green,100);
        healthBarFill.borderWidth = 0;
        healthBarFill.worldSpace = false;

        this.gm = gm;
    

    }

    public void CenterCameraOnMe(float deltaTime)
    {
                //CENTER CAMERA ON PLAYER. MUST BE CALLED IN RENDER FUNCTION OR BIG JITTERS.
        //If we're in the world bounds.
        // added else out of bounds take damage
        bool[] collided = this.GetCollisionSides(gm.gl.GetWorldBounds()); //see if we fall out of world bounds, and what side it is.

        //gm.CenterCameraOn(this.transform);
        if (collided[0] && collided[2])//Inside Y axis bouynds
        {
            gm.CenterCameraOnLerp(this.GetLerpPosition(), deltaTime, true,false); //cente camera Y axis only.

        }
        if (collided[1] && collided[3]) //Inside X axis bounds
        {
            gm.CenterCameraOnLerp(this.GetLerpPosition(), deltaTime, false,true); //center camera X axis only
        } 
        
    }
    /*
    public override void Decode(BinaryReader reader)
    {   
        float localX = this.transform.position.X;
        float localY = this.transform.position.Y;
        float localRotation = this.transform.rotation;
        // 3. Read the server data. 
        // This overwrites this.transform with the server's past data.
        base.Decode(reader);
        // 4. Reconcile (Only for the local player)
        // Calculate how far the server's reality is from our local reality
        float dx = localX - this.transform.position.X;
        float dy = localY - this.transform.position.Y;
        float distanceSquared = (dx * dx) + (dy * dy); // Faster than Vector2.Distance

        // Threshold: How much error do we tolerate before snapping?
        // Adjust this based on your game's movement speed. 
        float snapThresholdSquared = 4.0f; // Equivalent to distance of 2.0

        if (distanceSquared < snapThresholdSquared) {
            // WE PREDICTED CORRECTLY (or close enough).
            // Revert the server's old data back to our snappy local data.
            this.transform.position.X = localX;
            this.transform.position.Y = localY;
            this.transform.rotation = localRotation;
            
            // You can also reset velocities here if you sync them
        } else {
            // SNAP CORRECTION! 
            // We hit a wall on the server, got teleported, or lagged terribly.
            // We do NOTHING here, leaving the server's data intact to snap the player.
            Console.WriteLine($"[CSP] Rubber-banding! Error distance: {Math.Sqrt(distanceSquared)}");
        }
        
    }
    */
    public void UpdateBase()
    {
        //base.Update(); // do what game logic would do, right now.
    }
    public override void Render(float deltaTime)
    {

        //gm.RenderText(playerName);
        playerName.Draw(gm);
        //Console.WriteLine("Playername pos: " + playerName.transform.position.X +"," +playerName.transform.position.Y);
        playerName.text = playerNameString;
        if (!isLocalPlayer)
        {
            return;
        }
        playerNameString = Settings.name;
        //Console.WriteLine("Score TExt pos: " + scoreText.transform.position.X +"," +scoreText.transform.position.Y);
        scoreText.Draw(gm);
        this.UpdateHealthBarVisual();
        healthBarBackground.Draw(gm);
        healthBarFill.Draw(gm);


        // if (collided[1] && collided[3]) // if we're inside the bounds on the X axis
        // {
        //     gm.CenterCameraOnLerp(this.transform,deltaTime, false,true); //center camera X axis only
        // } else {
        //     TakeDamage(1);
        // }
        


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
            oobScreenFlashRect.setFillColor(Color.Red, alpha);
            oobScreenFlashRect.setBorderColor(Color.Red, alpha);
            oobScreenFlashRect.Draw(gm);


        }
        else
        {
            alpha = 0;
        }
        


        if(!this.CollideWith(gm.gl.GetWorldBounds())){
            outOfBoundsText.Draw(gm);
            if (alpha <= 5)
            {
                direction = 1;
            }
            else if (alpha >= 75)
            {
                direction = -1;
            }
            alpha += direction;
            oobScreenFlashRect.setFillColor(Color.Red, 0);
            oobScreenFlashRect.setBorderColor(Color.Red, alpha);
            oobScreenFlashRect.Draw(gm);


        }
        else
        {
            alpha = 0;
        }
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
            healthBarFill.setFillColor(newColor,100);
            currentHealthColor = newColor;
        }
    }


    public Transform GetLerpPosition()
    {
                        // Interpolation: Calculate the interpolated position based on previous and current transform
                float interpolationOffsetX = this.transform.position.X;// - gm.worldOffsetX;
                float interpolationOffsetY =  this.transform.position.Y;// - gm.worldOffsetY;
                float interpolationRotation = this.transform.rotation;
                if (this.previousTransform != null) {
                    float prevRelX = this.previousTransform.position.X ;//- gm.worldOffsetX;
                    float prevRelY = this.previousTransform.position.Y;// - gm.worldOffsetY;
                    
                    float currRelX = this.transform.position.X;// - gm.worldOffsetX;
                    float currRelY = this.transform.position.Y;;// - gm.worldOffsetY;

                    // 2. Interpolate between the RELATIVE points
                    float t = gm.GetInterpolationFactor();
                    interpolationOffsetX = prevRelX + (currRelX - prevRelX) * t;
                    interpolationOffsetY = prevRelY + (currRelY - prevRelY) * t;


                    //interpolationOffsetX = obj.previousTransform.position.X + (obj.transform.position.X - obj.previousTransform.position.X) * GetInterpolationFactor();
                    //interpolationOffsetY = obj.previousTransform.position.Y + (obj.transform.position.Y - obj.previousTransform.position.Y) * GetInterpolationFactor();
                    float deltaRotation = (this.transform.rotation - this.previousTransform.rotation + 540) % 360 - 180;
                    float smoothedRotation = this.previousTransform.rotation + (deltaRotation * gm.GetInterpolationFactor());

                    // Use degrees for tsfs.
                    interpolationRotation = smoothedRotation;
                }
            return new Transform(interpolationOffsetX, interpolationOffsetY, (int)this.transform.size.X, (int)this.transform.size.Y, interpolationRotation);
    }

}
