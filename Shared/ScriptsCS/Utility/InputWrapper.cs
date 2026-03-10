namespace Shared;
using System;

public class InputWrapper
{
    public Guid owner;
    public DateTime timeStamp;
    // WASD keys
    public bool[] keys = { false, false, false, false, false, false };

    // store mouse state ourselves (Event args from Blazorex are read-only / double)
    public double MouseX { get; set; } = 0.0;
    public double MouseY { get; set; } = 0.0;

    // click state we control (LeftPressed is a single-frame edge)
    public bool LeftDown { get; set; } = false;
    public bool LeftPressed { get; set; } = false;


    public InputWrapper() { }

    
}