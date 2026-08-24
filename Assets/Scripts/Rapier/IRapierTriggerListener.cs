using UnityEngine;

public interface IRapierTriggerListener
{

    void OnRapierTriggerEnter(GameObject EntityEntering,ulong EntityEnteringID);
    void OnRapierTriggerExit(GameObject EntityExiting, ulong EntityExitingID);
}
