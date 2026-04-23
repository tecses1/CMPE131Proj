namespace Shared;
using System;
using System.Collections.Generic;
using Blazorex;
using System.Numerics;
using ClientSideWASM;
using System.Diagnostics;
using System.Linq;

public class ClientInputWrapper : InputWrapper
{
    // Dictionary to track which keys were JUST pressed this frame
    public Dictionary<string, bool> keysPressed = new Dictionary<string, bool>();

    public bool CLeftDown
    {
        get => LeftDown;
        set
        {
            // If it's a fresh press, set LeftPressed to true for the duration of this frame
            if (value && !LeftDown)
            {
                LeftPressed = true;
            }
            LeftDown = value;
        }
    }

    // Simple pass-through properties. 
    // No logic in the getter means the debugger won't break anything.
    public bool CLeftPressed => LeftPressed;

    public ClientInputWrapper() { }

    public void loadKeysDown(KeyboardPressEvent keysDown)
    {
        string k = keysDown.Key;
        bool wasAlreadyDown = keys.TryGetValue(k, out bool state) && state;

        if (!wasAlreadyDown)
        {
            keysPressed[k] = true; 
        }

        keys[k] = true;
    }

    public void loadKeysUp(KeyboardPressEvent keysUp)
    {
        keys[keysUp.Key] = false;
    }

    public bool CKeyPressed(string key)
    {
        return keysPressed.TryGetValue(key, out bool isPressed) && isPressed;
    }

    public void loadMouseDown(bool left) => this.CLeftDown = true;
    public void loadMouseUp(bool left) => this.CLeftDown = false;

    public void loadMouseMove(double offsetX, double offsetY)
    {
        MouseX = offsetX;
        MouseY = offsetY;
    }

    /// <summary>
    /// Call this at the VERY END of your Game Loop (after Update, Render, and Networking).
    /// This turns "Pressed" signals into "Down" signals for the next frame.
    /// </summary>
    public void Flush()
    {
        // Clear the single-frame mouse signal
        LeftPressed = false;

        // Clear all single-frame keyboard signals
        // Using a list to avoid "collection modified" errors if you were to add keys mid-flush
        foreach (var key in keysPressed.Keys.ToList())
        {
            keysPressed[key] = false;
        }
    }

    public void OverwriteCameraToWorldPos(GameManager gm)
    {
        Vector2 overwrite = gm.CameraToWorldPos(new Vector2((float)MouseX, (float)MouseY));
        this.MouseXWorld = overwrite.X;
        this.MouseYWorld = overwrite.Y;
    }
}