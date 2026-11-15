using System;
using UnityEngine;

public class RapierCompositeShape : MonoBehaviour
{

    [SerializeField]
    private CollisionLayer layers;
    [SerializeField]
    private float friction;

    void Awake()
    {

        if (!TryGetComponent<CompositeCollider2D>(out var composite)) Debug.Log("NOT COMPOSITE");

        if (composite.attachedRigidbody == null)
            Debug.Log(" COMPOND WITH NO BODY");

        Rigidbody2D rb = composite.attachedRigidbody;
        var handle = RapierWorld.body_create(
            RapierWorld.world,
            2,
            1,
            transform.position.x,
            transform.position.y,
            Mathf.Deg2Rad * transform.localEulerAngles.z,
            0,0,
            false,
            false,
            true
            );


        var col_listeners = GetComponents<IRapierCollisionListener>();
        var trig_listeners = GetComponents<IRapierTriggerListener>();
        GameSyncManager.rapierToUnityDatabase.Add(handle, new EntityData
        {
            GameObject = gameObject,
            Transform = transform,
            col_listener = col_listeners,
            trig_listener = trig_listeners
        });

        if (composite.pathCount != 1) Debug.Log(" PATH PROBLEM");

        UIntPtr count = (UIntPtr)composite.GetPathPointCount(0);

        Vector2[] points = new Vector2[count.ToUInt64()];
        composite.GetPath(0, points);

        RapierWorld.body_add_polyline_collider(RapierWorld.world, handle, points, count,friction,(uint)layers);

        foreach (var box in GetComponentsInChildren<BoxCollider2D>())
        {
            Destroy(box);
        }
        foreach (var circle in GetComponentsInChildren<CircleCollider2D>())
        {
            Destroy(circle);
        }

        Destroy(composite);
        Destroy(rb);
        

    }
}
