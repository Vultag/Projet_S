using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Map map;

    private void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
        Time.fixedDeltaTime = 1f / 60f;
        GameSyncManager.gameManager = this;
    }

    public void Tick(short relativeTick)
    {
        foreach (ISyncronizable syncronizable in map.Syncronized)
        {
            syncronizable.Tick(relativeTick);
        }

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
