namespace ClientSideWASM;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class Network : Attribute 
{

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
            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => Attribute.IsDefined(p, typeof(Network)))
                    .ToList();
        });
    }
}