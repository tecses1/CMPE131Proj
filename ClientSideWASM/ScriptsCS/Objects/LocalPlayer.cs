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



    public override void Decode(BinaryReader reader)
    {
        base.Decode(reader);
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

        //CENTER CAMERA ON PLAYER. MUST BE CALLED IN RENDER FUNCTION OR BIG JITTERS.
        //If we're in the world bounds.
        // added else out of bounds take damage
        bool[] collided = this.GetCollisionSides(gm.gl.GetWorldBounds()); //see if we fall out of world bounds, and what side it is.

        //gm.CenterCameraOn(this.transform);
        if (collided[0] && collided[2] && collided[1] && collided[3]) // if we're inside the bounds on the Y axis and X axis now
        {
            gm.CenterCameraOnLerp(this.transform,deltaTime, true,false); //cente camera Y axis only.
            gm.CenterCameraOnLerp(this.transform,deltaTime, false,true); //center camera X axis only
        } else {
            TakeDamage(1);
        }
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

}
