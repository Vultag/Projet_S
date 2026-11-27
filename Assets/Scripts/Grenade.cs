using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Grenade : MonoBehaviour,IPoolable
{
    [HideInInspector]
    public ulong bodyHandle;
    [HideInInspector]
    public ulong colliderHandle;

    [HideInInspector]
    public Pool pool { get; set; }

    /// Implement falloff
    const float explosionRadius = 10f;
    const float explosionDamage = 25f;

    short ticksTillExplode = 180;
    short ticksTillExplodeROLLBACK;
    CollisionLayer selfTeam;
    CollisionLayer selfTeamROLLBACK;

    bool active;
    bool activeROLLBACK;

    private void Start()
    {
        bodyHandle = this.GetComponent<RapierBody>().entityHandle;
        colliderHandle = this.GetComponent<RapierCircleShape>().colliderHandle;
        ticksTillExplodeROLLBACK = ticksTillExplode;
        gameObject.SetActive(false);
    }

    public void Shoot(float posX, float posY, Vector2 direction, float speed, CollisionLayer team)
    {
        active = true;
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
        selfTeam = team;
    }

    public void PoolableTick()
    {

        if (--ticksTillExplode <=0)
        {

            var grenadeState = RapierWorld.body_get_state(RapierWorld.world, bodyHandle);

            var hitCount = RapierWorld.get_overlap_bodies(RapierWorld.world,
               grenadeState.x,
               grenadeState.y,
               explosionRadius,
               (uint)CollisionLayer.Player,
               (uint)((CollisionLayer.TeamA | CollisionLayer.TeamB | CollisionLayer.TeamC | CollisionLayer.TeamD) & ~selfTeam),
               RapierWorld.handleQueryCollector,
                (UIntPtr)RapierWorld.handleQueryCollector.Length
               );

            //if (GameSyncManager.syncingPhase) Debug.Log("expl");

            for (int i = 0; i < hitCount; i++)
            {

                //if(GameSyncManager.syncingPhase )Debug.Log("hit");

                var hitHandle = RapierWorld.handleQueryCollector[i];
                if (GameSyncManager.damageableDatabase.TryGet(hitHandle, out IDamageable damageable))
                {
                    var hitState = RapierWorld.body_get_state(RapierWorld.world, hitHandle);
                    var pushDir = new Vector2(hitState.x- grenadeState.x, hitState.y - grenadeState.y).normalized;

                    RapierWorld.body_add_force(
                          RapierWorld.world,
                          ((PlayerNet)damageable).PlayerBody.entityHandle,
                          pushDir.x * 1000 * 1.5f,
                          pushDir.y * 1000 * 1.5f,
                          0
                      );

                    damageable.TakeDamage(explosionDamage);
                }
                
            }

            RapierWorld.body_set_state(RapierWorld.world, bodyHandle,
                new BodyStateFFI
                {
                    x = 1000,
                    y = 1000,
                    velocityX = 0,
                    velocityY = 0,
                    angularVelocity = 0
                });
            transform.position = new Vector3(1000, 1000, 0);
            RapierWorld.body_set_enabled(RapierWorld.world, bodyHandle, false);

            active = false;
            pool.pendingRemoval.Enqueue(this);

            ticksTillExplode = 180;
        }
    }

    public void PoolableStateSave()
    {
        activeROLLBACK = active;
        ticksTillExplodeROLLBACK = ticksTillExplode;
        //Debug.Log(ticksTillExplode);
        selfTeamROLLBACK = selfTeam;
    }

    public void PoolableStateRestore()
    {
        gameObject.SetActive(activeROLLBACK);
        active = activeROLLBACK;
        ticksTillExplode = ticksTillExplodeROLLBACK;
        //Debug.LogError(ticksTillExplodeROLLBACK);
        selfTeam = selfTeamROLLBACK;
    }
}
