using Unity.Netcode;
using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

public class Bumper : NetworkBehaviour
{
    private void Start()
    {
  
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            this.enabled = false;
            return;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //var contact = collision.GetContact(0);
        //if (contact.rigidbody.transform.parent.GetComponent<PlayerNet>().IsClient) return;
        ////Debug.Log("bump");
        //contact.rigidbody.transform.parent.GetComponent<PlayerNet>().Bump(-contact.normal * 2000);


    }

    /// FX -> client rcp


    //[ServerRpc]
    //public void BumpServerRpc(Collision2D collision)
    //{
    //    //body.AddForce(force);
    //}
}
