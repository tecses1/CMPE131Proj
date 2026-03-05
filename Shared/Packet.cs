namespace Shared;
using System.Text.Json;
[System.Serializable]
public class Packet
{
    public Guid CorrelationId { get; set; } // Matches requests to responses
    public bool RequiresResponse {get; set;}
    public bool IsResponse { get; set; }    // True if this is answering a command
    public string Purpose { get; set; }
    public string[] Args { get; set; }



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