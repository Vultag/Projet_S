using TMPro;
using UnityEngine;

public class respawnTimer : MonoBehaviour
{
    private TextMeshProUGUI timer;

    private void Awake()
    {
        timer = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        timer.text = GameSyncManager.gameManager.map.playerSpawner.GetPlayerTimer().ToString();
    }
}
