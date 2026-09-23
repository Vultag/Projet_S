using Unity.Mathematics;
using UnityEngine;

public class AutoTurret : MonoBehaviour,PowerUpInterface
{
    [HideInInspector]
    public ulong playerBodyHandle;
    [HideInInspector]
    public CollisionLayer team;
    //[HideInInspector]
    //public BulletPool bulletPool;

    float latestEnemyAngle;
    const float shootCooldownTime = 0.5f;
    float shootCooldown = shootCooldownTime;
    float rollbackShootCooldown = shootCooldownTime;


    public void PowerUpAction1(Vector2 delta)
    {
        shootCooldown -= PlayerNet.gameFixedDeltaTime;

        var closestPlayerhandle = RapierWorld.world_get_closest_body(RapierWorld.world,
                  playerBodyHandle,
                  0,
                  0,
                  15,
                  (uint)((CollisionLayer.TeamA | CollisionLayer.TeamB | CollisionLayer.TeamC | CollisionLayer.TeamD) & ~team)
                  );

        

        if (closestPlayerhandle != 0)
        {
            GameSyncManager.rapierToUnityDatabase.TryGet(closestPlayerhandle, out var entity);
            //Debug.Log(entity.GameObject.name);

            var closestPlayerState = RapierWorld.body_get_state(RapierWorld.world, closestPlayerhandle);
            var playerBodyState = RapierWorld.body_get_state(RapierWorld.world, playerBodyHandle);
            Vector2 direction = new Vector2(closestPlayerState.x - playerBodyState.x, closestPlayerState.y - playerBodyState.y);
            latestEnemyAngle = -Mathf.Atan2(direction.x, direction.y);

            if (shootCooldown <= 0)
            {

                //var entityB = RapierToUnityDatabase.Get(closestPlayerhandle);
                //RapierWorld.unityToRapierEntityMap.TryGetValue(closestPlayerhandle, out var entityB);
                Bullet bullet = GameSyncManager.bulletPool.Pull();
                if (bullet == null) return;
                bullet.Shoot(playerBodyState.x, playerBodyState.y, direction.normalized, 10f,team);
                shootCooldown = shootCooldownTime;
            }

        }
    }

    public void PowerUpAction2(Vector2 delta)
    {
        throw new System.NotImplementedException();
    }

    public void PowerUpAim(Vector2 dir)
    {
        throw new System.NotImplementedException();
    }


    public void PowerUpDisable()
    {
        throw new System.NotImplementedException();
    }

    public void PowerUpEnable()
    {
        throw new System.NotImplementedException();
    }

    public void PowerUpSelection(bool SelectOrDeselect)
    {
        this.gameObject.SetActive(SelectOrDeselect);
    }

    public void RestorePowerUpState(bool OnOrOff)
    {
        this.gameObject.SetActive(OnOrOff);
        shootCooldown = rollbackShootCooldown; 
    }

    public void SavePowerUpState()
    {
        rollbackShootCooldown = shootCooldown;
    }


    private void FixedUpdate()
    {
        this.transform.rotation = quaternion.RotateZ(latestEnemyAngle);

        //RapierWorld.unityToRapierEntityMap.TryGetValue(test, out var testAAA);

    }
}
