using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D;

public class Hook : MonoBehaviour, IRapierCollisionListener
{

    public RapierBody playerBody;

    //private ulong attachJointHandle;
    private ulong rollbackDistanceJointHandle;
    private ulong distanceJointHandle;

    private bool rollBackHookShoot;
    [HideInInspector]
    public bool hookShoot;
    [HideInInspector]
    public RapierBody body;

    [SerializeField]
    private SpriteShapeController hookRope;


    private void Awake()
    {
        body = GetComponent<RapierBody>();
        gameObject.SetActive(false);
    }

    public void Disable()
    {
        if (distanceJointHandle != 0)
        {
            RapierWorld.joint_destroy(RapierWorld.world, distanceJointHandle);
            distanceJointHandle = 0;
        }
        RapierWorld.body_set_enabled(RapierWorld.world, body.entityHandle, false);
        /// Useless and cause crash ?
        ////RapierWorld.body_set_dynamic(RapierWorld.world, body.entityHandle);
        hookShoot = false;
        this.gameObject.SetActive(false);
    }

    public void SaveState()
    {
        rollbackDistanceJointHandle = distanceJointHandle;
        rollBackHookShoot = hookShoot;
    }
    public void RestoreState(bool OnOrOff)
    {
        this.gameObject.SetActive(OnOrOff);
        distanceJointHandle = rollbackDistanceJointHandle;
        hookShoot = rollBackHookShoot;
    }

    public void Shoot(Vector2 dir)
    {
        if (distanceJointHandle != 0)
        {
            RapierWorld.joint_destroy(RapierWorld.world, distanceJointHandle);
            distanceJointHandle = 0;
        }
        float angle = Mathf.Atan2(dir.y, dir.x);
        RapierWorld.body_set_enabled(RapierWorld.world, body.entityHandle, true);
        RapierWorld.body_set_dynamic(RapierWorld.world, body.entityHandle);
        var playerState = RapierWorld.body_get_state(RapierWorld.world, playerBody.entityHandle);
        RapierWorld.body_set_state(RapierWorld.world, body.entityHandle,
            new BodyStateFFI
            {
                x = playerState.x,
                y = playerState.y,
                rotation = angle,
                velocityX = 0,
                velocityY = 0,
                angularVelocity = 0
            });
        RapierWorld.AddForce(body.entityHandle, dir.normalized * 600, 0);
        hookShoot = true;
    }

    private void Update()
    {
        hookRope.spline.SetPosition(1, transform.InverseTransformPoint(playerBody.transform.position));
        hookRope.RefreshSpriteShape();

    }


    public void OnRapierCollisionEnter(ulong HandleColiderEntering)
    {
        //if (!hookShoot)
        //{
        //    Debug.Log("kkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
        //    return;
        //}

        RapierWorld.body_set_fixed(RapierWorld.world, body.entityHandle);
        var playerState = RapierWorld.body_get_state(RapierWorld.world, playerBody.entityHandle);
        var hookState = RapierWorld.body_get_state(RapierWorld.world, body.entityHandle);
        distanceJointHandle = RapierWorld.joint_add_rope(
            RapierWorld.world,
            body.entityHandle,
            playerBody.entityHandle,
            false,
            false,
            0, 0, 0, 0,
            (new Vector2(playerState.x, playerState.y) - new Vector2(hookState.x, hookState.y)).magnitude,
            true
            );

    }

    public void OnRapierCollisionExit(ulong HandleColiderExiting)
    {

    }

}
