using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class RapierWorldManager : MonoBehaviour
{
    //IntPtr rapierWorldPtr;


    private void Awake()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;

    }



    private void Start()
    {
        RapierWorld.Create_world();
        //Debug.Log(rapierWorldPtr); 

        //RapierWorld.PhysicsStep(1f / 60f);


        ///// COMPOUND
        //foreach (var compound in FindObjectsByType<CompositeCollider2D>(FindObjectsSortMode.None))
        //{
        //    if (compound.attachedRigidbody == null)
        //        Debug.Log(" COMPOND WITH NO BODY");

        //    Rigidbody2D rb = compound.attachedRigidbody;
        //    var handle = RapierWorld.body_create(RapierWorld.world, (byte)rb.bodyType, rb.transform.position.x, rb.transform.position.y, rb.rotation);

        //    if (compound.pathCount != 1) Debug.Log(" PATH PROBLEM");

        //    UIntPtr count = (UIntPtr)compound.GetPathPointCount(0);

        //    Vector2[] points = new Vector2[count.ToUInt64()];
        //    compound.GetPath(0, points);

        //    RapierWorld.body_add_polyline_collider(RapierWorld.world, handle, points, count);

        //    Destroy(rb);
        //    Destroy(compound);
        //}

        /*

        /// CIRCLE
        foreach (var circle in FindObjectsByType<CircleCollider2D>(FindObjectsSortMode.None))
        {
            if (circle.composite)
            {
                Destroy(circle);
                continue;
            }

            if (circle.attachedRigidbody != null)
            {
                Rigidbody2D rb = circle.attachedRigidbody;
                var handle = RapierWorld.body_create(RapierWorld.world, (byte)rb.bodyType, rb.transform.position.x, rb.transform.position.y, rb.rotation);

                var col_listeners = rb.GetComponents<IRapierCollisionListener>();
                bool collision_listen = false;
                foreach (var listener in col_listeners)
                {
                    if (listener is MonoBehaviour mono && mono.isActiveAndEnabled)
                    {
                        collision_listen = true;
                        break;
                    }
                }

                if (!RapierWorld.unityToRapierEntityMap.TryAdd(handle, new EntityData
                {
                    GameObject = rb.gameObject,
                    Transform = rb.transform,
                    col_listener = col_listeners
                })) Debug.Log("coundt add " + handle);

                RapierWorld.body_add_circle_collider(RapierWorld.world, handle, circle.radius, 0, 0, collision_listen);
                Destroy(rb);
            }
            else
            {

                var trig_listeners = circle.GetComponents<IRapierTriggerListener>();

                bool tigger_listen = false;
                foreach (var listener in trig_listeners)
                {
                    if (listener is MonoBehaviour mono && mono.isActiveAndEnabled)
                    {
                        tigger_listen = true;
                        break;
                    }
                }

                ulong handle = RapierWorld.add_standalone_circle_collider(
                    RapierWorld.world, circle.radius, circle.transform.position.x, circle.transform.position.y, tigger_listen);
                
                
                if (!RapierWorld.unityToRapierEntityMap.TryAdd(handle, new EntityData
                {
                    GameObject = circle.gameObject,
                    Transform = circle.transform,
                    trig_listener = trig_listeners
                })) Debug.Log("coundt add " + handle);
            }
            Destroy(circle);
        }

        /// BOX
        foreach (var box in FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))
        {
            if (box.composite)
            {
                Destroy(box);
                continue;
            }


            if (box.attachedRigidbody != null)
            {
                Rigidbody2D rb = box.attachedRigidbody;
                var handle = RapierWorld.body_create(RapierWorld.world, (byte)rb.bodyType, rb.transform.position.x, rb.transform.position.y, rb.rotation);
                var col_listeners = rb.GetComponents<IRapierCollisionListener>();

                bool collision_listen = false;
                foreach (var listener in col_listeners)
                {
                    if (listener is MonoBehaviour mono && mono.isActiveAndEnabled)
                    {
                        collision_listen = true;
                        break;
                    }
                }

                if (!RapierWorld.unityToRapierEntityMap.TryAdd(handle, new EntityData
                {
                    GameObject = rb.gameObject,
                    Transform = rb.transform,
                    col_listener = col_listeners,
                    //
                })) Debug.Log("coundt add " + handle);

                RapierWorld.body_add_box_collider(RapierWorld.world, handle, box.size.x / 2f, box.size.y / 2f, 0, 0, collision_listen);
                Destroy(rb);
            }
            else
            {
                var trig_listeners = box.GetComponents<IRapierTriggerListener>();


                bool tigger_listen = false;
                foreach (var listener in trig_listeners)
                {
                    if (listener is MonoBehaviour mono && mono.isActiveAndEnabled)
                    {
                        tigger_listen = true;
                        break;
                    }
                }

                ulong handle = RapierWorld.add_standalone_box_collider(
                    RapierWorld.world, box.size.x / 2f, box.size.y / 2f, box.transform.position.x, box.transform.position.y, tigger_listen);

                if (!RapierWorld.unityToRapierEntityMap.TryAdd(handle, new EntityData
                {
                    GameObject = box.gameObject,
                    Transform = box.transform,
                    trig_listener = trig_listeners
                })) Debug.Log("coundt add " + handle);
            }
            Destroy(box);
        }
        */


        //foreach (var body in FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
        //{
        //    var handle = RapierWorld.body_create(RapierWorld.world, (byte)body.bodyType, body.transform.position.x, body.transform.position.y, body.rotation);

        //    if (!RapierWorld.unityToRapierEntityMap.TryAdd(handle, new EntityData{ 
        //    GameObject = body.gameObject,
        //    Transform = body.transform,
        //    Listeners = body.GetComponents<IRapierCollisionListener>()
        //    } )) Debug.Log("coundt add " + handle);

        //    if (body.gameObject.TryGetComponent<CompositeCollider2D>(out var composite))
        //    {
        //        if(composite.pathCount!=1) Debug.Log(" PATH PROBLEM");

        //        UIntPtr count = (UIntPtr)composite.GetPathPointCount(0);

        //        Vector2[] points = new Vector2[count.ToUInt64()];
        //        composite.GetPath(0, points);

        //        RapierWorld.body_add_polyline_collider(RapierWorld.world, handle, points, count);

        //        Destroy(composite);
        //    }
        //    if (body.gameObject.TryGetComponent<CircleCollider2D>(out var cicle))
        //    {
        //        if (!cicle.composite) RapierWorld.body_add_circle_collider(RapierWorld.world, handle, cicle.radius, 0, 0);
        //        Destroy(cicle);
        //    }
        //    if (body.gameObject.TryGetComponent<BoxCollider2D>(out var box))
        //    {
        //        if (!box.composite) RapierWorld.body_add_box_collider(RapierWorld.world, handle, box.size.x/2f, box.size.y / 2f, 0, 0);
        //        Destroy(box);
        //    }

        //    Destroy(body);
        //}

        /// joints

    }
    private void OnDisable()
    {
        RapierWorld.Destory_world();
    }

    //private void FixedUpdate()
    //{
    //    RapierWorld.PhysicsStep(1f / 60f);
    //}


}
