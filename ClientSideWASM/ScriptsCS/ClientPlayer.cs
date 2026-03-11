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
    Text playerName;
    public ClientPlayer( GameManager gm, Transform transform) : base( transform )
    {
        this.gm = gm;

        Transform centerTransform = new Transform(this.transform.position.X, this.transform.position.Y, 100, 25);   
        playerName = new Text(playerNameString, ref centerTransform);//, 0,-transform.size.Y/2*1.25f);
        playerName.worldSpace = true; //spawn relative to player, not screen space.
        // health bar in background
        Transform hbBgTransform = new Transform(
            this.transform.position.X,
            this.transform.position.Y + transform.size.Y,
            healthBarWidth,
            healthBarHeight
        );
        healthBarBackground = new Rect(ref hbBgTransform);
        healthBarBackground.setFillColor(Color.DarkGray);
        healthBarBackground.worldSpace = true;
        healthBarBackground.borderWidth = 0;

        Transform hbFillTransform = new Transform(
            this.transform.position.X,
            this.transform.position.Y + transform.size.Y,
            healthBarWidth,
            healthBarHeight
        );
        healthBarFill = new Rect(ref hbFillTransform);
        healthBarFill.setFillColor(Color.Green);
        healthBarFill.borderWidth = 0;
        healthBarFill.worldSpace = true;
    }


    public override void Render(float deltaTime)
    {
        
        this.UpdateHealthBarVisual();

        //draw our healthbar.
        this.healthBarBackground.Draw(gm);
        this.healthBarFill.Draw(gm);
        //draw our name!
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
            healthBarFill.setFillColor(newColor);
            currentHealthColor = newColor;
        }
    }
}