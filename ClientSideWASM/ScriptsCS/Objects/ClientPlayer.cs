using Shared;
using System.Drawing;
namespace ClientSideWASM;

//must extend player for render functions!
public class ClientPlayer : Player
{

    GameManager gm;

    // for health bar
    public int MaxHealth = 1000;
    private Rect healthBarBackground;
    private Rect healthBarFill;
    private int healthBarWidth = 70;
    private int healthBarHeight = 10;
    private Color currentHealthColor = Color.Green;

    //for name
    public Text playerName;
    public ClientPlayer( GameManager gm, Transform transform) : base( transform )
    {
        this.gm = gm;

        Transform centerTransform = new Transform(this.transform.position.X, this.transform.position.Y - transform.size.Y, 100, 25);   
        playerName = new Text(playerNameString, centerTransform);//, 0,-transform.size.Y/2*1.25f);
        playerName.setTextColor(Color.White,200);
        playerName.worldSpace = true; //spawn relative to player, not screen space.
        // health bar in background
        Transform hbBgTransform = new Transform(
            this.transform.position.X,
            this.transform.position.Y + transform.size.Y,
            healthBarWidth,
            healthBarHeight
        );
        healthBarBackground = new Rect( hbBgTransform);
        healthBarBackground.setFillColor(Color.DarkGray,100);
        healthBarBackground.worldSpace = true;
        healthBarBackground.borderWidth = 0;

        Transform hbFillTransform = new Transform(
            this.transform.position.X,
            this.transform.position.Y + transform.size.Y,
            healthBarWidth,
            healthBarHeight
        );
        healthBarFill = new Rect( hbFillTransform);
        healthBarFill.setFillColor(Color.Green,100);
        healthBarFill.borderWidth = 0;
        healthBarFill.worldSpace = true;
    }


    public override void Render(float deltaTime)
    {
        
        this.UpdateHealthBarVisual();

        //draw our healthbar.
        this.healthBarBackground.SetPosition(this.transform.position.X, this.transform.position.Y + this.transform.size.Y);
        this.healthBarBackground.Draw(gm);
        this.healthBarFill.SetPosition(this.transform.position.X, this.transform.position.Y + this.transform.size.Y);
        this.healthBarFill.Draw(gm);
        //draw our name!
        this.playerName.SetPosition(this.transform.position.X, this.transform.position.Y - this.transform.size.Y);
        this.playerName.Draw(gm);

        //Console.WriteLine("Drawing: " + playerName.text + " @ " + playerName.transform.position.X + ", " + playerName.transform.position.Y);
    }
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