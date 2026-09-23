using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;


public struct rigidbodyState : INetworkSerializable
{
    public ushort rigidbodyNetId;
    public BodyStateFFI phyState;

    public void NetworkSerialize<T>(BufferSerializer<T> s)
        where T : IReaderWriter
    {
        s.SerializeValue(ref rigidbodyNetId);
        s.SerializeValue(ref phyState);
    }
}

public struct PlayerInitialisationData : INetworkSerializable
{
    public Color playerColor;
    public CollisionLayer playerTeam;

    public void NetworkSerialize<T>(BufferSerializer<T> s)
     where T : IReaderWriter
    {
        s.SerializeValue(ref playerColor);
        s.SerializeValue(ref playerTeam);
    }
}


public class ServerManagerNet : NetworkBehaviour
{
    static public uint tick;

    ////[HideInInspector]
    ////public List<PlayerNet> Players;
    [HideInInspector]
    public List<PlayerInitialisationData> playersInitialisationData = new List<PlayerInitialisationData>();

    private uint latestServerStatePayloadTick = 0;
    private uint syncedTick = 0;

    private bool pendingServerData;

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        gameManager.gameObject.SetActive(true);
        GameSyncManager.bulletPool = FindAnyObjectByType<BulletPool>(FindObjectsInactive.Include);
    }

    /// RE IMPLEMENT PASSING BODIES STATES IN THE FUTURE ?
    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendLatestDataPayloadsClientRpc(uint tick, byte rigidbodyNumber, rigidbodyState[] rigidbodyStates, StatePayload[] playersStatePayloads, ClientRpcParams rpcParams)
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

        for (int i = 0; i < GameSyncManager.Players.Count; i++)
        {
            GameSyncManager.Players[i].latestServerStatePayload = playersStatePayloads[i];
            //Debug.Log("receive tick start : " +playersStatePayloads[i].tick);
        }
    }

    /// OPTI GC
    //[ClientRpc(Delivery = RpcDelivery.Unreliable)]
    //public void SendLatestDataPayloadsClientRpc(uint tick, InputPayload[] p1inputPayloads, InputPayload[] p2inputPayloads, InputPayload[] p3inputPayloads, InputPayload[] p4inputPayloads)
    //{
    /// Discard outdated ServerStatePayloads
    //if (tick <= latestServerStatePayloadTick)
    //    return;

    //uint newInputsNum = tick - latestServerStatePayloadTick;

    //latestServerStatePayloadTick = tick;
    //pendingServerData = true;

    /// OPTI
    //if(!Players[0].IsOwner)
    //{
    //    for (int i = 0; i < newInputsNum; i++)
    //    {
    //        Players[0].inputPayloadRBuffer.Write(p1inputPayloads[i]);
    //    }
    //}
    //if (Players.Count == 1) return;
    //if (!Players[1].IsOwner)
    //{
    //    for (int i = 0; i < newInputsNum; i++)
    //    {
    //        Players[1].inputPayloadRBuffer.Write(p2inputPayloads[i]);
    //    }
    //}
    //if (Players.Count == 2) return;
    //if (!Players[2].IsOwner)
    //{
    //    for (int i = 0; i < newInputsNum; i++)
    //    {
    //        Players[2].inputPayloadRBuffer.Write(p3inputPayloads[i]);
    //    }
    //}
    //if (Players.Count == 3) return;
    //if (!Players[3].IsOwner)
    //{
    //    for (int i = 0; i < newInputsNum; i++)
    //    {
    //        Players[3].inputPayloadRBuffer.Write(p4inputPayloads[i]);
    //    }
    //}

    //}




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

    [ClientRpc(Delivery = RpcDelivery.Reliable)]
    public void playerJoinClientRpc(PlayerInitialisationData playerInitialisationData, StatePayload statePayload, ulong newClientID, ClientRpcParams rpcParams)
    {
        NetworkManager.Singleton.ConnectedClients.TryGetValue(newClientID, out var newPlayerNetwork);
        var newPlayer = newPlayerNetwork.PlayerObject.GetComponent<PlayerNet>();
        GameSyncManager.Players.Add(newPlayer);
        newPlayer.playerIcon.color = playerInitialisationData.playerColor;

        RapierWorld.collider_set_memberships(
           RapierWorld.world,
            newPlayer.PlayerBody.gameObject.GetComponent<RapierCircleShape>().colliderHandle,
           (uint)(CollisionLayer.Player | playerInitialisationData.playerTeam)
           );

        newPlayer.team = playerInitialisationData.playerTeam;
        newPlayer.latestServerStatePayload = statePayload;
        newPlayer.latestInputsRecivedTick = statePayload.tick;
        for (int i = 0; i < GameSyncManager.Players.Count; i++)
        {
            if (GameSyncManager.Players[i].IsOwner)
            {
                GameSyncManager.Players[i].GetComponent<Player>().enabled = true;
                GameSyncManager.Players[i].GetComponent<Player>().syncTick(statePayload.tick);
            }
        }
        newPlayer.enabled = true;

    }



    /// OPTI
    //[ClientRpc(Delivery = RpcDelivery.Reliable)]
    //public void SyncPlayersClientRpc(GameInitialisationData initialisationData,StatePayload[] statePayloads, ulong newClientID)
    //{
    //    //Color playerColor = Color.white;
    //    //CollisionLayer team = CollisionLayer.TeamA;
    //    {

    //        int i = 0;
    //        foreach (var item in NetworkManager.Singleton.ConnectedClients.Values)
    //        {
    //            i++;
    //            Players.Add(item.PlayerObject.GetComponent<PlayerNet>());


    //            Players[i].playerIcon.color = initialisationData;

    //            if (Players[i].IsOwner)
    //            {
    //                Players[i].GetComponent<Player>().enabled = true;
    //                Players[i].GetComponent<Player>().syncTick(statePayloads[0].tick);
    //                RapierWorld.collider_set_memberships(
    //                   RapierWorld.world,
    //                   Players[i].PlayerBody.entityHandle,
    //                   (uint)(CollisionLayer.Player | team)
    //                   );

    //                Players[i].GetComponent<PlayerNet>().team = team;
    //                Players[i].GetComponent<PlayerNet>().enabled = true;
    //            }
    //            else
    //            {
    //                Players[i].latestInputsRecivedTick = statePayloads[0].tick;
    //            }
    //            Players[i].latestServerStatePayload = statePayloads[i];





    //            //switch (i)
    //            //{
    //            //    case 1:
    //            //        playerColor = Color.red;
    //            //        break;
    //            //    case 2:
    //            //        playerColor = Color.blue;
    //            //        team = CollisionLayer.TeamB;
    //            //        break;
    //            //    case 3:
    //            //        playerColor = Color.green;
    //            //        team = CollisionLayer.TeamC;
    //            //        break;
    //            //    case 4:
    //            //        playerColor = Color.grey;
    //            //        team = CollisionLayer.TeamD;
    //            //        break;

    //            //}
    //            //item.PlayerObject.GetComponent<PlayerNet>().playerIcon.color = playerColor;
    //        }
    //    }

    //    //for (int i = 0; i < Players.Count; i++)
    //    //{
    //    //    if (!Players[i].IsOwner)
    //    //    {
    //    //        Players[i].latestInputsRecivedTick = statePayloads[0].tick;
    //    //    }
    //    //}
    //    //if (newClientID != NetworkManager.Singleton.LocalClientId)
    //    //{
    //    //    return;
    //    //}
    //    //for (int i = 0; i < Players.Count; i++)
    //    //{
    //    //    if (Players[i].IsOwner)
    //    //    {
    //    //        Players[i].GetComponent<Player>().enabled = true;
    //    //        Players[i].GetComponent<Player>().syncTick(statePayloads[0].tick);
    //    //        RapierWorld.collider_set_memberships(
    //    //           RapierWorld.world,
    //    //           Players[i].PlayerBody.entityHandle,
    //    //           (uint)(CollisionLayer.Player | team)
    //    //           );

    //    //        Players[i].GetComponent<PlayerNet>().team = team;
    //    //        Players[i].GetComponent<PlayerNet>().enabled = true;
    //    //    }
    //    //    else
    //    //    {
    //    //        Players[i].latestInputsRecivedTick = statePayloads[0].tick;
    //    //    }



    //    //    Players[i].latestServerStatePayload = statePayloads[i];
    //    }
    //}

    /// <summary>
    /// To avoid rolling back to a tick where some phy object didn't exist yet
    /// </summary>
    public void PromoteTickAsSynced()
    {
        ////RapierWorld.world_store_snapshot(RapierWorld.world);
        ////RapierToUnityDatabase.ROLLBACKsave();
        GameSyncManager.GameSyncSave();

        latestServerStatePayloadTick = tick;
        foreach (PlayerNet player in GameSyncManager.Players)
        {
            player.latestSyncedMechanicsStatePayload = player.mechanicalState;
        }
    }

    //[ClientRpc(Delivery = RpcDelivery.Unreliable)]
    //public void SendClientsInputsClientRpc(InputPayload[] inputPayloads)
    //{
    //    for (int i = 0; i < Players.Count; i++)
    //    {
    //        /// discard outdated inputpayloads
    //        if (Players[i].latestServerInputPayload.tick > inputPayloads[i].tick)
    //        {
    //            return;
    //        }
    //        Players[i].latestServerInputPayload = inputPayloads[i];
    //    }
    //}


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

        uint latestCommonInputTick = uint.MaxValue;
        foreach (PlayerNet player in GameSyncManager.Players)
        {
            latestCommonInputTick = latestCommonInputTick > player.latestInputsRecivedTick ? player.latestInputsRecivedTick : latestCommonInputTick;
        }
        if (latestCommonInputTick < latestServerStatePayloadTick)
        {
            return;
        }



        /// 1 tick value for all payload ?
        //short rollbackTicksTillSync = (short)(syncedTick - latestServerStatePayloadTick);




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

        //Debug.Log(rollbackTicksTillSync);
        //Debug.Log(rollbackTicksForProjection);

        if (GameSyncManager.Players.Count == 0)
            Debug.Log("eeeeeeeeeeeee");

        ////bool needDatabaseRollback = !RapierToUnityDatabase.IsDatabaseSynced();

        /// WORLD STATE AT LAST SYNC
        {
            GameSyncManager.GameSyncRestore();

            ////RapierWorld.world_restore_snapshot(RapierWorld.world);
            ////if (needDatabaseRollback) RapierToUnityDatabase.ROLLBACKrestore();
            ////foreach (PlayerNet player in Players)
            ////{
            ////    player.RestoreState(player.latestSyncedMechanicsStatePayload);
            ////}
            ////GameSyncManager.bulletPool.RestoreState();
            
            ///Debug.Log("PICKED BACK AT " + syncedTick);
        }
        /// syncing
        {
            short RollbackTicksTillSync = (short)(syncedTick - latestServerStatePayloadTick);
            while (syncedTick <= latestServerStatePayloadTick)
            {
                foreach (PlayerNet player in GameSyncManager.Players)
                {
                    short relativeRollbackTicksTillSync = (short)(RollbackTicksTillSync - (player.latestInputsRecivedTick - latestServerStatePayloadTick));
                    player.Tick(relativeRollbackTicksTillSync);
                    ///Debug.Log(player.inputPayloadRBuffer.Read(relativeRollbackTicksTillSync).direction);
                }
                RapierWorld.PhysicsStep(PlayerNet.gameFixedDeltaTime);
                RollbackTicksTillSync++;
                syncedTick++;
            }
        }


        //foreach (PlayerNet player in Players)
        //{
        //    var bodyState = RapierWorld.body_get_state(RapierWorld.world,player.PlayerBody.GetComponent<RapierBody>().entityHandle);
        //    player.transform.position = new Vector2(bodyState.x,bodyState.y);
        //    player.transform.rotation = quaternion.RotateZ(bodyState.rotation);
        //}
        /// NEW LATEST WORLD SYNC
        foreach (PlayerNet player in GameSyncManager.Players)
        {
            /// Debuging restore and condition, its supposed to be the same
            player.RestoreState(player.latestServerStatePayload);
            if (player.latestServerStatePayload.playerMechanicsState.ticksTillPistonPushActivation != player.mechanicalState.ticksTillPistonPushActivation) Debug.LogError("ccccccccc");
            ////player.UpdateSyncedStates();
        }
        GameSyncManager.GameSyncSave();
        ////ServerManagerNet.bulletPool.SaveState();
        ////RapierWorld.world_store_snapshot(RapierWorld.world);
        ////if (needDatabaseRollback) RapierToUnityDatabase.ROLLBACKsave();
        ///Debug.Log("HALTED AT " + latestServerStatePayloadTick);
        syncedTick = ++latestServerStatePayloadTick;

        short rollbackTicksForProjection = (short)(latestServerStatePayloadTick - tick);
        ///rollbackTicksForProjection++;

        /// projection
        while (rollbackTicksForProjection < 0)
        {
            foreach (PlayerNet player in GameSyncManager.Players)
            {
                short relativeRollbackTicksForProjection = (short)Mathf.Min((rollbackTicksForProjection + (tick - player.latestInputsRecivedTick)),0);
                //if (temp != (uint)(tick + rollbackTicksForProjection) - 1) Debug.Log(temp);
                //temp = (uint)(tick + rollbackTicksForProjection);
                player.Tick(relativeRollbackTicksForProjection);
            }
            RapierWorld.PhysicsStep(PlayerNet.gameFixedDeltaTime);
            rollbackTicksForProjection++;
        }

        pendingServerData = false;
        //latestServerStatePayloadTick = (tick-1);


    }

}
