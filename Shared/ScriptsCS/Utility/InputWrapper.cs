namespace Shared;
using System;
using System.Text;

public class InputWrapper
{
    public Guid owner;
    public DateTime timeStamp;
    // WASD keys
    public bool[] keys = { false, false, false, false, false, false };

    // store mouse state ourselves (Event args from Blazorex are read-only / double)
    public double MouseX { get; set; } = -1.0; //impossible. know if this is wrong.
    public double MouseY { get; set; } = -1.0;

    public double MouseXWorld {get; set;} = -1.0;
    public double MouseYWorld {get; set;} = -1.0;


    // click state we control (LeftPressed is a single-frame edge)
    public bool LeftDown { get; set; } = false;
    public bool LeftPressed { get; set; } = false;


    public InputWrapper() { }
    public override string ToString()
    {
        var sb = new StringBuilder();
        
        // Header with the Owner ID and Timestamp
        sb.AppendLine($"--- Input Frame [{timeStamp:HH:mm:ss.fff}] ---");
        sb.AppendLine($"Owner: {owner.ToString().Substring(0, 8)}..."); // Shortened Guid for readability

        // Keys Visualization
        // Mapping keys[0-3] to WASD for easy debugging
        char w = keys[0] ? 'W' : '_';
        char a = keys[1] ? 'A' : '_';
        char s = keys[2] ? 'S' : '_';
        char d = keys[3] ? 'D' : '_';
        // Mapping keys[4-5] to Space/Shift or others
        string extra = $"{(keys[4] ? "[K4]" : "")} {(keys[5] ? "[K5]" : "")}";

        sb.AppendLine($"Movement: [{w}{a}{s}{d}] {extra}");

        // Mouse & Clicks
        string clickState = LeftPressed ? "PRESSED (Edge)" : (LeftDown ? "HELD" : "idle");
        sb.AppendLine($"Mouse: ({MouseX:F1}, {MouseY:F1}) | Left: {clickState}");

        return sb.ToString();
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        {
            using (var writer = new BinaryWriter(ms))
            {
                // Write Guid (16 bytes)
                writer.Write(owner.ToByteArray());

                // Write DateTime (8 bytes - ticks)
                writer.Write(timeStamp.Ticks);

                // Pack 6 booleans into 1 byte (bitmask)
                byte keyMask = 0;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i]) keyMask |= (byte)(1 << i);
                }
                writer.Write(keyMask);

                // Mouse Data (Double = 8 bytes each)
                writer.Write(MouseX);
                writer.Write(MouseY);
                writer.Write(MouseXWorld);
                writer.Write(MouseYWorld);

                // Click states (Could be packed too, but keeping separate for clarity)
                writer.Write(LeftDown);
                writer.Write(LeftPressed);

                return ms.ToArray();
            }
        }
    }

    public static InputWrapper FromBytes(byte[] data)
    {
        using (var ms = new MemoryStream(data))
        {
            using (var reader = new BinaryReader(ms))
            {
                var wrapper = new InputWrapper();

                // Read Guid
                wrapper.owner = new Guid(reader.ReadBytes(16));

                // Read DateTime
                wrapper.timeStamp = new DateTime(reader.ReadInt64());

                // Unpack keys from bitmask
                byte keyMask = reader.ReadByte();
                for (int i = 0; i < wrapper.keys.Length; i++)
                {
                    wrapper.keys[i] = (keyMask & (1 << i)) != 0;
                }

                // Read Mouse Data
                wrapper.MouseX = reader.ReadDouble();
                wrapper.MouseY = reader.ReadDouble();
                wrapper.MouseXWorld = reader.ReadDouble();
                wrapper.MouseYWorld = reader.ReadDouble();
                // Read Click states
                wrapper.LeftDown = reader.ReadBoolean();
                wrapper.LeftPressed = reader.ReadBoolean();

                return wrapper;
            }
        }
    }

    
}