using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Propeller : MonoBehaviour, PowerUpInterface
{

    //[SerializeField]
    //private GameObject Thruster;
    [SerializeField]
    private GameObject flames;
    public RapierBody playerBody;

    private bool thrusting;
    private byte thrustingIndex;
    //private Vector2 thrustingDirection;

    public void RestorePowerUpState(bool OnOrOff)
    {

    }

    public void SavePowerUpState()
    {

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

    }

    public void PowerUpAim(Vector2 dir)
    {
        float angle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle-90f);
        RapierWorld.AddForce(playerBody.entityHandle, -dir.normalized *50f, 0);
    }


    public void PowerUpAction1(Vector2 delta)
    {
        flames.SetActive(true);
    }

    public void PowerUpAction2(Vector2 delta)
    {
        flames.SetActive(false);
    }

}
