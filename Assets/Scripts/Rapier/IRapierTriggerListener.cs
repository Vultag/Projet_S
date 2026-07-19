using UnityEngine;

public interface IRapierTriggerListener
{

    void OnRapierTriggerEnter(ulong HandleEntered, ulong HandleEntering);
    void OnRapierTriggerExit(ulong HandleExited, ulong HandleExiting);
}
