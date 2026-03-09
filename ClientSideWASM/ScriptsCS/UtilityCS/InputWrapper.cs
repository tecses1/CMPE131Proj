namespace ClientSideWASM;
using System;
using Blazorex;

public class InputWrapper
{
    public Guid owner;
    public DateTime timeStamp;
    // WASD keys
    public bool[] keys = { false, false, false, false, false, false };

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

    // Call at the end of the frame to reset single-frame edges (LeftPressed)
    public void Clear()
    {
        LeftPressed = false;
    }


    public byte[] Encode()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            //write the UID owner.
            writer.Write(owner.ToString());
            //Write the timestamp.
            writer.Write(timeStamp.ToBinary());
            // Write keys as a single byte (bitmask)
            byte keyByte = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i]) keyByte |= (byte)(1 << i);
            }
            writer.Write(keyByte);

            // Write mouse position as two doubles
            writer.Write(MouseX);
            writer.Write(MouseY);

            // Write mouse button state as a single byte
            byte mouseByte = 0;
            if (LeftDown) mouseByte |= 1; // bit 0 for left button
            writer.Write(mouseByte);


            return ms.ToArray();

        }
        
    }
    public static InputWrapper Decode(byte[] playerInput)
    {
        using (MemoryStream ms = new MemoryStream(playerInput))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            InputWrapper input = new InputWrapper();
            //read the header.
            input.owner = Guid.Parse(reader.ReadString());
            input.timeStamp = DateTime.FromBinary(reader.ReadInt64());
            // Read keys from single byte
            byte keyByte = reader.ReadByte();
            for (int i = 0; i < input.keys.Length; i++)
            {
                input.keys[i] = (keyByte & (1 << i)) != 0;
            }

            // Read mouse position
            input.MouseX = reader.ReadDouble();
            input.MouseY = reader.ReadDouble();

            // Read mouse button state
            byte mouseByte = reader.ReadByte();
            input.LeftDown = (mouseByte & 1) != 0;
            
            return input;
        }
    }
}