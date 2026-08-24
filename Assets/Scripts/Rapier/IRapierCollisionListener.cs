using UnityEngine;

public interface IRapierCollisionListener
{
    void OnRapierCollisionEnter( ulong HandleColiderEntering);
    void OnRapierCollisionExit(ulong HandleColiderExiting);
}