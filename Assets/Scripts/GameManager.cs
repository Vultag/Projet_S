using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Map map;

    //public Queue<ISyncronizable> pendingRemoval;

    private void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
        Time.fixedDeltaTime = 1f / 60f;
        GameSyncManager.gameManager = this;
        var poolManagerInstance =  gameObject.AddComponent<PoolManager>();
        //pendingRemoval = new Queue<ISyncronizable>(4);
        GameSyncManager.poolManager = poolManagerInstance;   
    }

    public void Tick(short relativeTick)
    {
        GameSyncManager.poolManager.PoolsTick();
        foreach (ISyncronizable syncronizable in map.Syncronized)
        {
            syncronizable.Tick(relativeTick);
        }
        //for (int i = 0; i < pendingRemoval.Count; i++)
        //{
        //    map.Syncronized.Remove(pendingRemoval.Dequeue());
        //}

    }
    public void ROLLBACKsave()
    {
        foreach (ISyncronizable syncronizable in map.Syncronized)
        {
            syncronizable.ROLLBACKsave();
        }
    }
    public void ROLLBACKrestore()
    {
        foreach (ISyncronizable syncronizable in map.Syncronized)
        {
            syncronizable.ROLLBACKrestore();
        }
    }

}
