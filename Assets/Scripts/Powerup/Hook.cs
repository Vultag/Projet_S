using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D;

public class Hook : MonoBehaviour, IRapierCollisionListener
{

    [HideInInspector]
    public RapierBody playerBody;

    private ulong DistanceJointHandleROLLBACK;
    public ulong distanceJointHandle;
    private ulong AttachJointHandleROLLBACK;
    private ulong attachJointHandle;

    private bool HookShootROLLBACK;
    [HideInInspector]
    public bool hookShoot;
    private bool hookedROLLBACK;
    [HideInInspector]
    public bool hooked;
    [HideInInspector]
    public RapierBody body;
    [HideInInspector]
    public RapierBoxShape shape;

    [SerializeField]
    private SpriteShapeController hookRope;


    private void Awake()
    {
        body = GetComponent<RapierBody>();
        shape = GetComponent<RapierBoxShape>();
        gameObject.SetActive(false);
        RapierWorld.body_set_enabled(RapierWorld.world, body.entityHandle, true);

    }

    public void Disable()
    {
        if (distanceJointHandle == 0 && attachJointHandle != 0) Debug.LogError("aaaaaaa");
        if (attachJointHandle == 0 && distanceJointHandle != 0) Debug.LogError("bbbbbbb");

        if (hooked)
        {
            RapierWorld.joint_destroy(RapierWorld.world, distanceJointHandle);
            RapierWorld.joint_destroy(RapierWorld.world, attachJointHandle);
            distanceJointHandle = 0;
            attachJointHandle = 0;
            //Debug.Log("destroyyy");
        }
        hooked = false;

        //RapierWorld.body_set_enabled(RapierWorld.world, body.entityHandle, false);
        /// Useless and cause crash ?
        ////RapierWorld.body_set_dynamic(RapierWorld.world, body.entityHandle);
        hookShoot = false;
        this.gameObject.SetActive(false);
    }

    public void SaveState()
    {
        DistanceJointHandleROLLBACK = distanceJointHandle;
        AttachJointHandleROLLBACK = attachJointHandle;
        HookShootROLLBACK = hookShoot;
        hookedROLLBACK = hooked;
        //RapierWorld.wake_up_body(RapierWorld.world, body.entityHandle);
    }
    public void RestoreState(bool OnOrOff)
    {
        this.gameObject.SetActive(OnOrOff);
        distanceJointHandle = DistanceJointHandleROLLBACK;
        attachJointHandle = AttachJointHandleROLLBACK;
        hookShoot = HookShootROLLBACK;
        hooked = hookedROLLBACK;
    }

    public void Shoot(Vector2 dir)
    {

        //if(!GameSyncManager.DEBUGreconciliating) Debug.Log("shoot at " + ServerManagerNet.tick);

        if (distanceJointHandle == 0 && attachJointHandle != 0) Debug.LogError("aaaaaaa");
        if (attachJointHandle == 0 && distanceJointHandle != 0) Debug.LogError("bbbbbbb");

        if (hooked)
        {
            RapierWorld.joint_destroy(RapierWorld.world, distanceJointHandle);
            RapierWorld.joint_destroy(RapierWorld.world, attachJointHandle);
            distanceJointHandle = 0;
            attachJointHandle = 0;
            //Debug.Log("destroy");

        }
        hooked = false;
        hookShoot = true;

        float angle = Mathf.Atan2(dir.y, dir.x);
        //RapierWorld.body_set_dynamic(RapierWorld.world, body.entityHandle);
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

        RapierWorld.collider_set_memberships(
          RapierWorld.world,
          shape.colliderHandle,
          (uint)CollisionLayer.Projectiles
          );
        //RapierWorld.body_set_enabled(RapierWorld.world, body.entityHandle, true);
    }

    private void Update()
    {
        hookRope.spline.SetPosition(1, transform.InverseTransformPoint(playerBody.transform.position));
        hookRope.RefreshSpriteShape();

    }


    public void OnRapierCollisionEnter(ulong HandleBodyEntering, Vector2 normal)
    {

        //Debug.Log(hookShoot);

        if (!hookShoot | hooked)
        {
            //Debug.Log(hookShoot);
            //Debug.Log(hooked);
            //Debug.Log("attach at " + ServerManagerNet.tick);
            return;
        }
        hooked = true;

        if (distanceJointHandle != 0) Debug.Log("azeaze");
        if (attachJointHandle != 0) Debug.Log("qsdqsd");

        ///RapierWorld.body_set_fixed(RapierWorld.world, body.entityHandle);
        var playerState = RapierWorld.body_get_state(RapierWorld.world, playerBody.entityHandle);
        var hookState = RapierWorld.body_get_state(RapierWorld.world, body.entityHandle);

        var hitBodyState = RapierWorld.body_get_state(RapierWorld.world, HandleBodyEntering);

        Vector2 anchor = Quaternion.Euler(0, 0, -hitBodyState.rotation * Mathf.Rad2Deg)
            * (new Vector2(hookState.x, hookState.y) - new Vector2(hitBodyState.x, hitBodyState.y));

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


        RapierWorld.collider_set_memberships(
          RapierWorld.world,
          shape.colliderHandle,
          0
          );
    }

    public void OnRapierCollisionExit(ulong HandleColiderExiting, Vector2 normal)
    {

    }

}
