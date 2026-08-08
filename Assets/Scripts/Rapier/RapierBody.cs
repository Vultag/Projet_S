using UnityEngine;



public enum BodyType // *
{
    Dynamic,
    Kinematic,
    Static
}

public class RapierBody : MonoBehaviour
{
    [HideInInspector]
    public ulong entityHandle;

    [SerializeField]
    BodyType bodyType;

    [SerializeField]
    float Mass;
    [SerializeField]
    float LinearDampening;
    [SerializeField]
    float AngularDampening;

    [Range(0.0f, 1f), SerializeField]
    float restitution = 0f;

    [SerializeField]
    bool FreezeRotation;

    void Start()
    {
        entityHandle = RapierWorld.body_create(
            RapierWorld.world, 
            (byte)bodyType, 
            Mass,
            transform.position.x, transform.position.y, 
            Mathf.Deg2Rad*transform.localEulerAngles.z,
            LinearDampening,
            AngularDampening,
            FreezeRotation
            );

        var col_listeners = GetComponents<IRapierCollisionListener>();

        if (!RapierWorld.unityToRapierEntityMap.TryAdd(entityHandle, new EntityData
        {
            GameObject = gameObject,
            Transform = transform,
            col_listener = col_listeners
        })) Debug.Log("coundt add " + entityHandle);

    }
}
