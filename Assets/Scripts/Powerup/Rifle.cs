using Unity.Mathematics;
using UnityEngine;

public class Rifle : MonoBehaviour, PowerUpInterface
{

    [HideInInspector]
    public ulong playerBodyHandle;
    [HideInInspector]
    public CollisionLayer team;
    const float shootCooldownTime = 0.9f;
    float shootCooldown = shootCooldownTime;
    float rollbackShootCooldown = shootCooldownTime;

    Vector2 shootDir=Vector2.up;

    public void PowerUpAction1(Vector2 delta)
    {
        if (shootCooldown <= 0)
        {
            var playerBodyState = RapierWorld.body_get_state(RapierWorld.world, playerBodyHandle);
            Bullet bullet = GameSyncManager.bulletPool.Pull();
            if (bullet == null) return;
            bullet.Shoot(playerBodyState.x, playerBodyState.y, delta.normalized, 32f, team);
            shootCooldown = shootCooldownTime;
        }
    }

    public void PowerUpAction2(Vector2 delta)
    {
       
    }

    public void PowerUpAim(Vector2 dir)
    {
        shootDir = dir;
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
        shootCooldown = rollbackShootCooldown;
    }

    public void SavePowerUpState()
    {
        rollbackShootCooldown = shootCooldown;
    }

    private void FixedUpdate()
    {
        float side = Mathf.Sign(shootDir.x);
        float angle = Mathf.Atan2(shootDir.y * side, shootDir.x* side);
        this.transform.localScale = new Vector3(this.transform.localScale.x, Mathf.Abs(this.transform.localScale.y) * side, this.transform.localScale.z);
        this.transform.rotation = quaternion.RotateZ(angle - Mathf.PI*0.5f);

    }

    public void Tick()
    {
        shootCooldown -= PlayerNet.gameFixedDeltaTime;
    }
}
