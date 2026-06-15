using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManagerNet : NetworkBehaviour
{
    [HideInInspector]
    public List<PlayerNet> Players;
    private uint latestServerStatePayloadTick = 0;


    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendLatestStatePayloadsClientRpc(StatePayload[] statePayloads)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].latestServerStatePayload = statePayloads[i];
        }
    }

    [ClientRpc(Delivery = RpcDelivery.Reliable)]
    public void InitializePlayerClientRpc(StatePayload[] statePayloads, ClientRpcParams rpcParams)
    {
        /// OPTI
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


    public bool ShouldReconcile(short rollbackTicks)
    {
        /// SHOULD RECONCILE
        foreach (PlayerNet playerNet in Players)
        {
            var dist = Vector2.Distance(playerNet.latestServerStatePayload.playerPhyState.position, playerNet.statePayloadRBuffer.Read((short)(rollbackTicks + 1)).playerPhyState.position);

            if ((Vector2.Distance(playerNet.latestServerStatePayload.playerPhyState.position, playerNet.statePayloadRBuffer.Read((short)(rollbackTicks + 1)).playerPhyState.position) > 0.5f))
                return true;
        }
        return false;
    }

    public void Reconciliation(uint tick)
    {

        uint newServerStatePayloadTick = Players[0].latestServerStatePayload.tick;

        /// 1 tick value for all payload ?
        short rollbackTicks = (short)((newServerStatePayloadTick - tick));

        /// Discard delayed ServerStatePayloads
        if (rollbackTicks == 0 | newServerStatePayloadTick <= latestServerStatePayloadTick)
        {
            return;
        }

        ///DEBUG
        foreach (PlayerNet playerNet in Players)
        {
            if (Mathf.Abs(rollbackTicks) > PlayerNet.PayloadRBufferSize) Debug.LogError("capacity exeeded : " + (short)(newServerStatePayloadTick - tick));
        }

        if (!ShouldReconcile(rollbackTicks))
            return;

        //Debug.Log("recon");

        foreach (PlayerNet player in Players)
        {
            player.SynchronizeState();
        }

        rollbackTicks++;

        while (rollbackTicks < 0)
        {
            foreach (PlayerNet player in Players)
            {
                player.Tick(rollbackTicks);
            }
            Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);
            rollbackTicks++;
        }

        latestServerStatePayloadTick = tick;

    }

}
