using UnityEngine;

public class PowerUpPassif : MonoBehaviour
{

    [HideInInspector]
    public Player player;
    [HideInInspector]
    public int powerUpIdx;

    private void FixedUpdate()
    {
        player.RegisterAction(Action.PowerUpPassif,new Vector2(powerUpIdx, 0));
    }

}
