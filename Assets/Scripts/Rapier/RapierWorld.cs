using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public struct TransformUpdate
{
    public ulong EntityId;
    public float2 Position;
    public float Rotation;
}
public struct ColTrigEvent
{
    public ulong EntityA;
    public ulong EntityB;
    /// later enter/exit...
    //public byte type;
}
public struct UpdtatesCount
{
    public UIntPtr trans_count;
    public UIntPtr colision_event_count;
    public UIntPtr trigger_event_count;
}
public struct BodyStateFFI : INetworkSerializable
{
    public float x;
    public float y;
    public float rotation;
    public float velocityX;
    public float velocityY;
    public float angularVelocity;
    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref x);
        s.SerializeValue(ref y);
        s.SerializeValue(ref rotation);
        s.SerializeValue(ref velocityX);
        s.SerializeValue(ref velocityY);
        s.SerializeValue(ref angularVelocity);
    }
}
public class EntityData
{
    public Transform Transform;
    public GameObject GameObject;
    public IRapierCollisionListener[] col_listener;
    public IRapierTriggerListener[] trig_listener;
}

[Flags]
public enum CollisionLayer
{
    //None = (1 << 0),
    //All = ~0, 
    Player = (1 << 0),
    Obstacle = (1 << 1),
    Projectiles = (1 << 2),
    PlayerAccessories = (1 << 3),
    TeamA = (1 << 4),
    TeamB = (1 << 5),
    TeamC = (1 << 6),
    TeamD = (1 << 7),
}
internal static class RapierWorld
{
    // Precomputed collision masks
    private static readonly CollisionLayer[] CollisionMasks = new CollisionLayer[32]
    {
        ///Player
        CollisionLayer.Player | CollisionLayer.Obstacle | CollisionLayer.PlayerAccessories,
        ///Obstacle
        CollisionLayer.Player | CollisionLayer.Projectiles |  CollisionLayer.PlayerAccessories,
        ///Projectiles
        CollisionLayer.Obstacle,
        ///PlayerAccessories
        CollisionLayer.Player | CollisionLayer.Obstacle,
        ///TeamA
        CollisionLayer.TeamB | CollisionLayer.TeamC | CollisionLayer.TeamD,
        ///TeamB
        CollisionLayer.TeamA | CollisionLayer.TeamC | CollisionLayer.TeamD,
        ///TeamC
        CollisionLayer.TeamA | CollisionLayer.TeamB | CollisionLayer.TeamD,
        ///TeamD
        CollisionLayer.TeamA | CollisionLayer.TeamB | CollisionLayer.TeamC,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
    };

    static public IntPtr world;

    public static readonly uint[] LayerFilters = new uint[32];

    //static public RapierToUnityDatabase unityToRapierEntityMap = new();
    //static private Dictionary<ulong, EntityData> unityToRapierEntityMapROLLBACK = new(512);
    //static private int unityToRapierEntityMapVersion = 0;

    static NativeArray<TransformUpdate> TransformUpdates;
    static UInt32 trans_lenght;
    static NativeArray<ColTrigEvent> CollisionEvents;
    static NativeArray<ColTrigEvent> TiggerEvents;

    static Queue<ApplyForceCommand> ApplyForceCommands = new(32);
    ///static Queue<CreateBodyCommand> CreateBodyCommands = new(16);
    static Queue<DestroyBodyCommand> DestroyBodyCommands = new(16);
    /// Remove, get instead
    static Queue<CreateShapeCommand> CreateShapeCommands = new(16);

    //public static void CreateBody(byte type,float mass,Vector2 pos, float rot,float linDamp,float angDamp)
    //    => CreateBodyCommands.Enqueue(new CreateBodyCommand(type,mass,pos,rot,linDamp,angDamp));

    public static void DestroyBody(ulong handle)
        => DestroyBodyCommands.Enqueue(new DestroyBodyCommand(handle));
    public static void AddForce(ulong handle, Vector2 linear, float angular)
        => ApplyForceCommands.Enqueue(new ApplyForceCommand(handle,linear,angular));


    /* CALL FROM 1 SCRIPT ONLY WITH ONCREATE/ONDESTROY */
    [DllImport("rapier_unity")]
    static extern IntPtr world_create();
    [DllImport("rapier_unity")]
    static extern void world_destroy(IntPtr world);
    [DllImport("rapier_unity")]
    static extern IntPtr world_set_layer_filters(
        IntPtr worldPtr,
        uint[] filters
        );
    [DllImport("rapier_unity")]
    public static extern void ignore_collision(
        IntPtr world,
        ulong body1,
        ulong body2,
        bool ignore
    );
    [DllImport("rapier_unity")]
    static extern UpdtatesCount world_step(
        IntPtr worldPtr,
        float deltaTime,
        IntPtr transformUpdatesOutput,
        IntPtr collisionEventUpdatesOutput,
        IntPtr triggerEventUpdatesOutput,
        UIntPtr transCapacity,
        UIntPtr colisionEventCapacity,
        UIntPtr triggerEventCapacity
        );



    [DllImport("rapier_unity")]
    public static extern ulong body_create(
          IntPtr worldPtr,
          byte type,
          float mass,
          float x,
          float y,
          float rot,
          float linearDamp,
          float angularDamp,
          bool freezeRotation,
          bool enabled
      );
    [DllImport("rapier_unity")]
    public static extern void body_destroy(
          IntPtr world,
          ulong handle
     );
    [DllImport("rapier_unity")]
    public static extern void body_set_enabled(
         IntPtr world,
         ulong bodyHandle,
         bool enabled
     );
    [DllImport("rapier_unity")]
    public static extern void shape_set_enabled(
        IntPtr world,
        ulong shapeHandle,
        bool enabled
    );
    [DllImport("rapier_unity")]
    public static extern void joint_destroy(
        IntPtr world,
        ulong handle
    );
 



    [DllImport("rapier_unity")]
    public static extern ulong body_add_circle_collider(
          IntPtr world,
         ulong handle,
         float radius,
         float offX,
         float offY,
         float friction,
         uint layer,
         bool registerCollideEvent,
         bool isSensor
      );
    [DllImport("rapier_unity")]
    public static extern ulong body_add_box_collider(
         IntPtr world,
         ulong handle,
         float width,
         float height,
         float offX,
         float offY,
         float friction,
         uint layer,
         bool registerCollideEvent,
         bool isSensor
     ); 

    [DllImport("rapier_unity")]
    public static extern void body_add_polyline_collider(
        IntPtr world,
        ulong body,
        Vector2[] vertices,
        UIntPtr vertexCount,
        float friction,
        uint layer
    );



    [DllImport("rapier_unity")]
    public static extern ulong joint_add_fixed(
        IntPtr world,
        ulong body1,
        ulong body2,
        bool enableCollision,
        bool connectedToWorld,
        float ax1,
        float ay1,
        float ax2,
        float ay2,
        bool enabled
    );
    [DllImport("rapier_unity")]
    public static extern ulong joint_add_rope(
        IntPtr world,
        ulong body1,
        ulong body2,
        bool enableCollision,
        bool connectedToWorld,
        float ax1,
        float ay1,
        float ax2,
        float ay2,
        float maxDistance,
        bool enabled
    );
    [DllImport("rapier_unity")]
    public static extern void joint_set_rope_max_distance(
        IntPtr world,
        ulong jointHandle,
        float maxDistance
    );
    [DllImport("rapier_unity")]
    public static extern ulong joint_add_revolute(
       IntPtr world,

       ulong body1,
       ulong body2,
       bool connectedToWorld,
       bool enableCollision,

       float ax1,
       float ay1,
       float ax2,
       float ay2,

       bool useLimits,
       float minAngle,
       float maxAngle,

       bool motorEnabled,
       float targetAngle,
       float stiffness,
       float damping,

       float velocityTarget,
       float velocityGain,

       float maxTorque,
        bool enabled
   );
    [DllImport("rapier_unity")]
    public static extern ulong joint_add_prismatic(
        IntPtr world,

        ulong body1,
        ulong body2,
        bool connectedToWorld,
        bool enableCollision,

        float anchorX,
        float anchorY,
        float connectedAnchorX,
        float connectedAnchorY,

        float axisX,
        float axisY,

        bool useLimits,
        float minLimit,
        float maxLimit,

        bool motorEnabled,
        float targetPosition,
        float stiffness,
        float damping,
        float maxForce,
        bool enabled
    );
    //[DllImport("rapier_unity")]
    //internal static extern ulong joint_change_prismatic(
    //    IntPtr world,
    //    ulong jointHandle,
    //    float axisX,
    //    float axisY,
    //    float limitMin,
    //    float limitMax
    //);
    [DllImport("rapier_unity")]
    internal static extern void joint_set_prismatic_motor(
        IntPtr world,
        ulong jointHandle,
        float targetPos,
        float stiffness,
        float damping,
        float maxForce
    );
    [DllImport("rapier_unity")]
    public static extern void joint_set_enabled(
        IntPtr world,
        ulong jointHandle,
        bool enabled
    );



    [DllImport("rapier_unity")]
    public static extern void body_set_position(
         IntPtr world,
         ulong bodyHandle,
         float x,
         float y
      );
    [DllImport("rapier_unity")]
    public static extern void body_set_rotation(
        IntPtr world,
        ulong bodyHandle,
        float angle
     );
    [DllImport("rapier_unity")]
    public static extern void body_set_fixed(
       IntPtr world,
       ulong bodyHandle
    );
    [DllImport("rapier_unity")]
    public static extern void body_set_dynamic(
       IntPtr world,
       ulong bodyHandle
    );


    [DllImport("rapier_unity")]
    public static extern void body_add_force(
        IntPtr world,
        ulong handle,
        float linearX,
        float linearY,
        float angular
     );
    [DllImport("rapier_unity")]
    public static extern BodyStateFFI body_get_state(
        IntPtr world,
        ulong handle
    );
    [DllImport("rapier_unity")]
    public static extern void body_set_state(
         IntPtr world,
         ulong handle,
         BodyStateFFI bodyState
     );
    [DllImport("rapier_unity")]
    public static extern void world_store_snapshot(IntPtr world);
    [DllImport("rapier_unity")]
    public static extern void world_restore_snapshot(IntPtr world);

    [DllImport("rapier_unity")]
    public static extern ulong world_get_closest_body(
        IntPtr world,
        ulong fromBodyHandle,
        float x,
        float y,
        float radius,
        uint layer
    );
    [DllImport("rapier_unity")]
    public static extern void collider_set_memberships(
        IntPtr worldPtr,
        ulong colliderHandle,
        uint membership
    );


    public static void Create_world()
    {
        /// LARGER FOR SERVER ?
        TransformUpdates = new NativeArray<TransformUpdate>(100,Allocator.Persistent);
        CollisionEvents = new NativeArray<ColTrigEvent>(100, Allocator.Persistent);
        TiggerEvents = new NativeArray<ColTrigEvent>(100, Allocator.Persistent);

        uint[] layerFilters = new uint[32];

        for (int layer = 0; layer < 32; layer++)
        {
            layerFilters[layer] = (uint)CollisionMasks[layer];
        }
        //for (int layer = 0; layer < 32; layer++)
        //{
        //    layerFilters[layer] = (uint)Physics2D.GetLayerCollisionMask(layer);
        //    Debug.Log(LayerMask.LayerToName(layer) +  " _ " + Physics2D.GetLayerCollisionMask(layer));
        //}
        world = world_create();
        world_set_layer_filters(world, layerFilters);
    }

    public static void Destory_world()
    {
        TransformUpdates.Dispose();
        CollisionEvents.Dispose();
        TiggerEvents.Dispose();
        world_destroy(world);
    }

    public static void Collect(IntPtr world)
    {
        // read contacts, triggers, impulses
        // store as immutable data for gameplay
    }
    public static unsafe void PhysicsStep(float dt)
    {
        /// forces
        /// destroy
        /// create

        while (ApplyForceCommands.Count > 0)
        {
            ApplyForceCommands.Dequeue().Execute(world);
        }
        while (DestroyBodyCommands.Count > 0)
        {
            DestroyBodyCommands.Dequeue().Execute(world);
        }
        //while (CreateBodyCommands.Count > 0)
        //{
        //    CreateBodyCommands.Dequeue().Execute(world);
        //}
        /// Remove
        while (CreateShapeCommands.Count > 0)
        {
            CreateShapeCommands.Dequeue().Execute(world);
        }


        //if (TransformUpdates.Length != 0) Debug.Log("ooooo");
        //if (((UIntPtr)TransformUpdates.Length).ToUInt32() != 0) Debug.Log("xxxxxxx");

        //foreach (var item in RapierWorld.map)
        //{
        //    Debug.Log(item.Key);
        //}

        UpdtatesCount UpdateNum = world_step(
            world,
            dt,
            (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr(TransformUpdates),
            (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr(CollisionEvents),
            (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr(TiggerEvents),
            (UIntPtr)TransformUpdates.Length,
            (UIntPtr)CollisionEvents.Length,
            (UIntPtr)TiggerEvents.Length
            );

        //var trans_lenght = UpdateNum.trans_count.ToUInt32();
        trans_lenght = UpdateNum.trans_count.ToUInt32();
        var col_event_lenght = UpdateNum.colision_event_count.ToUInt32();
        var trig_event_lenght = UpdateNum.trigger_event_count.ToUInt32();
        //Debug.Log(transUpdateNum);

        //for (int i = 0; i < trans_lenght; i++)
        //{
        //    if(!RapierWorld.unityToRapierEntityMap.TryGetValue(TransformUpdates[i].EntityId, out var body))Debug.Log("couldnt get body " + TransformUpdates[i].EntityId);
        //    body.Transform.position = new Vector3(TransformUpdates[i].Position.x, TransformUpdates[i].Position.y, 0);
        //    body.Transform.rotation = quaternion.RotateZ(TransformUpdates[i].Rotation);
        //}

        for (int i = 0; i < col_event_lenght; i++)
        {

            GameSyncManager.rapierToUnityDatabase.TryGet(CollisionEvents[i].EntityA,out var entityA);
            GameSyncManager.rapierToUnityDatabase.TryGet(CollisionEvents[i].EntityB, out var entityB);

            foreach (var l in entityA.col_listener)
                l.OnRapierCollisionEnter(CollisionEvents[i].EntityB);
            foreach (var l in entityB.col_listener)
                l.OnRapierCollisionEnter(CollisionEvents[i].EntityA);

        }
        for (int i = 0; i < trig_event_lenght; i++)
        {
            GameSyncManager.rapierToUnityDatabase.TryGet(TiggerEvents[i].EntityA, out var entityA);
            GameSyncManager.rapierToUnityDatabase.TryGet(TiggerEvents[i].EntityB, out var entityB);

            foreach (var l in entityA.trig_listener)
            {
                l.OnRapierTriggerEnter(entityB.GameObject, TiggerEvents[i].EntityB);
                //Debug.Log(entityB.GameObject);
            }
            //foreach (var l in entityB.trig_listener)
            //{
            //    Debug.Log(entityA.GameObject);
            //}
        }
    }

    public static unsafe void UpdateTransforms()
    {
        for (int i = 0; i < trans_lenght; i++)
        {
            GameSyncManager.rapierToUnityDatabase.TryGet(TransformUpdates[i].EntityId, out var body);
            body.Transform.SetPositionAndRotation(new Vector3(TransformUpdates[i].Position.x, TransformUpdates[i].Position.y, 0), quaternion.RotateZ(TransformUpdates[i].Rotation));
        }
    }
}
