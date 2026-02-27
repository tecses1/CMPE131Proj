namespace CMPE131Proj;
using System;
using Blazorex;

public class InputWrapper
{
    // WASD keys
    public bool[] keys = { false, false, false, false };

    // store mouse state ourselves (Event args from Blazorex are read-only / double)
    public double MouseX { get; private set; } = 0.0;
    public double MouseY { get; private set; } = 0.0;

    // click state we control (LeftPressed is a single-frame edge)
    public bool LeftDown { get; private set; } = false;
    public bool LeftPressed { get; private set; } = false;

    private bool _prevLeftDown = false;

    public InputWrapper() { }

    public void loadKeysDown(KeyboardPressEvent keysDown)
    {
        switch (keysDown.Key)
        {
            case "w": keys[0] = true; break;
            case "a": keys[1] = true; break;
            case "s": keys[2] = true; break;
            case "d": keys[3] = true; break;
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
        if (left)
        {
            LeftDown = false;
            _prevLeftDown = false;
            // don't clear LeftPressed here - Clear() will be called at end of frame
        }
    }

    // Call at the end of the frame to reset single-frame edges (LeftPressed)
    public void Clear()
    {
        LeftPressed = false;
    }
}