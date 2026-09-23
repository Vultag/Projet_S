using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDamageableDatabase : Database<IDamageable>
{
    public IDamageableDatabase(int capacity) : base(capacity){ }

    public override void ROLLBACKsave()
    {
        ROLLBACKdictionary.Clear();

        foreach (var entity in dictionary)
        {
            if (!ROLLBACKdictionary.TryAdd(entity.Key, entity.Value)) Debug.Log("coundt add " + entity.Key + " in IDamageable rollback save");
            entity.Value.ROLLBACKhealth = entity.Value.health;
            
        }

        ROLLBACKdictionaryVersion = dictionaryVersion;
    }
    public override void ROLLBACKrestore()
    {
        //Debug.Log("restore to " + dictionary.Count);
        foreach (var entity in dictionary)
        {
            entity.Value.health = entity.Value.ROLLBACKhealth;
        }
        base.ROLLBACKrestore();
    }

}
