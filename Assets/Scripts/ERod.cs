using Unity.Netcode;
using UnityEngine;

public class ERod : NetworkBehaviour
{

    private void Start()
    {
        if (!IsServer)
        {
            this.enabled = false;
            return;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //collision.gameObject.transform.parent.GetComponent<Player>().Respawn();
    }

}
