using UnityEngine;

public class HarmfullSurface : MonoBehaviour,IRapierCollisionListener
{
    [SerializeField]
    float damage;

    public void OnRapierCollisionEnter(ulong HandleBodyEntering, Vector2 normal)
    {
        if (GameSyncManager.damageableDatabase.TryGet(HandleBodyEntering, out IDamageable damageable))
        {
            RapierWorld.body_add_force(
                  RapierWorld.world,
                  ((PlayerNet)damageable).PlayerBody.entityHandle,
                  normal.x * 1000 * 1.1f,
                  normal.y * 1000 * 1.1f,
                  0
              );

            damageable.TakeDamage(damage);
        }
    }

    public void OnRapierCollisionExit(ulong HandleBodyExiting, Vector2 normal)
    {

    }

}
