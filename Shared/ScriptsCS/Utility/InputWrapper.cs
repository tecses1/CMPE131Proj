namespace Shared;
using System;
using System.Text;

public class InputWrapper
{
    public Guid owner;
    public DateTime timeStamp;
    // WASD keys
    public Dictionary<string, bool> keys = new Dictionary<string, bool>();
    // store mouse state ourselves (Event args from Blazorex are read-only / double)
    public double MouseX { get; set; } = -1.0; //impossible. know if this is wrong.
    public double MouseY { get; set; } = -1.0;

    public double MouseXWorld {get; set;} = -1.0;
    public double MouseYWorld {get; set;} = -1.0;


    // click state we control (LeftPressed is a single-frame edge)
    public bool LeftDown { get; set; } = false;
    public bool LeftPressed { get; set; } = false;


    public InputWrapper() { }

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

                // --- NEW DICTIONARY SERIALIZATION ---
                // 1. Gather only the keys that are currently pressed down
                List<string> activeKeys = new List<string>();
                foreach (var kvp in keys)
                {
                    if (kvp.Value) 
                    {
                        activeKeys.Add(kvp.Key);
                    }
                }

                // 2. Write how many keys are currently pressed
                // A byte is perfect here (max 255 simultaneous keys is plenty)
                writer.Write((byte)activeKeys.Count);

                // 3. Write the actual string names of the pressed keys
                foreach (string key in activeKeys)
                {
                    writer.Write(key); // BinaryWriter handles string length-prefixing automatically
                }
                // ------------------------------------

                // Mouse Data (Double = 8 bytes each)
                writer.Write(MouseX);
                writer.Write(MouseY);
                writer.Write(MouseXWorld);
                writer.Write(MouseYWorld);

                // Click states
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
                // Ensure the dictionary is initialized
                wrapper.keys = new Dictionary<string, bool>();

                // Read Guid
                wrapper.owner = new Guid(reader.ReadBytes(16));

                // Read DateTime
                wrapper.timeStamp = new DateTime(reader.ReadInt64());

                // --- NEW DICTIONARY DESERIALIZATION ---
                // 1. Find out how many keys are pressed
                byte activeKeyCount = reader.ReadByte();

                // 2. Read each string and set it to true in the dictionary
                for (int i = 0; i < activeKeyCount; i++)
                {
                    string keyName = reader.ReadString();
                    wrapper.keys[keyName] = true;
                }
                // --------------------------------------

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
    public bool IsKeyDown(string key, bool ignoreCase = false)
    {
        if (!ignoreCase)
        {
            // Original behavior: Check for exact case match.
            // Returns true only if the key exists AND its value is true.
            return keys.TryGetValue(key, out bool isDown) && isDown;
        }

        // Ignore case behavior: Check both lowercase and uppercase variations.
        string lowerKey = key.ToLower();
        string upperKey = key.ToUpper();

        bool isLowerDown = keys.TryGetValue(lowerKey, out bool lDown) && lDown;
        bool isUpperDown = keys.TryGetValue(upperKey, out bool uDown) && uDown;

        // Return true if either the lowercase OR the uppercase version is currently pressed.
        return isLowerDown || isUpperDown;
    }
}