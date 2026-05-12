using UnityEngine;

public class ItemBoxSpawner : MonoBehaviour
{

    [SerializeField]
    private GameObject activeItemBoxPrefab;

    [HideInInspector]
    public byte spawned;
    private readonly float respawnCooldown = 5;
    private float respawnTime;

    private void Start()
    {
        spawned = 1;
        respawnTime = respawnCooldown;
    }

    private void Update()
    {

        respawnTime = respawnTime - (Time.deltaTime * Mathf.Abs(spawned-1));
        if (respawnTime <= 0)
        {
            activeItemBoxPrefab.SetActive(true);
            spawned = 1;
            respawnTime = respawnCooldown;
        }
    }

}
