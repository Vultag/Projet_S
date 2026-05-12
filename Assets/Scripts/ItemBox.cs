using Unity.Netcode;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    private ItemBoxSpawner itemBoxSpawner;

    private void Start()
    {
        itemBoxSpawner = transform.parent.GetComponent<ItemBoxSpawner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject.transform.parent.GetComponent<Player>();
        ///if (player.IsOwner)
        {
            player.GainPowerup((PowerUps)UnityEngine.Random.Range(1, 2));
        }
        itemBoxSpawner.spawned = 0;
        this.gameObject.SetActive(false);
    }

}
