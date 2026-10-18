using Unity.Mathematics;
using UnityEngine;

public class Rifle : MonoBehaviour, PowerUpInterface
{

    [HideInInspector]
    public PlayerNet player;
    [HideInInspector]
    public int equipmentIdx;
    [HideInInspector]
    public CollisionLayer team;

    const float shootCooldownTime = 0.9f;
    float shootCooldown = shootCooldownTime;
    float rollbackShootCooldown = shootCooldownTime;

    Vector2 shootDir=Vector2.up;

    public void PowerUpAction1(Vector2 delta)
    {
        if (shootCooldown <= 0 & player.weaponsAmmo[equipmentIdx]>0)
        {
            var playerBodyState = RapierWorld.body_get_state(RapierWorld.world, player.PlayerBody.entityHandle);
            Bullet bullet = GameSyncManager.bulletPool.Pull();
            if (bullet == null) return;
            bullet.Shoot(playerBodyState.x, playerBodyState.y, delta.normalized, 32f, team);
            shootCooldown = shootCooldownTime;
            player.weaponsAmmo[equipmentIdx] -= 1;
            player.syncedUIstatesBitmask |= 1u << (equipmentIdx + 16);

            //foreach (var item in GameSyncManager.Players)
            //{
            //    item.Die();
            //}
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

    public void Tick(bool OnOrOff)
    {
        shootCooldown -= PlayerNet.gameFixedDeltaTime;
    }
}
