namespace Shared;
using System;
using Blazorex;
using System.Numerics;
using ClientSideWASM;

public class ClientInputWrapper : InputWrapper
{

    private bool _prevLeftDown = false;

    public ClientInputWrapper() { }

    public void loadKeysDown(KeyboardPressEvent keysDown)

    {
        switch (keysDown.Key)
        {
            case "w": keys[0] = true; break;
            case "a": keys[1] = true; break;
            case "s": keys[2] = true; break;
            case "d": keys[3] = true; break;
            case "r": keys[4] = true; break;
            case "Escape": keys[5] = true; break;
        }
    }

    public void loadKeysUp(KeyboardPressEvent keysUp)
    {
        switch (keysUp.Key)
        {
            case "w": keys[0] = false; break;
            case "a": keys[1] = false; break;
            case "s": keys[2] = false; break;
            case "d": keys[3] = false; break;
            case "r": keys[4] = false; break;
            case "Escape": keys[5] = false; break;
        }
    }

    // Called from UI handler: supply raw mouse coords (double)
    public void loadMouseMove(double offsetX, double offsetY)
    {
        MouseX = offsetX;
        MouseY = offsetY;
    }

    // Call when mouse button pressed; "left" identifies left button
    public void loadMouseDown(bool left)
    {
        this.LeftDown = true;
        if (left)
        {
            LeftDown = true;
            if (!_prevLeftDown)
            {
                LeftPressed = true; // single frame edge
            }
            _prevLeftDown = true;
        }
    }

    public void loadMouseUp(bool left)
    {
        this.LeftDown = false;
        
        if (left)
        {
            LeftDown = false;
            _prevLeftDown = false;
            // don't clear LeftPressed here - Clear() will be called at end of frame
        }
    }

    public void OverwriteCameraToWorldPos(GameManager gm)
    {
        Vector2 overwrite = gm.CameraToWorldPos(new Vector2((float)MouseX, (float)MouseY));
        this.MouseXWorld = overwrite.X;
        this.MouseYWorld = overwrite.Y;
    }
    // Call at the end of the frame to reset single-frame edges (LeftPressed)
    public void Clear()
    {
        LeftPressed = false;
    }
    
}