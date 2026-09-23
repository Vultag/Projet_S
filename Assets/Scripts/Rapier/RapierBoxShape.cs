using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

public class RapierBoxShape : MonoBehaviour
{

    [HideInInspector]
    public ulong colliderHandle;

    [SerializeField]
    private CollisionLayer layers;

    [SerializeField]
    private float2 dimentions;
    [SerializeField]
    private float friction;
    [SerializeField]
    bool isSensor;


    void Awake()
    {
        if (TryGetComponent<RapierBody>(out var rb))
        {
            var col_listeners = rb.GetComponents<IRapierCollisionListener>();
            var trig_listeners = rb.GetComponents<IRapierTriggerListener>();

            colliderHandle = RapierWorld.body_add_box_collider(
                RapierWorld.world, 
                rb.entityHandle,
            dimentions.x/2f, dimentions.y / 2f, 0, 0, friction,
                (uint)layers,
                col_listeners.Length > 0 | trig_listeners.Length>0,
                isSensor
                );
            //Debug.Log((gameObject.layer) + gameObject.name);
        }
        else
        {
            Debug.LogError("Collider with no body " + gameObject.name);
        }

        RapierWorld.shape_set_enabled(RapierWorld.world, colliderHandle, isActiveAndEnabled);
    }

    //private void OnEnable()
    //{
    //    RapierWorld.shape_set_enabled(RapierWorld.world, entityHandle, true);
    //}
    //private void OnDisable()
    //{
    //    RapierWorld.shape_set_enabled(RapierWorld.world, entityHandle, false);
    //}

    private void OnDrawGizmos()
    {
        
        Vector3 position = transform.position;
        Quaternion rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z); // Only Z rotation for 2D
        Vector3 scale = new Vector3(dimentions.x, dimentions.y, 1f);

        // Save the old matrix
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Apply the new matrix (position * rotation * scale)
        Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

        // Draw cube centered at origin, it will be transformed by the matrix
        Gizmos.DrawWireCube(Vector3.zero, scale);

        // Restore the old matrix
        Gizmos.matrix = oldMatrix;
        
    }

}
