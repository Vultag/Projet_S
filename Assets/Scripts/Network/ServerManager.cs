
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private uint tick = 0;
    //[HideInInspector]
    //public List<ServerPlayer> serverPlayers = new List<ServerPlayer>();
    [HideInInspector]
    public List<PlayerNet> playerNets = new List<PlayerNet>();
    private List<ulong> targetClientIds = new();

   
    void FixedUpdate()
    {

        uint latestCommonPayloadTick = 67676767;

        if (playerNets.Count < 1)
        {
            Debug.LogError(playerNets.Count);
            return;
        }

        /// FIGURE MAXIMUN COMMON TICK TO CATCH UP + SEND INPUTS TO ALL CLIENTS
        foreach (PlayerNet playerNet in playerNets)
        {
            var payload = playerNet.inputPayloadRBuffer.Read(0);

            latestCommonPayloadTick = latestCommonPayloadTick < payload.tick ? latestCommonPayloadTick : payload.tick;

            { /// PRECOMPUTE AFTER ALL PLAYER JOINED
                targetClientIds.Clear();
                foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (clientId != playerNet.OwnerClientId)
                        targetClientIds.Add(clientId);
                }
                // Send ClientRpc only to those clients
                playerNet.SendClientsInputsClientRpc(
                payload,
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = targetClientIds
                    }
                }
                );
            }
        }

        /// PROCESS AS MUCH INPUT AS POSSIBLE TO OUTPUT THE PHYSICSTATES
        while (tick < latestCommonPayloadTick)
        {
            tick++;
            foreach (PlayerNet playerNet in playerNets)
            {
                playerNet.Tick(tick);
            }

            Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);
        }

        /// send frequency depending on network quality ?
        /// send unreliable ?
        foreach (PlayerNet playerNet in playerNets)
        {
            playerNet.SendLatestStatePayloadClientRpc(new StatePayload
            {
                tick = tick,
                playerPhyState = new PhysicsState
                {
                    position = playerNet.PlayerBody.position,
                    rotation = playerNet.PlayerBody.rotation,
                    linearVelocity = playerNet.PlayerBody.linearVelocity,
                    angularVelocity = playerNet.PlayerBody.angularVelocity,
                },
                pistonPhyState = new PhysicsState
                {
                    position = playerNet.PistonBody.position,
                    rotation = playerNet.PistonBody.rotation,
                    linearVelocity = playerNet.PistonBody.linearVelocity,
                    angularVelocity = playerNet.PistonBody.angularVelocity,
                },
                cogPhyState = new PhysicsState
                {
                    position = playerNet.CogBody.position,
                    rotation = playerNet.CogBody.rotation,
                    linearVelocity = playerNet.CogBody.linearVelocity,
                    angularVelocity = playerNet.CogBody.angularVelocity,
                },
                activeRevertCooldown = playerNet.activeRevertCooldown,
                revertCooldown = playerNet.revertCooldown,
                pistonPushOrPull = playerNet.pistonPushOrPull,
                ticksTillPistonPushActivation = playerNet.ticksTillPistonPushActivation,
                pistonAngle = playerNet.Pistonjoint.angle
            });
        }
        ///reconciliation here ?


    }
    //private void FixedUpdate()
    //{
  
    //}
}
