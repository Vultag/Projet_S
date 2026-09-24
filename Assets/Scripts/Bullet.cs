using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour, IRapierTriggerListener
{
    [HideInInspector]
    public ulong bodyHandle;
    [HideInInspector]
    public ulong colliderHandle;
    [HideInInspector]
    public BulletPool pool;

    private void Start()
    {
        bodyHandle = this.GetComponent<RapierBody>().entityHandle;
        colliderHandle = this.GetComponent<RapierCircleShape>().colliderHandle;
    }

    public void Shoot(float posX, float posY, Vector2 direction, float speed, CollisionLayer team)
    {
        RapierWorld.body_set_enabled(RapierWorld.world, bodyHandle, true);
        RapierWorld.collider_set_memberships(
                RapierWorld.world,
                colliderHandle,
                (uint)(CollisionLayer.Projectiles | team)
                );
        RapierWorld.body_set_state(RapierWorld.world,bodyHandle,
            new BodyStateFFI
            {
                x = posX, 
                y = posY,
                velocityX = direction.x * speed,
                velocityY = direction.y * speed,
                angularVelocity = 0
            }
            );
    }

    public void OnRapierTriggerEnter(GameObject EntityEntering, ulong EntityEnteringID)
    {

        if (GameSyncManager.damageableDatabase.TryGet(EntityEnteringID, out IDamageable damageable))
        {
            damageable.TakeDamage(15f);
        }
        else
        {
            //Debug.Log("no damageable on " + EntityEntering.gameObject.name);
        }

        RapierWorld.body_set_enabled(RapierWorld.world, bodyHandle, false);
        transform.position = new Vector3(1000, 1000, 0);
        pool.Return(this);
    }

    public void OnRapierTriggerExit(GameObject EntityExiting, ulong EntityExitingID)
    {

    }
}
