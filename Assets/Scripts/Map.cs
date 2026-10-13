using UnityEngine;

public class Map : MonoBehaviour
{


    /// OPTI : array to be able to index them
    public GameObject TeamAspawner;
    public GameObject TeamBspawner;

    [HideInInspector]
    public PlayerSpawner playerSpawner;

    private void Awake()
    {
        playerSpawner = this.GetComponent<PlayerSpawner>();
        playerSpawner.map = this;

        foreach (var item in TeamAspawner.GetComponentsInChildren<SpriteRenderer>())
        {
            item.color = new Color(1,0,0, item.color.a);
        }
        foreach (var item in TeamBspawner.GetComponentsInChildren<SpriteRenderer>())
        {
            item.color = new Color(0, 0, 1, item.color.a);
        }
    }

}
