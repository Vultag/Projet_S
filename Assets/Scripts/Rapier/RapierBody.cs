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
    [SerializeField]
    bool canSleep = true;
    [SerializeField]
    bool isEnabled = true;

    //public bool cyclicSyncing = false;

    void Awake()
    {
        //Debug.Log(transform.position.x + "  " + gameObject.name);

        entityHandle = RapierWorld.body_create(
            RapierWorld.world, 
            (byte)bodyType, 
            Mass,
            transform.position.x, transform.position.y, 
            Mathf.Deg2Rad*transform.eulerAngles.z,
            LinearDampening,
            AngularDampening,
            FreezeRotation,
            canSleep,
            isEnabled
            );

        var col_listeners = GetComponents<IRapierCollisionListener>();
        var trig_listeners = GetComponents<IRapierTriggerListener>();


        GameSyncManager.rapierToUnityDatabase.Add(entityHandle, new EntityData
        {
            GameObject = gameObject,
            Transform = transform,
            col_listener = col_listeners,
            trig_listener = trig_listeners
        });

        //Debug.Log(entityHandle + " " + gameObject.name);

        transform.SetParent(null);

        ///RapierWorld.body_set_enabled(RapierWorld.world,entityHandle,isActiveAndEnabled);

    }

    //private void OnEnable()
    //{
    //    RapierWorld.body_set_enabled(RapierWorld.world, entityHandle, true);
    //}
    //private void OnDisable()
    //{
    //    RapierWorld.body_set_enabled(RapierWorld.world, entityHandle, false);
    //}

}
