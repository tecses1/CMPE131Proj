namespace Shared;
public class Packet
{
    public Guid CorrelationId { get; set; }
    public bool RequiresResponse { get; set; }
    public bool IsResponse { get; set; }
    public string Purpose { get; set; }
    
    // For simple commands: ["Kick", "Reason"]
    public string[] Args { get; set; } 
    
    // For heavy game data: [Byte Array from Encode()]
    public byte[] Data { get; set; } 

    public byte[] Serialize()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            // 1. Standard Header
            writer.Write(CorrelationId.ToByteArray());
            writer.Write(RequiresResponse);
            writer.Write(IsResponse);
            writer.Write(Purpose ?? "");

            // 2. Serialize string[] Args
            if (Args != null)
            {
                writer.Write(Args.Length);
                foreach (string arg in Args)
                {
                    writer.Write(arg ?? "");
                }
            }
            else
            {
                writer.Write(0); // Length 0
            }

            // 3. Serialize byte[] Data
            if (Data != null)
            {
                writer.Write(Data.Length);
                writer.Write(Data);
            }
            else
            {
                writer.Write(0);
            }

            return ms.ToArray();
        }
    }

    public static Packet Deserialize(byte[] bytes)
    {
        using (MemoryStream ms = new MemoryStream(bytes))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            Packet p = new Packet();
            p.CorrelationId = new Guid(reader.ReadBytes(16));
            p.RequiresResponse = reader.ReadBoolean();
            p.IsResponse = reader.ReadBoolean();
            p.Purpose = reader.ReadString();

            // Read Args
            int argCount = reader.ReadInt32();
            p.Args = new string[argCount];
            for (int i = 0; i < argCount; i++)
            {
                p.Args[i] = reader.ReadString();
            }

            // Read Data
            int dataLength = reader.ReadInt32();
            if (dataLength > 0)
            {
                p.Data = reader.ReadBytes(dataLength);
            }

            return p;
        }
    }
}