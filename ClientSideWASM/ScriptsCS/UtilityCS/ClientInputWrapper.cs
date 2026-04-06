namespace Shared;
using System;
using Blazorex;
using System.Numerics;
using ClientSideWASM;

public class ClientInputWrapper : InputWrapper
{

    public Dictionary<string, bool> keysPressed = new Dictionary<string, bool>();


    public bool CLeftDown
    {
        get { return LeftDown; }
        set
        {
            // If it is being pressed down right now, AND it wasn't already down...
            if (value == true && LeftDown == false)
            {
                LeftPressed = true; // Queue up the single-frame press!
            }
            
            // Update the actual held state
            LeftDown = value;
        }
    }

    public bool CLeftPressed
    {
        get
        {
            // If there's a press queued up, consume it and return true
            if (LeftPressed)
            {
                LeftPressed = false; 
                return true;
            }
            return false;
        }
        set 
        { 
            // Standard setter in case you ever need to manually force/reset it
            LeftPressed = value; 
        }
    }
    public ClientInputWrapper() { }

    public void loadKeysDown(KeyboardPressEvent keysDown)
    {
        string k = keysDown.Key;

        // Check the CURRENT state before we update it
        // TryGetValue prevents a crash if this key has never been pressed before
        bool wasAlreadyDown = keys.TryGetValue(k, out bool state) && state;

        // If it is being pressed down right now, AND it wasn't already down...
        if (!wasAlreadyDown)
        {
            keysPressed[k] = true; // Queue up the single-frame press!
        }

        // Update the actual held state
        keys[k] = true;
    }

    public void loadKeysUp(KeyboardPressEvent keysUp)
    {
        // Update the key state to false when released
        keys[keysUp.Key] = false;
        
        // Safety cleanup: ensuring the queued press doesn't get stuck 
        // if the game logic missed it
        keysPressed[keysUp.Key] = false; 
    }
    public bool CKeyPressed(string key)
    {
        // If the key is in the dictionary and is currently queued as 'true'...
        if (keysPressed.TryGetValue(key, out bool isPressed) && isPressed)
        {
            // Consume it!
            keysPressed[key] = false; 
            return true;
        }
        
        return false;
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
        this.CLeftDown = true;
    }

    public void loadMouseUp(bool left)
    {
        this.LeftDown = false;
        this.LeftPressed = false;
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