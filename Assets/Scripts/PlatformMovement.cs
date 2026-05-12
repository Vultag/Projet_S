using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlatformMovement : MonoBehaviour
{
    [SerializeField]
    private float travelDurartion;
    [SerializeField]
    private Vector2 mouvTarget;
    private Vector2 mouvStart;
    private Vector2[] positionArray;
    private byte positionState;
    private byte mouvState;
    private float idleTime;
    private byte isIdleTimeTicking = 0;
    private Vector2 travelDelta;
    [SerializeField]
    private float idleDuration;
    [SerializeField]
    private float startTimeOffset;

    Rigidbody2D body;

    private void Start()
    {
        body = this.GetComponent<Rigidbody2D>();
        mouvStart = new Vector2(this.transform.position.x, this.transform.position.y);
        /// global to local
        mouvTarget = new Vector2(this.transform.parent.position.x, this.transform.parent.position.y) + mouvTarget;

        Vector2 distance = (mouvTarget - mouvStart);
        travelDelta = travelDurartion==0?Vector2.zero: distance /(travelDurartion);

        positionArray = new Vector2[2];
        positionArray[0] = mouvStart;
        positionArray[1] = mouvTarget;

        if (travelDurartion + idleDuration == 0)
            enabled = false;
    }


    private void FixedUpdate()
    {
        idleTime += isIdleTimeTicking;

        //if ((Mathf.FloorToInt(GameManager.gameTime / (travelDurartion+ idleDuration)) & 1) == positionState)
        //{
        //    HaltServerRpc();
        //    positionState = (byte)Mathf.Abs(positionState - 1);
        //    isIdleTimeTicking = 1;
        //    idleTime += isIdleTimeTicking;
        //}
        //else if ((Mathf.FloorToInt(idleTime / idleDuration) & 1) == 1)
        //{
        //    isIdleTimeTicking = 0;
        //    idleTime = 0;
        //    MoveServerRpc(mouvState);
        //    mouvState = (byte)Mathf.Abs(mouvState - 1);
        //}
    }

    [ServerRpc]
    private void HaltServerRpc()
    {
        body.linearVelocity = Vector2.zero;
        body.position = positionArray[positionState];
    }
    [ServerRpc]
    private void MoveServerRpc(byte state)
    {
        body.linearVelocity = -travelDelta * (state-0.5f)*2;
    }

}
