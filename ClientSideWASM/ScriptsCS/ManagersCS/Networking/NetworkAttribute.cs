namespace ClientSideWASM;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
using System.Numerics;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class Network : Attribute 
{
    public int Order { get; }
        public Network(int order) => Order = order;
}

public static class NetworkMemberCache
{
    
    // Caches the list of properties for each type to avoid repeated Reflection hits
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> _cache = 
        new ConcurrentDictionary<Type, List<PropertyInfo>>();

    public static List<PropertyInfo> GetSyncProperties(Type type)
    {
        return _cache.GetOrAdd(type, t => 
        {
            // This code only runs the FIRST time a type is requested
    return type.GetProperties()
                .Where(p => p.GetCustomAttribute<Network>() != null)
                .OrderBy(p => p.GetCustomAttribute<Network>().Order)
                .ToList();
            });
    }

    public static void WriteProperty(BinaryWriter writer, object value, Type type)
    {
        if (type == typeof(int)) writer.Write((int)value);
        else if (type == typeof(float)) writer.Write((float)value);
        else if (type == typeof(bool)) writer.Write((bool)value);
        else if (type == typeof(string)) writer.Write((string)value ?? "");
        // Support for Unity types
        else if (type == typeof(Vector2)) {
            Vector2 v = (Vector2)value;
            writer.Write(v.X); writer.Write(v.Y);
        }else if (type == typeof(Transform))
        {
            Transform t = (Transform)value;
            writer.Write(t.position.X);
            writer.Write(t.position.Y);
            writer.Write((int)t.size.X);
            writer.Write((int)t.size.Y);
            writer.Write(t.rotation);

        }
    }

    public static object ReadProperty(BinaryReader reader, Type type)
    {
        if (type == typeof(int)) return reader.ReadInt32();
        if (type == typeof(float)) return reader.ReadSingle();
        if (type == typeof(bool)) return reader.ReadBoolean();
        if (type == typeof(string)) return reader.ReadString();
        else if (type == typeof(Vector2)) return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        else if (type == typeof(Transform))
        {
            Transform t = new Transform(reader.ReadSingle(), reader.ReadSingle(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadSingle());
            return t;

        }
        return null;
    }
}