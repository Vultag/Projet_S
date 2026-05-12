using UnityEngine;

public class Module : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;

    [SerializeField]
    private UI ui;

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    collision.gameObject.transform.parent.GetComponent<Player>().respawnPoint = respawnPoint.position;
    //    ui.respawnPoint.transform.position = respawnPoint.position;

    //}
}
