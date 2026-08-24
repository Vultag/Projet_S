using UnityEngine;

public class RapierCircleShape : MonoBehaviour
{

    [HideInInspector]
    public ulong entityHandle;

    [SerializeField] 
    private float radius;
    [SerializeField]
    private float friction;
    void Awake()
    {

        if(TryGetComponent<RapierBody>(out var rb))
        {
            var col_listeners = rb.GetComponents<IRapierCollisionListener>();

            RapierWorld.body_add_circle_collider(
                RapierWorld.world, 
                rb.entityHandle, 
                radius, 0, 0, friction, 
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

            entityHandle = RapierWorld.add_standalone_circle_collider(
                RapierWorld.world, radius, transform.position.x, transform.position.y, (byte)gameObject.layer, tigger_listen);

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
            Gizmos.DrawWireSphere(this.transform.position, radius);
       
    }


}
