using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.JSInterop;
using Shared;

namespace ClientSideWASM;
public class HomeManager : RenderManager
{
    public ClientInputWrapper userInput;
    public string requestedLobby = "";

    public string failedToJoinLobby = "";

    List<GameObject> stars = new List<GameObject>();

    //text stuff
    DrawText title;
    float starSpeed = 0.1f;
    public HomeManager(IJSRuntime JS) : base(JS)
    {
        Transform titleTransform = new Transform(Settings.CanvasWidth / 2, 100, 600, 70);
        title = new DrawText("GALAX.IO", titleTransform);
        RegisterDrawTextToRender(title);
        GenerateStars();
        Transform canvasCenter = new Transform(Settings.CanvasWidth / 2, Settings.CanvasHeight / 2, 0, 0);
        this.CenterCameraOn(canvasCenter);
        
    }
    void GenerateStars()
    {
        Random r = new Random();
        // Calculate total area and desired density
        long totalArea = (long)Settings.CanvasWidth * Settings.CanvasHeight;
        
        // Instead of iterating 25 million times, we calculate how many stars we want.
        // Adjust 'DensityConstant' to get the look you want (e.g., 0.0001 for 1 star per 10k pixels)
        int starCount = (int)(totalArea * Settings.Sparseness / Math.Sqrt(totalArea) ); 
        Console.WriteLine("spawning " + starCount + " stars for bg");
        for (int i = 0; i < starCount; i++)
        {
            int x = r.Next(0, Settings.CanvasWidth);
            int y = r.Next(0, Settings.CanvasHeight);
            
            int size = (int)Math.Clamp(Settings.minSize + r.NextDouble() * Settings.maxSize, Settings.minSize, Settings.maxSize);
            
            Transform t = new Transform(x, y, size, size);
            Star s = new Star(t);
            stars.Add(s);

        }

        RegisterGroupToRender(stars);
    }
    public void ApplyInput(ClientInputWrapper input)
    {
        userInput = input;
    }
    public override void Update()
    {
        base.Update();

        foreach (Star s in stars)
        {
            s.transform.rect.X += starSpeed;
            if (s.transform.rect.X > Settings.CanvasWidth + s.transform.rect.Width)
            {
                s.transform.rect.X = 0 - s.transform.rect.Width;
            }
        }
    }

    public override void Render(float deltaTime)
    {
        base.Render(deltaTime);
    }
}