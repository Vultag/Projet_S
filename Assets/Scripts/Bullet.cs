using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour, IRapierTriggerListener,IPoolable
{
    [HideInInspector]
    public ulong bodyHandle;
    [HideInInspector]
    public ulong colliderHandle;

    private bool active;
    private bool activeROLLBACK;

    private bool hit;
    private bool hitROLLBACK;

    [HideInInspector]
    public Pool pool { get; set; }

    private void Start()
    {
        bodyHandle = this.GetComponent<RapierBody>().entityHandle;
        colliderHandle = this.GetComponent<RapierCircleShape>().colliderHandle;
        gameObject.SetActive(false);
    }

    public void Shoot(float posX, float posY, Vector2 direction, float speed, CollisionLayer team)
    {
        //Debug.Log("enable");
        active = true;
        hit = false;
        RapierWorld.body_set_enabled(RapierWorld.world, bodyHandle, true);
        RapierWorld.shape_set_enabled(RapierWorld.world, colliderHandle, true);
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
        if (hit) return;

        if (GameSyncManager.damageableDatabase.TryGet(EntityEnteringID, out IDamageable damageable))
        {
            damageable.TakeDamage(15f);
        }
        else
        {
            //Debug.Log("no damageable on " + EntityEntering.gameObject.name);
        }
        //Debug.Log("         disable");
        RapierWorld.body_set_enabled(RapierWorld.world, bodyHandle, false);
        RapierWorld.shape_set_enabled(RapierWorld.world, colliderHandle, false);
        active = false;
        hit = true;
        pool.Return(this);
    }

    public void OnRapierTriggerExit(GameObject EntityExiting, ulong EntityExitingID)
    {

    }


    public void PoolableStateSave()
    {
        activeROLLBACK = active;
        hitROLLBACK = hit;
    }

    public void PoolableStateRestore()
    {
        //Debug.Log(activeROLLBACK + gameObject.GetInstanceID().ToString());
        gameObject.SetActive(activeROLLBACK);
        active = activeROLLBACK;
        hit = hitROLLBACK;
    }

    public void PoolableTick()
    {

    }
}
