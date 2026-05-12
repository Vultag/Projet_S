using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerPlayer : MonoBehaviour
{


    /// <summary>
    /// REMPLACE WITH SERVERMANAGER ALL TOGETHER ?
    /// </summary>



    public Rigidbody2D PlayerBody;


    private PlayerNet playerNet;

    private StatePayload latestStatePayload;

    private List<ulong> targetClientIds = new();

    void Awake()
    {

    }
    void Start()
    {

        playerNet = GetComponent<PlayerNet>();

    }

    //public void Tick(uint tick)
    //{
    //    var latestPayloadTick = playerNet.inputPayloadRBuffer.Read(0).tick;
    //    InputPayload payload = playerNet.inputPayloadRBuffer.Read((short)(tick - latestPayloadTick));

    //    if (playerNet.revertCooldown == 0)
    //    {
    //        playerNet.PistonPull();
    //    }
    //    playerNet.revertCooldown = (byte)(playerNet.revertCooldown - playerNet.activeRevertCooldown);

    //    if (payload.ticksTillPistonPushActivation == 0)
    //    {
    //        playerNet.PistonPush(payload.pistonDirection);
    //    }

    //    PlayerBody.AddTorque((3000 * -payload.direction) - (PlayerBody.angularVelocity * 2f * Mathf.Abs(payload.direction)), ForceMode2D.Force);

    //    //Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);

    //    //if (payload.direction != 0)
    //    //    Debug.Log("move2");

    //    //tick++;
    //}


    private void Update()
    {


        //var latestPayloadTick = playerNet.inputPayloadRBuffer.Read(0).tick;

        //if (latestPayload.direction != 0)
        //    Debug.Log("rrrr");

        //InputPayload payload = playerNet.inputPayloadRBuffer.Read((short)(tick - latestPayloadTick));

        //while (tick < latestPayloadTick)
        //{
        //    if (playerNet.revertCooldown == 0)
        //    {
        //        playerNet.PistonPull();
        //    }
        //    playerNet.revertCooldown = (byte)(playerNet.revertCooldown - playerNet.activeRevertCooldown);
       
        //    if (payload.ticksTillPistonPushActivation == 0)
        //    {
        //        playerNet.PistonPush(payload.pistonDirection);
        //    }

        //    PlayerBody.AddTorque((3000 * -payload.direction) - (PlayerBody.angularVelocity * 2f * Mathf.Abs(payload.direction)), ForceMode2D.Force);

        //    Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);

        //    //if (payload.direction != 0)
        //    //    Debug.Log("move2");

        //    tick++;

        //    payload = playerNet.inputPayloadRBuffer.Read((short)(tick - latestPayloadTick));

        //}

        //latestStatePayload = new StatePayload
        //{
        //    tick = tick,
        //    playerPhyState = new PhysicsState
        //    {
        //        position = PlayerBody.position,
        //        rotation = PlayerBody.rotation,
        //        linearVelocity = PlayerBody.linearVelocity,
        //        angularVelocity = PlayerBody.angularVelocity,
        //    },
        //    pistonPhyState = new PhysicsState
        //    {
        //        position = playerNet.PistonBody.position,
        //        rotation = playerNet.PistonBody.rotation,
        //        linearVelocity = playerNet.PistonBody.linearVelocity,
        //        angularVelocity = playerNet.PistonBody.angularVelocity,
        //    },
        //    cogPhyState = new PhysicsState
        //    {
        //        position = playerNet.CogBody.position,
        //        rotation = playerNet.CogBody.rotation,
        //        linearVelocity = playerNet.CogBody.linearVelocity,
        //        angularVelocity = playerNet.CogBody.angularVelocity,
        //    },
        //    activeRevertCooldown = playerNet.activeRevertCooldown,
        //    revertCooldown = playerNet.revertCooldown
        //};


        //{ /// PRECOMPUTE AFTER ALL PLAYER JOINED
        //    targetClientIds.Clear();
        //    foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        //    {
        //        if (clientId != playerNet.OwnerClientId)
        //            targetClientIds.Add(clientId);
        //    }
        //    // Send ClientRpc only to those clients
        //    playerNet.SendClientsInputsClientRpc(
        //    payload,
        //    new ClientRpcParams
        //    {
        //        Send = new ClientRpcSendParams
        //        {
        //            TargetClientIds = targetClientIds
        //        }
        //    }
        //    );
        //}
        //playerNet.SendClientsInputsClientRpc(payload);

    }

    //private void FixedUpdate()
    //{
    //    /// send frequency depending on network quality ?
    //    /// send unreliable ?
    //    playerNet.SendLatestStatePayloadClientRpc(latestStatePayload);
    //    ///reconciliation here ?
    //}
}
