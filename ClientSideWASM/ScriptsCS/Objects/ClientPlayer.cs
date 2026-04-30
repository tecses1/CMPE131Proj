using Shared;
using System.Drawing;
namespace ClientSideWASM;

//must extend player for render functions!
public class ClientPlayer : Player
{

    GameManager gm;

    // for health bar
    public int MaxHealth = 1000;
    private DrawRect healthBarBackground;
    private DrawRect healthBarFill;
    private int healthBarWidth = 70;
    private int healthBarHeight = 10;
    private Color currentHealthColor = Color.Green;

    //for name
    public DrawText playerName;
    public ClientPlayer( GameManager gm, Transform transform) : base( transform )
    {
        this.gm = gm;

        Transform centerTransform = new Transform(this.transform.rect.X, this.transform.rect.Y - transform.rect.Height, 100, 25);   
        playerName = new DrawText(playerNameString, centerTransform);//, 0,-transform.size.Y/2*1.25f);
        playerName.setTextColor(Color.White,200);
        playerName.worldSpace = true; //spawn relative to player, not screen space.
        // health bar in background
        Transform hbBgTransform = new Transform(
            this.transform.rect.X,
            this.transform.rect.Y + transform.rect.Height,
            healthBarWidth,
            healthBarHeight
        );
        healthBarBackground = new DrawRect( hbBgTransform);
        healthBarBackground.setFillColor(Color.DarkGray,100);
        healthBarBackground.worldSpace = true;
        healthBarBackground.borderWidth = 0;

        Transform hbFillTransform = new Transform(
            this.transform.rect.X,
            this.transform.rect.Y + transform.rect.Height,
            healthBarWidth,
            healthBarHeight
        );
        healthBarFill = new DrawRect( hbFillTransform);
        healthBarFill.setFillColor(Color.Green,100);
        healthBarFill.borderWidth = 0;
        healthBarFill.worldSpace = true;

        //register all of our things
        healthBarBackground.Register(gm);
        healthBarFill.Register(gm);
        playerName.Register(gm);

    }
    public void Deregister()
    {
        gm.UnregisterDrawText(this.playerName);
        gm.UnregisterDrawRect(healthBarBackground);
        gm.UnregisterDrawRect(healthBarFill);

    }

    public override void Render(float deltaTime)
    {
        if (IsDead)
        {
            this.healthBarBackground.disableRender = true;
            this.healthBarFill.disableRender = true;
            this.playerName.disableRender = true;
            this.disableRender = true;
            return;
        }
        this.disableRender = false;
        this.UpdateHealthBarVisual();

        //draw our healthbar.
        this.healthBarBackground.SetPosition(this.transform.rect.X, this.transform.rect.Y + this.transform.rect.Height);
        this.healthBarBackground.disableRender = false;
        //this.healthBarBackground.Draw(gm);
        this.healthBarFill.SetPosition(this.transform.rect.X, this.transform.rect.Y + this.transform.rect.Height);
        this.healthBarFill.disableRender = false;
        //this.healthBarFill.Draw(gm);
        //draw our name!
        this.playerName.SetPosition(this.transform.rect.X, this.transform.rect.Y - this.transform.rect.Height);
        this.playerName.disableRender = false;
        //this.playerName.Draw(gm);

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
        healthBarFill.transform.rect.Width = healthBarWidth * healthPercent;

        if (newColor != currentHealthColor)
        {
            healthBarFill.setFillColor(newColor,100);
            currentHealthColor = newColor;
        }
    }
}