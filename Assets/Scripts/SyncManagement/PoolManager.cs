using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    List<Pool> pools = new List<Pool>(4);

    public Pool GetOrCreatePool(Type poolType,GameObject itemPrefab, ushort itemPoolQuantity)
    {
        foreach (Pool pool in pools) 
        {
            if(pool.poolType == poolType)
            {
                return pool;
            }
        }
        /// pool type doesnt exist -> create it
        var poolItems = new Queue<IPoolable>(itemPoolQuantity);
        for (int i = 0; i < itemPoolQuantity; i++)
        {
            var item = Instantiate(itemPrefab, this.transform).GetComponent<IPoolable>();
            poolItems.Enqueue(item);
        }
        var newPool = new Pool(poolItems);
        newPool.poolType = poolType;
        pools.Add(newPool);
        return newPool;
    }


    public void PoolsTick()
    {
        foreach (var pool in pools)
        {
            pool.PoolTick();
        }
    }
    public void SavePoolsState()
    {
        foreach (var pool in pools)
        {
            pool.SaveState();
        }
    }
    public void RestorePoolsState()
    {
        foreach (var pool in pools)
        {
            pool.RestoreState();
        }
    }

}
