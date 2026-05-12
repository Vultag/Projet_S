using Unity.Netcode;
using UnityEngine;

public class PropellerNet : NetworkBehaviour
{
    [SerializeField]
    GameObject flames;
    //[SerializeField]
    //GameObject thruster;

    public NetworkVariable<bool> flamesActive =
     new NetworkVariable<bool>(
         false,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
     );

    public override void OnNetworkSpawn()
    {
        flamesActive.OnValueChanged += (_, v) => UpdateFlames(v);
    }


    [ServerRpc]
    public void UpdateRotationServerRpc(float angle)
    {
        transform.parent.rotation = Quaternion.Euler(0f, 0f, angle + 90);
    }
    public void UpdateFlames(bool state)
    {
        flames.SetActive(state);
    }

    [ServerRpc]
    public void ActivateFlamesServerRpc(bool state)
    {
        flamesActive.Value = state;
    }
    //[ServerRpc]
    //public void ActivateThrusterServerRpc(bool state)
    //{
    //    thruster.SetActive(state);
    //}
}
