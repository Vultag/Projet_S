using UnityEngine;

public class AmmoCreate : MonoBehaviour, IRapierTriggerListener
{
    [SerializeField]
    private AmmoCreateSpawner ammoCreateSpawner;


    private void Start()
    {

    }


    public void OnRapierTriggerEnter(GameObject EntityEntering, ulong EntityEnteringID)
    {

        if (GameSyncManager.damageableDatabase.TryGet(EntityEnteringID, out IDamageable damageable))
        {
            var player = (PlayerNet)damageable;
            player.AmmoPickUp();

        }
        else
        {
            //Debug.Log("no damageable on " + EntityEntering.gameObject.name);
        }
        ammoCreateSpawner.spawned = 0;
        this.gameObject.SetActive(false);
    }

    public void OnRapierTriggerExit(GameObject EntityExiting, ulong EntityExitingID)
    {
    }
}
