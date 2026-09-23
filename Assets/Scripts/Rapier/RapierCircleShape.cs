using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class RapierCircleShape : MonoBehaviour
{

    [HideInInspector]
    public ulong colliderHandle;

    [SerializeField]
    private CollisionLayer layers;

    [SerializeField] 
    private float radius;
    [SerializeField]
    private float friction;
    [SerializeField]
    bool isSensor;

    void Awake()
    {

        if(TryGetComponent<RapierBody>(out var rb))
        {
            var col_listeners = rb.GetComponents<IRapierCollisionListener>();
            var trig_listeners = rb.GetComponents<IRapierTriggerListener>();

            colliderHandle = RapierWorld.body_add_circle_collider(
                RapierWorld.world, 
                rb.entityHandle, 
                radius, 0, 0, friction, 
                (uint)layers,
                col_listeners.Length > 0 | trig_listeners.Length > 0,
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
            Gizmos.DrawWireSphere(this.transform.position, radius);
       
    }


}
