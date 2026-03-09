
namespace ClientSideWASM;


public class NetworkObject
{
    //Servers as a header. No need to tag this network, its grabbed automatically.
    public Guid uid {get; set;}

    public bool eventOnly = false;

    public NetworkObject()
    {
        this.uid = Guid.NewGuid();
    }

    public virtual void WriteMetaData(BinaryWriter writer)
    {
        writer.Write(this.GetType().Name);
        writer.Write(this.uid.ToString());
        writer.Write(this.eventOnly);
    }
    public static object[] ReadMetaData(BinaryReader reader)//, out int length) //returns meta data objects. needs to be casted.
    {
        return new object[] {reader.ReadString(), reader.ReadString(), reader.ReadBoolean()};
    }
    public virtual void Encode(BinaryWriter writer)
    {
        var properties = NetworkMemberCache.GetSyncProperties(this.GetType());
        
        
        foreach (var prop in properties)
        {
            NetworkMemberCache.WriteProperty(writer, prop.GetValue(this), prop.PropertyType);
        }


        
    }

    public virtual void Decode(BinaryReader reader)
    {
        var properties = NetworkMemberCache.GetSyncProperties(this.GetType());
        
        foreach (var prop in properties)
        {
            object value = NetworkMemberCache.ReadProperty(reader, prop.PropertyType);
            prop.SetValue(this, value);
        }
    }

}