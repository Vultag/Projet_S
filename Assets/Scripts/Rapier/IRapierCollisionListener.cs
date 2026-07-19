using UnityEngine;

public interface IRapierCollisionListener
{
    void OnRapierCollisionEnter(ulong HandleColiderEntered, ulong HandleColiderEntering);
    void OnRapierCollisionExit(ulong HandleColiderExited, ulong HandleColiderExiting);
}