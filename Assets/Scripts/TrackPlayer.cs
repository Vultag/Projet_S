using UnityEngine;

public class TrackPlayer : MonoBehaviour
{
    [HideInInspector]
    public GameObject PlayerGB;

    void Update()
    {
        var playerPos = PlayerGB.transform.position;
        Camera.main.transform.position = new Vector3(playerPos.x, playerPos.y, Camera.main.transform.position.z);
    }
}
