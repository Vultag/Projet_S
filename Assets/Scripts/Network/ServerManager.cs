
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRigidbodyStates
{
    public byte playerRigidbodyStatesCount;
    public rigidbodyState[] playerRigidbodyStates = new rigidbodyState[16];
}

public class ServerManager : MonoBehaviour
{
    /// WRONG PLACE ?
    //[HideInInspector]
    //public uint tick = 0;
    [HideInInspector]
    public List<ulong> targetClientIds = new();

    [HideInInspector]
    public StatePayload[] statePayloads = new StatePayload[4];

    [HideInInspector]
    public PlayerRigidbodyStates player1RigidbodyStates;
    [HideInInspector]
    public PlayerRigidbodyStates player2RigidbodyStates;
    [HideInInspector]
    public PlayerRigidbodyStates player3RigidbodyStates;
    [HideInInspector]
    public PlayerRigidbodyStates player4RigidbodyStates;

    private ServerManagerNet serverManagerNet;

    private uint maximumTickGap = 60;

    private GameManager gameManager;
    private ServerDataDispatcher dataDispatcher;

    private uint temp;
    private uint tempAA;

    private void Awake()
    {
        serverManagerNet = this.GetComponent<ServerManagerNet>();
        player1RigidbodyStates = new();
        player2RigidbodyStates = new();
        player3RigidbodyStates = new();
        player4RigidbodyStates = new();
        dataDispatcher = this.AddComponent<ServerDataDispatcher>();
        gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        for (int i = 0; i < statePayloads.Length; i++)
        {
            statePayloads[i] = new StatePayload
            {
                tick = 0,
                playerMechanicsState = MechanicsState.Default(),
            };
        }
    }


    void FixedUpdate()
    {
        uint oldestCommonPayloadTick = 67676767;
        uint earlyestPayloadTick = 0;

        if (GameSyncManager.Players.Count < 1)
        {
            Debug.LogError(GameSyncManager.Players.Count);
            return;
        }

        for (int i = 0; i < GameSyncManager.Players.Count; i++)
        {
            var payload = GameSyncManager.Players[i].inputPayloadRBuffer.Read(0);
            oldestCommonPayloadTick = oldestCommonPayloadTick > payload.tick ? payload.tick : oldestCommonPayloadTick;
            earlyestPayloadTick = payload.tick > earlyestPayloadTick ? payload.tick : earlyestPayloadTick;
        }

        /* 
         * prevent simulation being stalled by a user with bad connection
         * -> limit the number of ticks user are allowed to trail behind
         * -> cut to newer tick at the cost of the user's old inputs drop
         */
        /// -> ADVANCE THE SIMULTATION OF THE TRAILLING PLAYER TO NOT INDER ON OTHERS ?
        {
            if ((earlyestPayloadTick - oldestCommonPayloadTick) > maximumTickGap)
            {
                Debug.Log("qsdqsdaara");
                oldestCommonPayloadTick = earlyestPayloadTick - maximumTickGap;
            }
            for (int i = 0; i < GameSyncManager.Players.Count; i++)
            {
                var payload = GameSyncManager.Players[i].inputPayloadRBuffer;
                if (payload.Read(0).tick <= (short)(earlyestPayloadTick - maximumTickGap))
                {
                    Debug.Log("Inputs dropped at PLAYER " + i);
                    var newInputPayloadHead = new InputPayload { tick = payload.Read(0).tick };
                    payload.SlideHead(-1);
                    payload.Write(newInputPayloadHead);
                }
            }
        }

        ///serverManagerNet.SendClientsInputsClientRpc(inputPayloads);

        if (ServerManagerNet.tick >= oldestCommonPayloadTick)
        {
            return;
        }

        short relativeTick;
        /// PROCESS AS MUCH INPUT AS POSSIBLE TO OUTPUT THE PHYSICSTATES
        while (ServerManagerNet.tick < oldestCommonPayloadTick)
        {
            ServerManagerNet.tick++;
            gameManager.Tick(ServerManagerNet.tick);

            foreach (PlayerNet playerNet in GameSyncManager.Players)
            {

                short leadingPayloadTickDiff = (short)(oldestCommonPayloadTick - playerNet.inputPayloadRBuffer.Read(0).tick);
                relativeTick = (short)((ServerManagerNet.tick - oldestCommonPayloadTick)+ leadingPayloadTickDiff);
                playerNet.Tick(relativeTick);

                //if (playerNet.inputPayloadRBuffer.Read(relativeTick).pistonPush) Debug.Log("arm at " + (ServerManagerNet.tick));
            }

            RapierWorld.PhysicsStep(PlayerNet.gameFixedDeltaTime);
        }

        dataDispatcher.shouldDispatch = true;

    }

    public void playerJoin(PlayerNet newPlayerNet)
    {
        /// Join rpc to new client for every other connected player
        var newClientTarget = new ClientRpcParams{ Send = new ClientRpcSendParams { TargetClientIds = new[] { newPlayerNet.OwnerClientId } } };
        for (int i = 0; i < targetClientIds.Count; i++)
        {
            serverManagerNet.playerJoinClientRpc(
                serverManagerNet.playersInitialisationData[i],
                statePayloads[i],
                targetClientIds[i],
                newClientTarget
                );
        }
        targetClientIds.Add(newPlayerNet.OwnerClientId);
        var newPlyerIdx = targetClientIds.Count-1;

        newPlayerNet.inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);
        newPlayerNet.inputPayloadRBuffer.SlideHead(-1);
        newPlayerNet.inputPayloadRBuffer.Write(new InputPayload { tick = ServerManagerNet.tick});
        newPlayerNet.latestInputsRecivedTick = ServerManagerNet.tick;

        switch (GameSyncManager.Players.Count)
        {
            case 0:
                serverManagerNet.playersInitialisationData.Add(new PlayerInitialisationData
                {
                    playerColor = Color.red,
                    playerTeam = CollisionLayer.TeamA
                });
                newPlayerNet.playerIcon.color = Color.red;
                newPlayerNet.team = CollisionLayer.TeamA;
                break;
            case 1:
                serverManagerNet.playersInitialisationData.Add(new PlayerInitialisationData
                {
                    playerColor = Color.blue,
                    playerTeam = CollisionLayer.TeamB
                });
                newPlayerNet.playerIcon.color = Color.blue;
                newPlayerNet.team = CollisionLayer.TeamB;
                break;
            case 2:
                serverManagerNet.playersInitialisationData.Add(new PlayerInitialisationData
                {
                    playerColor = Color.green,
                    playerTeam = CollisionLayer.TeamC
                });
                newPlayerNet.playerIcon.color = Color.green;
                newPlayerNet.team = CollisionLayer.TeamC;
                break;
            case 3:
                serverManagerNet.playersInitialisationData.Add(new PlayerInitialisationData
                {
                    playerColor = Color.grey,
                    playerTeam = CollisionLayer.TeamD
                });
                newPlayerNet.playerIcon.color = Color.grey;
                newPlayerNet.team = CollisionLayer.TeamD;
                break;
            default:
                Debug.LogError("4+ players not implemented");
                break;

        }

        RapierWorld.collider_set_memberships(
          RapierWorld.world,
          newPlayerNet.PlayerBody.gameObject.GetComponent<RapierCircleShape>().colliderHandle,
          (uint)(CollisionLayer.Player | newPlayerNet.team)
          );

        newPlayerNet.enabled = true;

        serverManagerNet.playerJoinClientRpc(
            serverManagerNet.playersInitialisationData[newPlyerIdx],
            new StatePayload {
                tick = ServerManagerNet.tick ,
                playerMechanicsState = MechanicsState.Default(),
            },
            newPlayerNet.OwnerClientId, 
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = targetClientIds } }
            );

    }
}
