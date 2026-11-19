using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Grapling : MonoBehaviour, PowerUpInterface
{
    public Hook hook;
    [SerializeField]
    private GameObject HookSprite;

    [HideInInspector]
    public PlayerNet player;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    //public void Shoot(Vector2 dir)
    //{
    //    hook.gameObject.SetActive(true);
    //    hook.Shoot(dir);
    //}

    //public void Detatch()
    //{
    //    if (hook.hookShoot)
    //    {
    //        hook.Disable();
    //    }
    //}


    public void RestorePowerUpState(bool OnOrOff)
    {
        hook.RestoreState(OnOrOff);
    }

    public void SavePowerUpState()
    {
        hook.SaveState();
    }
    public void PowerUpSelection(bool SelectOrDeselect)
    {
        this.gameObject.SetActive(SelectOrDeselect);
    }
    //public void PowerUpSelect()
    //{
    //    gameObject.SetActive(true);
    //}

    //public void PowerUpDeselect()
    //{
    //    gameObject.SetActive(false);
    //}

    public void PowerUpEnable()
    {

    }

    public void PowerUpDisable()
    {
        hook.Disable();
        gameObject.SetActive(false);
    }

    public void PowerUpAim(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// SHOOT
    public void PowerUpAction1(Vector2 delta)
    {
        if(player.mechanicalState.energy >= 15f)
        {
            hook.gameObject.SetActive(true);
            hook.Shoot(delta);
            player.mechanicalState.energy -= 15f;
            player.mechanicalState.PowerUpsStates = (byte)(player.mechanicalState.PowerUpsStates | (1 << player.mechanicalState.selectedPowerUpIdx));
        }
    }

    /// DETATCH
    public void PowerUpAction2(Vector2 delta)
    {
        hook.Disable();
        player.mechanicalState.PowerUpsStates = (byte)(player.mechanicalState.PowerUpsStates & ~(1 << (byte)delta.x));
        player.syncedUIstatesBitmask &= ~(1u << (int)(delta.x+8));
    }

    public void Tick(bool OnOrOff)
    {

    }
}
