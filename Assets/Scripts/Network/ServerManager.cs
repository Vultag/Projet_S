
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private uint tick = 0;
    private List<ulong> targetClientIds = new();

    private StatePayload[] statePayloads = new StatePayload[4];
    private InputPayload[] inputPayloads = new InputPayload[4];

    private ServerManagerNet serverManagerNet;

    private uint maximumTickGap = 240;

    private void Awake()
    {
        serverManagerNet = this.GetComponent<ServerManagerNet>();
    }


    void FixedUpdate()
    {
        var playerNets = serverManagerNet.Players;

        uint oldestCommonPayloadTick = 67676767;
        uint earlyestPayloadTick = 0;

        if (playerNets.Count < 1)
        {
            Debug.LogError(playerNets.Count);
            return;
        }

        for (int i = 0; i < serverManagerNet.Players.Count; i++)
        {
            var payload = serverManagerNet.Players[i].inputPayloadRBuffer.Read(0);
            oldestCommonPayloadTick = oldestCommonPayloadTick > payload.tick ? payload.tick : oldestCommonPayloadTick;
            earlyestPayloadTick = payload.tick > earlyestPayloadTick ? payload.tick : earlyestPayloadTick;
            inputPayloads[i] = payload;
        }

        /* 
         * prevent simulation being stalled by a user with bad connection
         * -> limit the number of ticks user are allowed to trail behind
         * -> cut to newer tick at the cost of the user's old inputs drop
         */
        {
            if ((earlyestPayloadTick - oldestCommonPayloadTick) > maximumTickGap)
            {
                oldestCommonPayloadTick = earlyestPayloadTick - maximumTickGap;
            }
            for (int i = 0; i < serverManagerNet.Players.Count; i++)
            {
                var payload = serverManagerNet.Players[i].inputPayloadRBuffer;
                if (payload.Read(0).tick <= (short)(earlyestPayloadTick - maximumTickGap))
                {
                    Debug.Log("Inputs dropped at PLAYER " + i);
                    var newÏnputPayloadHead = InputPayload.Default(payload.Read(0).tick);
                    payload.SlideHead(-1);
                    payload.Write(newÏnputPayloadHead);
                }
            }
        }

        serverManagerNet.SendClientsInputsClientRpc(inputPayloads);


        short relativeTick;
        /// PROCESS AS MUCH INPUT AS POSSIBLE TO OUTPUT THE PHYSICSTATES
        while (tick < oldestCommonPayloadTick)
        {
            tick++;

            foreach (PlayerNet playerNet in playerNets)
            {
                relativeTick = (short)Mathf.Min((short)(tick - playerNet.inputPayloadRBuffer.Read(0).tick),0);
                playerNet.Tick(relativeTick);
            }

            Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);
        }

        {
            byte i = 0;
            foreach (PlayerNet playerNet in serverManagerNet.Players)
            {
                statePayloads[i] = new StatePayload
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
                };
                i++;
            }
        }

        serverManagerNet.SendLatestStatePayloadsClientRpc(statePayloads);


        /// send frequency depending on network quality ?
        /// send unreliable ?
        ///reconciliation here ?


    }

    public void playerJoin(PlayerNet newPlayerNet)
    {
        targetClientIds.Add(newPlayerNet.OwnerClientId);
        newPlayerNet.inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);
        newPlayerNet.inputPayloadRBuffer.Write(InputPayload.Default(statePayloads[0].tick));

        serverManagerNet.SyncPlayersClientRpc(statePayloads, newPlayerNet.OwnerClientId);

    }
}
