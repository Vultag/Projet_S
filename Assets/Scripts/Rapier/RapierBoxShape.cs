using Unity.Mathematics;
using UnityEngine;

public class RapierBoxShape : MonoBehaviour
{

    [HideInInspector]
    public ulong entityHandle;

    [SerializeField]
    private float2 dimentions;
    [SerializeField]
    private float friction;


    void Awake()
    {
        if (TryGetComponent<RapierBody>(out var rb))
        {
            var col_listeners = rb.GetComponents<IRapierCollisionListener>();

            RapierWorld.body_add_box_collider(
                RapierWorld.world, rb.entityHandle
                , dimentions.x/2f, dimentions.y / 2f, 0, 0, friction, 
                (byte)gameObject.layer,
                col_listeners.Length > 0
                );
        }
        else
        {
            var trig_listeners = GetComponents<IRapierTriggerListener>();

            bool tigger_listen = false;
            foreach (var listener in trig_listeners)
            {
                if (listener is MonoBehaviour mono && mono.isActiveAndEnabled)
                {
                    tigger_listen = true;
                    break;
                }
            }

            if (!tigger_listen)
            {
                Debug.Log("NO LISTENER");
                return;
            }

            entityHandle = RapierWorld.add_standalone_box_collider(
                RapierWorld.world, dimentions.x / 2f, dimentions.y / 2f, transform.position.x, transform.position.y,(byte)gameObject.layer, tigger_listen);


            if (!RapierWorld.unityToRapierEntityMap.TryAdd(entityHandle, new EntityData
            {
                GameObject = gameObject,
                Transform = transform,
                trig_listener = trig_listeners
            })) Debug.Log("coundt add " + entityHandle);

        }

        RapierWorld.shape_set_enabled(RapierWorld.world, entityHandle, isActiveAndEnabled);
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
