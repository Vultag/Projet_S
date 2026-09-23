using System.Collections.Generic;
using UnityEngine;

public class Database<T>
{
    protected Dictionary<ulong, T> dictionary;
    protected Dictionary<ulong, T> ROLLBACKdictionary;
    protected int dictionaryVersion = 0;
    protected int ROLLBACKdictionaryVersion = 0;

    public Database(int capacity)
    {
        dictionary = new Dictionary<ulong, T>(capacity);
        ROLLBACKdictionary = new Dictionary<ulong, T>(capacity);
    }

    public virtual bool TryGet(ulong handle, out T value)
    {
        bool found = dictionary.TryGetValue(handle, out var data);
        //if (!found) Debug.Log("coundt get " + handle);
        value = data;
        return found;
    }

    public virtual void Add(ulong handle, T data)
    {
        if (!dictionary.TryAdd(handle, data)) Debug.Log("coundt add " + handle);
        dictionaryVersion++;
    }

    ////public static bool Remove(int key)
    ////{
    ////    return data.Remove(key);
    ////dictionaryVersion++;
    ////}

    public virtual bool IsDatabaseSynced()
    {
        //Debug.Log(dictionaryVersion);
        return dictionaryVersion == ROLLBACKdictionaryVersion;
    }

    public virtual void ROLLBACKsave()
    {
        ROLLBACKdictionary.Clear();
        foreach (var entity in dictionary)
        {
            if (!ROLLBACKdictionary.TryAdd(entity.Key, entity.Value)) Debug.Log("coundt add " + entity.Key + " in rollback save");
        }
        ROLLBACKdictionaryVersion = dictionaryVersion;
    }
    public virtual void ROLLBACKrestore()
    {
        //Debug.Log("restore to " + dictionary.Count);

        ///swap dictionaries
        var temp = dictionary;
        dictionary = ROLLBACKdictionary;
        ROLLBACKdictionary=temp;
        /*
        dictionary.Clear();
        foreach (var entity in ROLLBACKdictionary)
        {
            if (!dictionary.TryAdd(entity.Key, entity.Value)) Debug.Log("coundt add " + entity.Key + " in rollback restore");
        }
        */
        dictionaryVersion = ROLLBACKdictionaryVersion;
    }
}
