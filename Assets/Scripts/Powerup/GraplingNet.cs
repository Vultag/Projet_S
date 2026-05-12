using Unity.Netcode;
using UnityEngine;

public class GraplingNet :NetworkBehaviour
{

    [SerializeField]
    GameObject hook;
    [SerializeField]
    GameObject hookSprite;

    [HideInInspector]
    public NetworkVariable<float> graplingAngle =
   new NetworkVariable<float>(
       0f,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
   );

    [HideInInspector]
    public NetworkVariable<bool> grapleActive =
    new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        grapleActive.OnValueChanged += (_, v) => UpdateGraple(v);

        var hookS = hook.GetComponent<Hook>();
        var hookNetS = hook.GetComponent<HookNet>();
        hookS.playerBody = this.transform.parent.GetComponent<Rigidbody2D>();
        hookNetS.playerBody = this.transform.parent.GetComponent<Rigidbody2D>();
        hook.SetActive(false);
        hookS.enabled = IsOwner;
        hook.transform.SetParent(null, true);
        gameObject.SetActive(false);
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, graplingAngle.Value);
    }

    //[ServerRpc]
    //public void UpdateRotationServerRpc(float angle)
    //{
    //    transform.rotation = Quaternion.Euler(0f, 0f, angle);
    //}

    private void UpdateGraple(bool state)
    {
        hook.SetActive(state);
        hookSprite.SetActive(!state);
    }

    //[ServerRpc]
    //public void ActivateHookServerRpc(bool state)
    //{
    //    Debug.Log("fff");
    //    hook.SetActive(state);
    //    hookSprite.SetActive(!state);
    //}

}
