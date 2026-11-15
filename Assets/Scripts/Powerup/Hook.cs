using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D;

public class Hook : MonoBehaviour, IRapierCollisionListener
{

    public RapierBody playerBody;

    private ulong rollbackDistanceJointHandle;
    private ulong distanceJointHandle;
    private ulong rollbackAttachJointHandle;
    private ulong attachJointHandle;

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
        if (distanceJointHandle == 0 && attachJointHandle != 0) Debug.LogError("aaaaaaa");
        if (attachJointHandle == 0 && distanceJointHandle != 0) Debug.LogError("bbbbbbb");

        if (distanceJointHandle != 0)
        {
            RapierWorld.joint_destroy(RapierWorld.world, distanceJointHandle);
            //RapierWorld.joint_destroy(RapierWorld.world, attachJointHandle);
            distanceJointHandle = 0;
            //attachJointHandle = 0;
        }
        if (attachJointHandle != 0)
        {
            RapierWorld.joint_destroy(RapierWorld.world, attachJointHandle);
            attachJointHandle = 0;
        }

        ////RapierWorld.body_set_enabled(RapierWorld.world, body.entityHandle, false);
        /// Useless and cause crash ?
        ////RapierWorld.body_set_dynamic(RapierWorld.world, body.entityHandle);
        hookShoot = false;
        this.gameObject.SetActive(false);
    }

    public void SaveState()
    {
        rollbackDistanceJointHandle = distanceJointHandle;
        rollbackAttachJointHandle = attachJointHandle;
        rollBackHookShoot = hookShoot;
    }
    public void RestoreState(bool OnOrOff)
    {
        this.gameObject.SetActive(OnOrOff);
        distanceJointHandle = rollbackDistanceJointHandle;
        attachJointHandle = rollbackAttachJointHandle;
        hookShoot = rollBackHookShoot;
    }

    public void Shoot(Vector2 dir)
    {

        if (distanceJointHandle == 0 && attachJointHandle != 0) Debug.LogError("aaaaaaa");
        if (attachJointHandle == 0 && distanceJointHandle != 0) Debug.LogError("bbbbbbb");

        if (distanceJointHandle != 0)
        {
            RapierWorld.joint_destroy(RapierWorld.world, distanceJointHandle);
            //RapierWorld.joint_destroy(RapierWorld.world, attachJointHandle);
            distanceJointHandle = 0;
            //attachJointHandle = 0;
        }
        if (attachJointHandle != 0)
        {
            RapierWorld.joint_destroy(RapierWorld.world, attachJointHandle);
            attachJointHandle = 0;
        }

        float angle = Mathf.Atan2(dir.y, dir.x);
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
        RapierWorld.body_add_force(
               RapierWorld.world,
               body.entityHandle,
               dir.normalized.x * 600f,
               dir.normalized.y * 600f,
               0
           );
        RapierWorld.body_set_enabled(RapierWorld.world, body.entityHandle, true);
        hookShoot = true;
    }

    private void Update()
    {
        hookRope.spline.SetPosition(1, transform.InverseTransformPoint(playerBody.transform.position));
        hookRope.RefreshSpriteShape();

    }


    public void OnRapierCollisionEnter(ulong HandleBodyEntering, Vector2 normal)
    {
        if (!hookShoot | distanceJointHandle!=0 | attachJointHandle != 0)
        {
            return;
        }


        ///RapierWorld.body_set_fixed(RapierWorld.world, body.entityHandle);
        var playerState = RapierWorld.body_get_state(RapierWorld.world, playerBody.entityHandle);
        var hookState = RapierWorld.body_get_state(RapierWorld.world, body.entityHandle);

        var hitBodyState = RapierWorld.body_get_state(RapierWorld.world, HandleBodyEntering);

        Vector2 anchor = Quaternion.Euler(0, 0, -hitBodyState.rotation * Mathf.Rad2Deg)
            * (new Vector2(hookState.x, hookState.y) - new Vector2(hitBodyState.x, hitBodyState.y));

        //if (distanceJointHandle != 0) 
        //{
        //    Debug.Log("ccccc");
        //}
        //if (attachJointHandle != 0)
        //{
        //    Debug.Log("dddd");
        //}

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
        attachJointHandle = RapierWorld.joint_add_fixed(
            RapierWorld.world,
            body.entityHandle,
            HandleBodyEntering,
            false,
            false,
            0, 0, anchor.x, anchor.y,
            true
            );

    }

    public void OnRapierCollisionExit(ulong HandleColiderExiting, Vector2 normal)
    {

    }

}
