using UnityEngine;

public interface IRapierCollisionListener
{
    void OnRapierCollisionEnter( ulong HandleBodyEntering, Vector2 normal);
    void OnRapierCollisionExit(ulong HandleBodyExiting, Vector2 normal);
}