using UnityEngine;

public class AmmoCreateSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject activeAmmoCreatePrefab;

    [HideInInspector]
    public byte spawned;
    private readonly float respawnCooldown = 10;
    private float respawnTime;

    private void Start()
    {
        spawned = 1;
        respawnTime = respawnCooldown;
    }

    private void Update()
    {

        respawnTime = respawnTime - (Time.deltaTime * Mathf.Abs(spawned - 1));
        if (respawnTime <= 0)
        {
            activeAmmoCreatePrefab.SetActive(true);
            spawned = 1;
            respawnTime = respawnCooldown;
        }
    }
}
