using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GridBrushBase;


public interface IPoolable
{
    Pool pool { get; set; }

    public void PoolableTick();

    public void PoolableStateSave();

    public void PoolableStateRestore();

}

public class Pool
{
    GameObject itemPrefab;
    ushort itemPoolQuantity;
    public Type poolType;

    private List<IPoolable> pulledItems;
    private List<IPoolable> pulledItemsROLLBACK;

    private Queue<IPoolable> storedItems;
    private Queue<IPoolable> storedItemsROLLBACK;

    public Queue<IPoolable> pendingRemoval;

    public Pool(Queue<IPoolable> poolItems)
    {
        storedItems = poolItems;
        storedItemsROLLBACK = new Queue<IPoolable>(poolItems.Count);
        foreach (var item in poolItems)
        {
            storedItemsROLLBACK.Enqueue(item);
            item.pool = this;
        }

        pulledItems = new List<IPoolable>(8);
        pulledItemsROLLBACK = new List<IPoolable>(8);

        pendingRemoval = new Queue<IPoolable>(4);
    }

    public IPoolable Pull()
    {
        if (storedItems.Count == 0)
        {
            Debug.LogError("empty"); 
            return null;
        }
        var item = storedItems.Dequeue();
        //Debug.Log("que " + ((MonoBehaviour)item).gameObject.GetInstanceID().ToString());
        pulledItems.Add(item);
        ((MonoBehaviour)item).gameObject.SetActive(true);
        return item;
    }
    public void Return(IPoolable item)
    {
        //Debug.Log("deque " + ((MonoBehaviour)item).gameObject.GetInstanceID().ToString());
        storedItems.Enqueue(item);
        pulledItems.Remove(item);
        ((MonoBehaviour)item).gameObject.SetActive(false);
    }


    public void PoolTick()
    {
        foreach (var item in pulledItems)
        {
            item.PoolableTick();
        }
        for (int i = 0; i < pendingRemoval.Count; i++)
        {
            Return(pendingRemoval.Dequeue());
        }
    }
    public void SaveState()
    {
        foreach (var item in pulledItemsROLLBACK)
        {
            item.PoolableStateSave();
        }
        storedItemsROLLBACK.Clear();
        pulledItemsROLLBACK.Clear();
        foreach (var item in storedItems)
        {
            storedItemsROLLBACK.Enqueue(item);
        }
        foreach (var item in pulledItems)
        {
            item.PoolableStateSave();
            pulledItemsROLLBACK.Add(item);
        }
    }
    public void RestoreState()
    {
        foreach (var item in pulledItems)
        {
            item.PoolableStateRestore();
        }
        storedItems.Clear();
        pulledItems.Clear();
        foreach (var item in storedItemsROLLBACK)
        {
            storedItems.Enqueue(item);
        }
        foreach (var item in pulledItemsROLLBACK)
        {
            item.PoolableStateRestore();
            pulledItems.Add(item);
            //Debug.Log("add ");
        }
        //Debug.Log("restore " + storedItems.Count);
    }

}
