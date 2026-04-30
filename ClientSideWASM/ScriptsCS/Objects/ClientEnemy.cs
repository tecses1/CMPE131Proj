namespace ClientSideWASM;
using Shared;
using System.Drawing;
public class ClientEnemy : Enemy
{
    private DrawRect healthBarBackground;
    private DrawRect healthBarFill;
    private int healthBarWidth = 70;
    private int healthBarHeight = 10;
    private Color currentHealthColor = Color.Green;

    int MaxHealth = 400;
    GameManager gm;

    public ClientEnemy(GameManager gm, Transform t) : base(t)
    {
        
    
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

        this.gm = gm;
    }
    public void Deregister()
    {
        gm.UnregisterDrawRect(healthBarFill);
        gm.UnregisterDrawRect(healthBarBackground);
    }


    public override void Render(float deltaTime)
    {
        this.UpdateHealthBarVisual();
        //Console.WriteLine("HP: " + hp);
        //draw our healthbar.
        this.healthBarBackground.SetPosition(this.transform.rect.X, this.transform.rect.Y + this.transform.rect.Height);
        //this.healthBarBackground.Draw(gm);
        this.healthBarFill.SetPosition(this.transform.rect.X, this.transform.rect.Y + this.transform.rect.Height);
        //this.healthBarFill.Draw(gm);

        //this.playerName.Draw(gm);

        //Console.WriteLine("Drawing: " + playerName.text + " @ " + playerName.transform.position.X + ", " + playerName.transform.position.Y);
    }
    private void UpdateHealthBarVisual()
    {
        float healthPercent = (float)hp / MaxHealth;

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