using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class RapierFixedJoint : MonoBehaviour
{
    public ulong handle { get; private set; }


    [SerializeField]
    private RapierBody connectedBody;
    [SerializeField]
    private bool enableCollision;

    [SerializeField]
    private float2 anchor;
    [SerializeField]
    private float2 connectedAnchor;
    [SerializeField]
    bool startEnabled = true;



    void Awake()
    {
        if (!TryGetComponent<RapierBody>(out var body)) Debug.Log("NO BODY ON JOINT");
        bool connectedToWorld = connectedBody == null;

        handle = RapierWorld.joint_add_fixed(
                RapierWorld.world,
                body.entityHandle,
                connectedToWorld ? 0 : connectedBody.entityHandle,
                connectedToWorld,
                enableCollision,
                anchor.x,
                anchor.y,
                connectedAnchor.x,
                connectedAnchor.y,
                startEnabled
            );

    }

    //private void OnEnable()
    //{
    //    RapierWorld.joint_set_enabled(RapierWorld.world, handle, true);
    //}
    //private void OnDisable()
    //{
    //    RapierWorld.joint_set_enabled(RapierWorld.world, handle, false);
    //}
}
