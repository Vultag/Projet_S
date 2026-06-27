using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public struct rigidbodyState : INetworkSerializable
{
    public ushort rigidbodyNetId;
    public PhysicsState phyState;

    public void NetworkSerialize<T>(BufferSerializer<T> s)
        where T : IReaderWriter
    {
        s.SerializeValue(ref rigidbodyNetId);
        s.SerializeValue(ref phyState);
    }
}


public class ServerManagerNet : NetworkBehaviour
{
    static public uint tick;

    [HideInInspector]
    public List<PlayerNet> Players;
    private uint latestServerStatePayloadTick = 0;

    private bool pendingServerData;

    private GameManager gameManager;

    uint temp;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        gameManager.gameObject.SetActive(true);
    }


    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendLatestDataPayloadsClientRpc(uint tick,byte rigidbodyNumber,rigidbodyState[] rigidbodyStates, StatePayload[] playersStatePayloads, ClientRpcParams rpcParams)
    {
        /// Discard outdated ServerStatePayloads
        if (tick <= latestServerStatePayloadTick)
            return;

        //if (tick == latestServerStatePayloadTick + 1)
        //    Debug.Log("sdqsd");

        //if (pendingServerData)
        //    Debug.Log("remplace");

        latestServerStatePayloadTick = tick;
        pendingServerData = true;

        //Debug.Log(rigidbodyNumber);

        for (int i = 0; i < rigidbodyNumber; i++)
        {
            Debug.Log("rezrzerzea;fmlgqoq,nolnikqogni");
            /// OPIT : over all spawned objects -> PB?
            //var rigidbody = NetworkManager.Singleton.SpawnManager.SpawnedObjects[rigidbodyStates[i].rigidbodyNetId].GetComponent<Rigidbody2D>();
            //rigidbody.position = rigidbodyStates[i].phyState.position;
            //rigidbody.rotation = rigidbodyStates[i].phyState.rotation;
            //rigidbody.linearVelocity = rigidbodyStates[i].phyState.linearVelocity;
            //rigidbody.angularVelocity = rigidbodyStates[i].phyState.angularVelocity;
        }

        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].latestServerStatePayload = playersStatePayloads[i];
            //Debug.Log("receive tick start : " +playersStatePayloads[i].tick);
        }
    }

    //[ClientRpc(Delivery = RpcDelivery.Reliable)]
    //public void InitializePlayerClientRpc(StatePayload[] statePayloads, ClientRpcParams rpcParams)
    //{
    //    /// OPTI
    //    for (int i = 0; i < Players.Count; i++)
    //    {
    //        if (Players[i].IsOwner)
    //        {
    //            Players[i].GetComponent<Player>().enabled = true;
    //            Players[i].GetComponent<Player>().syncTick(statePayloads[0].tick);
    //        }
    //        Players[i].latestServerStatePayload = statePayloads[i];
    //    }
    //}

    /// OPTI
    [ClientRpc(Delivery = RpcDelivery.Reliable)]
    public void SyncPlayersClientRpc(StatePayload[] statePayloads, ulong newClientID)
    {
        Players.Clear();

        Color playerColor = Color.white;
        {
            int i = 0;
            foreach (var item in NetworkManager.Singleton.ConnectedClients.Values)
            {
                i++;
                Players.Add(item.PlayerObject.GetComponent<PlayerNet>());
                switch (i)
                {
                    case 1:
                        playerColor = Color.red;
                        break;
                    case 2:
                        playerColor = Color.blue;
                        break;
                    case 3:
                        playerColor = Color.green;
                        break;
                    case 4:
                        playerColor = Color.grey;
                        break;

                }
                item.PlayerObject.GetComponent<PlayerNet>().playerIcon.color = playerColor;
            }
        }

        if (newClientID != NetworkManager.Singleton.LocalClientId)
            return;
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].IsOwner)
            {
                Players[i].GetComponent<Player>().enabled = true;
                Players[i].GetComponent<Player>().syncTick(statePayloads[0].tick);
            }
            Players[i].latestServerStatePayload = statePayloads[i];
        }
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendClientsInputsClientRpc(InputPayload[] inputPayloads)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            /// discard outdated inputpayloads
            if (Players[i].latestServerInputPayload.tick > inputPayloads[i].tick)
            {
                return;
            }
            Players[i].latestServerInputPayload = inputPayloads[i];
        }
    }


    //public bool ShouldReconcile(short rollbackTicks)
    //{
    //    /// SHOULD RECONCILE
    //    foreach (PlayerNet playerNet in Players)
    //    {
    //        var dist = Vector2.Distance(playerNet.latestServerStatePayload.playerPhyState.position, playerNet.statePayloadRBuffer.Read((short)(rollbackTicks)).playerPhyState.position);

    //        if ((Vector2.Distance(playerNet.latestServerStatePayload.playerPhyState.position, playerNet.statePayloadRBuffer.Read((short)(rollbackTicks)).playerPhyState.position) > 0.05f))
    //            return true;
    //    }
    //    return false;

    //    //foreach (PlayerNet playerNet in Players)
    //    //{
    //    //    if (Mathf.Abs(playerNet.latestServerStatePayload.playerPhyState.rotation - playerNet.statePayloadRBuffer.Read((short)(rollbackTicks)).playerPhyState.rotation) > 0.05f)
    //    //        return true;
    //    //}
    //    //return false;
    //}

    public void Reconciliation()
    {



        uint newServerStatePayloadTick = Players[0].latestServerStatePayload.tick;

        /// 1 tick value for all payload ?
        short rollbackTicks = (short)(newServerStatePayloadTick - tick);

        var statePayloadRBuffer = Players[0].statePayloadRBuffer.Read((short)(rollbackTicks+1));
        
        ///debug
        /*
        if (newServerStatePayloadTick != statePayloadRBuffer.tick)
        {
            Debug.Log("TICK  " + newServerStatePayloadTick + "   " + statePayloadRBuffer.tick);
        }
        if (statePayloadRBuffer.activeRevertCooldown != Players[0].latestServerStatePayload.activeRevertCooldown)
            Debug.Log("activeRevertCooldown " + statePayloadRBuffer.activeRevertCooldown + "   " + Players[0].latestServerStatePayload.activeRevertCooldown);

        if (statePayloadRBuffer.revertCooldown != Players[0].latestServerStatePayload.revertCooldown)
            Debug.Log("revertCooldown " + statePayloadRBuffer.revertCooldown + "   " + Players[0].latestServerStatePayload.revertCooldown);

        if (statePayloadRBuffer.ticksTillPistonPushActivation != Players[0].latestServerStatePayload.ticksTillPistonPushActivation)
            Debug.Log("ticksTillPistonPushActivation " + statePayloadRBuffer.ticksTillPistonPushActivation + "   " + Players[0].latestServerStatePayload.ticksTillPistonPushActivation);

        if (statePayloadRBuffer.pistonAngle != Players[0].latestServerStatePayload.pistonAngle)
            Debug.Log("pistonAngle " + statePayloadRBuffer.pistonAngle + "   " + Players[0].latestServerStatePayload.pistonAngle);

        if (statePayloadRBuffer.pistonPushArmed != Players[0].latestServerStatePayload.pistonPushArmed)
            Debug.Log("pistonPushArmed " + statePayloadRBuffer.pistonPushArmed + "   " + Players[0].latestServerStatePayload.pistonPushArmed);
        if (statePayloadRBuffer.pistonPushOrPull != Players[0].latestServerStatePayload.pistonPushOrPull)
            Debug.Log("pistonPushOrPull " + statePayloadRBuffer.pistonPushArmed + "   " + Players[0].latestServerStatePayload.pistonPushOrPull);
        */

        /// Discard outdated ServerStatePayloads
        if (!pendingServerData)
        {
            return;
        }


        if (rollbackTicks >= 0)
            Debug.Log("000");

        if (newServerStatePayloadTick >= tick)
            Debug.Log("jjjjjjjjjjjj   " + newServerStatePayloadTick + "   " + tick + "   " + rollbackTicks + "   " + temp);

        temp = (uint)(tick + rollbackTicks);

        //else
        //    Debug.Log("recon");

        ///DEBUG
        foreach (PlayerNet playerNet in Players)
        {
            if (Mathf.Abs(rollbackTicks) > PlayerNet.PayloadRBufferSize) Debug.Log("capacity exeeded : " + (short)(newServerStatePayloadTick - tick));
        }

        //if(ShouldReconcile(rollbackTicks)) Debug.Log("recon");
        //if (!ShouldReconcile(rollbackTicks))
        //return;

        //Debug.Log("recon");

        if (Players.Count == 0)
            Debug.Log("eeeeeeeeeeeee");


        foreach (PlayerNet player in Players)
        {
            player.SynchronizeState();
        }



        while (rollbackTicks < -1)
        {
            rollbackTicks++;
            gameManager.Tick((uint)(tick + rollbackTicks));


            foreach (PlayerNet player in Players)
            {

                player.Tick(rollbackTicks);
            }
            Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);

            //var dist = Vector2.Distance(Players[0].PlayerBody.position, Players[0].statePayloadRBuffer.Read(rollbackTicks).playerPhyState.position);
            //if (dist > 0.5f)
            //{
            //    Debug.Log("rollback bug " + ((tick-newServerStatePayloadTick)+rollbackTicks) +" ticks into rollback with " + dist);
            //    Debug.Log(Players[0].ticksTillPistonPushActivation);
            //    Debug.Log(Players[0].activeRevertCooldown);
            //    Debug.Log(Players[0].revertCooldown);
            //    Debug.Log(Players[0].pistonPushOrPull);
            //    Debug.Log(Players[0].pistonPushArmed);
            //}

        }

        pendingServerData = false;
        latestServerStatePayloadTick = (tick-1);

    }

}
