namespace Shared;
using System.Text.Json;
[System.Serializable]
public class Packet
{
    public string customMessage;
    public StartUpInfo startUpInfo;
    public static Packet fromJSON(string s)
    {
        // Deserialize packet from string
        try
        {
            Packet p = JsonSerializer.Deserialize<Packet>(s);
            return p;
        }catch (JsonException)
        {
            Console.WriteLine("[WARNING] Did not recognize packet content: " + s);
            return null;
        }
        

    }

    public string toJSON()
    {
        // Serialize packet to string
        return JsonSerializer.Serialize(this);
    }
    


}