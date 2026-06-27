using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// private -> procedural register
    [SerializeField]
    private List<Module> modules = new List<Module>();

    byte currentModuleIdx;
    byte previousModuleIdx;
    byte nextModuleIdx;


    private void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
        Time.fixedDeltaTime = 1f / 60f;
    }

    public void Tick(uint tick)
    {
        //modules[previousModuleIdx].Tick(tick);
        //modules[currentModuleIdx].Tick(tick);
        //modules[nextModuleIdx].Tick(tick);
    }

    public void ModuleChange(byte moduleIdx)
    {
        currentModuleIdx = moduleIdx;
        previousModuleIdx = moduleIdx == 0? (byte)(modules.Count-1): (byte)(moduleIdx-1);
        nextModuleIdx = moduleIdx == (modules.Count - 1) ? (byte)0 : (byte)(moduleIdx + 1);

        Debug.Log(previousModuleIdx);
        Debug.Log(currentModuleIdx);
        Debug.Log(nextModuleIdx);

    }


}
