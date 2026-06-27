
using System;
using System.Collections.Generic;
using Unity.Netcode;
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
    public InputPayload[] inputPayloads = new InputPayload[4];

    [HideInInspector]
    public PlayerRigidbodyStates player1RigidbodyStates;
    [HideInInspector]
    public PlayerRigidbodyStates player2RigidbodyStates;
    [HideInInspector]
    public PlayerRigidbodyStates player3RigidbodyStates;
    [HideInInspector]
    public PlayerRigidbodyStates player4RigidbodyStates;

    private ServerManagerNet serverManagerNet;

    private uint maximumTickGap = 240;

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
                Debug.Log("qsdqsdaara");
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

            foreach (PlayerNet playerNet in playerNets)
            {

                short leadingPayloadTickDiff = (short)(oldestCommonPayloadTick - playerNet.inputPayloadRBuffer.Read(0).tick);

                var arf = (short)(ServerManagerNet.tick + leadingPayloadTickDiff);

                if (leadingPayloadTickDiff > 0) Debug.Log("ezr456   " + leadingPayloadTickDiff + "   " + oldestCommonPayloadTick + "   " + earlyestPayloadTick);
                if (leadingPayloadTickDiff != 0) Debug.Log(leadingPayloadTickDiff);

                relativeTick = (short)((ServerManagerNet.tick - oldestCommonPayloadTick)+ leadingPayloadTickDiff);
                if (relativeTick > 0) Debug.Log("5465146");
                playerNet.Tick(relativeTick);

               // if (playerNet.inputPayloadRBuffer.Read(relativeTick).pistonPush) Debug.Log("jump at " + (ServerManagerNet.tick));
            }

            Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);
        }

        dataDispatcher.shouldDispatch = true;

    }

    public void playerJoin(int playerCount,PlayerNet newPlayerNet)
    {
        targetClientIds.Add(newPlayerNet.OwnerClientId);
        newPlayerNet.inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);
        newPlayerNet.inputPayloadRBuffer.SlideHead(-1);
        newPlayerNet.inputPayloadRBuffer.Write(InputPayload.Default(ServerManagerNet.tick));
        //Debug.Log(ServerManagerNet.tick);

        switch (playerCount)
        {
            case 1:
                newPlayerNet.gameObject.GetComponentInChildren<ObjectsStatesCollector>().playerRigidbodyStates = player1RigidbodyStates;
                break;
            case 2:
                newPlayerNet.gameObject.GetComponentInChildren<ObjectsStatesCollector>().playerRigidbodyStates = player2RigidbodyStates;
                break;
            case 3:
                newPlayerNet.gameObject.GetComponentInChildren<ObjectsStatesCollector>().playerRigidbodyStates = player3RigidbodyStates;
                break;
            case 4:
                newPlayerNet.gameObject.GetComponentInChildren<ObjectsStatesCollector>().playerRigidbodyStates = player4RigidbodyStates;
                break;
        }

        serverManagerNet.SyncPlayersClientRpc(statePayloads, newPlayerNet.OwnerClientId);

    }
}
