using UnityEngine;

public class EnergyOrbSpawner : MonoBehaviour
{

    [SerializeField]
    private GameObject activeEnergyOrbPrefab;

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

        respawnTime = respawnTime - (Time.deltaTime * Mathf.Abs(spawned-1));
        if (respawnTime <= 0)
        {
            activeEnergyOrbPrefab.SetActive(true);
            spawned = 1;
            respawnTime = respawnCooldown;
        }
    }

}
