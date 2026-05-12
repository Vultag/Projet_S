using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D;

public class HookNet : NetworkBehaviour
{
    [HideInInspector]
    public Rigidbody2D playerBody;
    [SerializeField]
    private SpriteShapeController hookRope;

    [HideInInspector]
    public NetworkVariable<Vector2> hookPos =
    new NetworkVariable<Vector2>(
       Vector2.zero,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {

    }
    private void Update()
    {
        if (IsOwner) return;

        transform.position = hookPos.Value;
        hookRope.spline.SetPosition(1, transform.InverseTransformPoint(playerBody.transform.position));
        hookRope.RefreshSpriteShape();
    }
}
