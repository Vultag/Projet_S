using Unity.Netcode;
using UnityEngine;

public class EnergyOrb : MonoBehaviour, IRapierTriggerListener
{
    [SerializeField]
    private EnergyOrbSpawner energyOrbSpawner;


    private void Start()
    {

    }


    public void OnRapierTriggerEnter(GameObject EntityEntering, ulong EntityEnteringID)
    {

        if (GameSyncManager.damageableDatabase.TryGet(EntityEnteringID, out IDamageable damageable))
        {
            var player = (PlayerNet)damageable;
            player.mechanicalState.energy+= 30;
        }
        else
        {
            //Debug.Log("no damageable on " + EntityEntering.gameObject.name);
        }
        energyOrbSpawner.spawned = 0;
        this.gameObject.SetActive(false);
    }

    public void OnRapierTriggerExit(GameObject EntityExiting, ulong EntityExitingID)
    {
    }
}
