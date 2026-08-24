using Unity.Netcode;
using UnityEngine;

public class ItemBox : MonoBehaviour, IRapierTriggerListener
{
    private ItemBoxSpawner itemBoxSpawner;


    private void Start()
    {
        itemBoxSpawner = transform.parent.GetComponent<ItemBoxSpawner>();
    }


    public void OnRapierTriggerEnter(GameObject EntityEntering, ulong EntityEnteringID)
    {
        Debug.Log(EntityEntering.gameObject.name);
        var player = EntityEntering.transform.parent.GetComponent<PlayerNet>();
        player.mechanicalState.energy += 10;
        itemBoxSpawner.spawned = 0;
        this.gameObject.SetActive(false);
    }

    public void OnRapierTriggerExit(GameObject EntityExiting, ulong EntityExitingID)
    {
    }
}
