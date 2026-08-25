using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    public void Shoot(Vector2 dir)
    {
        hook.gameObject.SetActive(true);
        hook.Shoot(dir);
    }

    public void Detatch()
    {
        if (hook.hookShoot)
        {
            hook.Disable();
        }
    }


    public void RestorePowerUpState(bool OnOrOff)
    {
        hook.RestoreState(OnOrOff);
    }

    public void SavePowerUpState()
    {
        hook.SaveState();
    }

    public void PowerUpSelect()
    {
        gameObject.SetActive(true);
    }

    public void PowerUpDeselect()
    {
        gameObject.SetActive(false);
    }

    public void PowerUpEnable()
    {

    }

    public void PowerUpDisable()
    {
        Detatch();
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
        hook.gameObject.SetActive(true);
        hook.Shoot(delta);
    }

    /// DETATCH
    public void PowerUpAction2(Vector2 delta)
    {
        Detatch();
    }

}
