using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class StickyFoam : MonoBehaviour, PowerUpInterface,IRapierTriggerListener
{

    [HideInInspector]
    public RapierBody playerBody;
    [HideInInspector]
    public ulong shapeHandle;

    private bool stickedROLLBACK;
    private bool sticked;

    private ulong stickJointHandle;
    private ulong stickJointHandleROLLBACK;

    private void Start()
    {
    }


    public void PowerUpAction1(Vector2 delta)
    {
        throw new System.NotImplementedException();
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
        if (sticked)
        {
            RapierWorld.joint_destroy(RapierWorld.world, stickJointHandle);
            stickJointHandle = 0;
        }
        sticked = false;

        this.gameObject.SetActive(false);
        RapierWorld.shape_set_enabled(RapierWorld.world, shapeHandle, false);
    }


    public void PowerUpSelection(bool SelectOrDeselect)
    {
        this.gameObject.SetActive(SelectOrDeselect);
        if (sticked)
        {
            RapierWorld.joint_destroy(RapierWorld.world, stickJointHandle);
            stickJointHandle = 0;
        }
        sticked = false;
        RapierWorld.shape_set_enabled(RapierWorld.world, shapeHandle, SelectOrDeselect);
    }

    public void RestorePowerUpState(bool OnOrOff)
    {
        enabled = OnOrOff;
        sticked = stickedROLLBACK;
        stickJointHandle = stickJointHandleROLLBACK;
    }

    public void SavePowerUpState()
    {
        stickedROLLBACK = sticked;
        stickJointHandleROLLBACK = stickJointHandle;
    }

    public void Tick(bool OnOrOff)
    {

    }

    public void OnRapierTriggerEnter(GameObject EntityEntering, ulong EntityEnteringID)
    {
        if (sticked)
        {
            /// when 2 object on trig enter
            //Debug.LogError("already sticked");
            return;
        }

        var playerState = RapierWorld.body_get_state(RapierWorld.world,playerBody.entityHandle);
        var collidedState = RapierWorld.body_get_state(RapierWorld.world, EntityEnteringID);

        Vector2 anchor = Quaternion.Euler(0, 0, -collidedState.rotation * Mathf.Rad2Deg)
           * (new Vector2(playerState.x, playerState.y) - new Vector2(collidedState.x, collidedState.y));

        stickJointHandle = RapierWorld.joint_add_rope(
            RapierWorld.world,
            playerBody.entityHandle,
            EntityEnteringID,
            true,
            false,
            0, 0, anchor.x, anchor.y,
            0.05f,
            true
            );

        sticked = true;
        RapierWorld.shape_set_enabled(RapierWorld.world, shapeHandle, false);

    }

    public void OnRapierTriggerExit(GameObject EntityExiting, ulong EntityExitingID)
    {
        throw new System.NotImplementedException();
    }
}
