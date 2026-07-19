using UnityEngine;

public class testrapiertrigger : MonoBehaviour, IRapierTriggerListener
{
    public void OnRapierTriggerEnter(ulong HandleEntered, ulong HandleEntering)
    {
        if (RapierWorld.unityToRapierEntityMap.TryGetValue(HandleEntering, out var bodyData))
        {
            Destroy(bodyData.GameObject);
            RapierWorld.DestroyBody(HandleEntering);
        }
    }

    public void OnRapierTriggerExit(ulong HandleExited, ulong HandleExiting)
    {


    }
}