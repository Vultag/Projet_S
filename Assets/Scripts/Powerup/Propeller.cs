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
    //public RapierBody playerBody;
    public PlayerNet player;

    private bool thrusting;
    private byte thrustingIndex;
    //private Vector2 thrustingDirection;

    //[HideInInspector]
    //public Image selectedUIImage;

    public void RestorePowerUpState(bool OnOrOff)
    {

    }

    public void SavePowerUpState()
    {

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

    }

    public void PowerUpAim(Vector2 dir)
    {
        float angle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle-90f);
        if (player.mechanicalState.energy >= 0.25f)
        {
            flames.SetActive(true);
            player.mechanicalState.energy -= 0.25f;
            RapierWorld.body_add_force(
               RapierWorld.world,
               player.PlayerBody.entityHandle,
               -dir.normalized.x * 50f * 3f,
               -dir.normalized.y * 50f * 3f,
               0
           );
        }
        else
            flames.SetActive(false);

        //Debug.Log(ServerManagerNet.tick + "  " + angle);
    }


    public void PowerUpAction1(Vector2 delta)
    {
    }

    public void PowerUpAction2(Vector2 delta)
    {
        flames.SetActive(false);
    }

    public void Tick(bool OnOrOff)
    {

    }
}
