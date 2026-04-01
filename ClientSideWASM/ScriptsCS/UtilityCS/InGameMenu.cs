namespace ClientSideWASM;
using Shared;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public class InGameMenu
{
    GameManager gm;

    public DrawText title;
    public DrawText exitButton;

    public DrawRect backgroundRect;
    Transform mouse  = new Transform(0,0,10,10);

    public bool isVisible = false;

    public InGameMenu(GameManager gm)
    {
        this.gm = gm;

        Transform backgroundTransform = new Transform(Settings.CanvasWidth / 2, Settings.CanvasHeight / 2, 300, 500);
        backgroundRect = new DrawRect(backgroundTransform);
        backgroundRect.setFillColor(Color.Gray,200);
        backgroundRect.setBorderColor(Color.LightGray,255);
        backgroundRect.worldSpace = false;

        Transform titleTransform = new Transform(Settings.CanvasWidth / 2, Settings.CanvasHeight / 2 - 200, 200, 50);
        title = new DrawText("Menu", titleTransform);
        title.setTextColor(Color.White, 255);
        title.worldSpace = false; // UI element

        Transform exitButtonTransform = new Transform(Settings.CanvasWidth / 2, Settings.CanvasHeight / 2 + 100, 200, 50);
        exitButton = new DrawText("Exit Game", exitButtonTransform);
        exitButton.setTextColor(Color.White, 255);
        exitButton.worldSpace = false; // UI element


        // Register the menu elements with the GameManager for rendering
        backgroundRect.Register(gm);
        title.Register(gm);
        exitButton.Register(gm);

        Hide(); // Start hidden
    }

    public void Show()
    {
        backgroundRect.disableRender = false;
        title.disableRender = false;
        exitButton.disableRender = false;
    }

    public void Hide()
    {
        backgroundRect.disableRender = true;
        title.disableRender = true;
        exitButton.disableRender = true;
    }

    public void UpdateInput(ClientInputWrapper input)
    {
        if (input.CKeyPressed("Escape"))
        {
            isVisible = !isVisible; // Toggle visibility
        }
        mouse.rect.X = (float)input.MouseX;
        mouse.rect.Y = (float)input.MouseY;
        if (isVisible)
        {
            this.Show();

            if (mouse.rect.IntersectsWith(exitButton.transform.rect))
            {
                // Handle exit button click
                exitButton.setTextColor(Color.Blue, 255);
                if (input.LeftDown)
                {
                    Console.WriteLine("LEAVE LOBBY OR SUMN");
                }
            }
            else
            {
                exitButton.setTextColor(Color.White, 255);
            }
        }
        else
        {
            this.Hide();
        }


        
    }
}