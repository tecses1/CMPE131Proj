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
        // This will automatically add the key if it doesn't exist, 
        // or update it to true if it does.
        keys[keysDown.Key] = true;
    }

    public void loadKeysUp(KeyboardPressEvent keysUp)
    {
        // Update the key state to false when released.
        keys[keysUp.Key] = false;
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