using System.Collections.Generic;
using UnityEngine;

public class RapierToUnityDatabase : Database<EntityData>
{
    //private readonly Dictionary<ulong, EntityData> dictionary = new(512);
    //private readonly Dictionary<ulong, EntityData> ROLLBACKdictionary = new(512);
    //static private int dictionaryVersion = 0;
    //static private int ROLLBACKdictionaryVersion = 0;

    public RapierToUnityDatabase(int capacity): base(capacity)
    {}

    //public override EntityData Get(ulong handle)
    //{
    //    if(!dictionary.TryGetValue(handle, out var data)) Debug.Log("coundt add " + handle); 
    //    return data;
    //}

    //public override void Add(ulong handle, EntityData data)
    //{
    //    if (!dictionary.TryAdd(handle, data)) Debug.Log("coundt add " + handle);
    //    dictionaryVersion++;
    //}

    //////public static bool Remove(int key)
    //////{
    //////    return data.Remove(key);
    //////dictionaryVersion++;
    //////}

    //public override bool IsDatabaseSynced()
    //{
    //    //Debug.Log(dictionaryVersion);
    //    return dictionaryVersion == ROLLBACKdictionaryVersion; 
    //}

    //public override void ROLLBACKsave()
    //{
    //    //Debug.Log("save with " + dictionary.Count);
    //    ROLLBACKdictionary.Clear();
    //    foreach (var entity in dictionary)
    //    {
    //         if (!ROLLBACKdictionary.TryAdd(entity.Key, entity.Value)) Debug.Log("coundt add " + entity.Key + " in rollback save");
    //    }
    //    ROLLBACKdictionaryVersion = dictionaryVersion;
    //}
    //public override void ROLLBACKrestore()
    //{
    //    //Debug.Log("restore to " + dictionary.Count);
    //    dictionary.Clear();
    //    foreach (var entity in ROLLBACKdictionary)
    //    {
    //        if (!dictionary.TryAdd(entity.Key, entity.Value)) Debug.Log("coundt add " + entity.Key + " in rollback restore");
    //    }
    //    dictionaryVersion = ROLLBACKdictionaryVersion;
    //}
}