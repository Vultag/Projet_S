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

    public void Tick()
    {

        map.playerSpawner.Tick();


    }


}
