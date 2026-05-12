using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
        Time.fixedDeltaTime = 1f / 60f;
    }
}
