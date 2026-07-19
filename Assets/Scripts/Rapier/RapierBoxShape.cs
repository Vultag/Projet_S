using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

public class RapierBoxShape : MonoBehaviour
{

    [HideInInspector]
    public ulong entityHandle;

    [SerializeField]
    private float2 dimentions;
    [SerializeField]
    private float friction;


    void Start()
    {
        if (TryGetComponent<RapierBody>(out var rb))
        {
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
            RapierWorld.body_add_box_collider(RapierWorld.world, rb.entityHandle, dimentions.x/2f, dimentions.y / 2f, 0, 0, friction, (byte)gameObject.layer, collision_listen);
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
    }

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
