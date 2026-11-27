using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;


public struct rigidbodyState : INetworkSerializable
{
    public ulong rigidbodyNetId;
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

    public GameObject playerPrefab;
    public Map map;

    static public uint tick;

    ////[HideInInspector]
    ////public List<PlayerNet> Players;
    [HideInInspector]
    public List<PlayerInitialisationData> playersInitialisationData = new List<PlayerInitialisationData>();

    private uint latestServerStatePayloadTick = 0;
    private uint syncedTick = 0;

    private bool pendingServerData;

    public List<RapierBody> CyclicSyncObjectsHandle;

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendLatestDataPayloadsClientRpc(uint tick, 
        StatePayload[] playersStatePayloads,
        BodyStateFFI CycleSyncObjectState,
        ushort CycleSyncObjectStateIdx,
        ClientRpcParams rpcParams)
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

        for (int i = 0; i < GameSyncManager.Players.Count; i++)
        {
            GameSyncManager.Players[i].latestServerStatePayload = playersStatePayloads[i];
            //Debug.Log("receive tick start : " +playersStatePayloads[i].tick);
        }
        GameSyncManager.CyclingRigidbodyState = CycleSyncObjectState;
        GameSyncManager.CyclingRigidbodyStateIdx = CycleSyncObjectStateIdx;
        //Debug.Log(CyclicSyncObjectsHandle[0]);
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
        //GameObject newPlayer = Instantiate(PlayerPrefab);
        //Debug.Log("azeaze");

        //NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
        //networkObject.SpawnAsPlayerObject(newClientID);
        //networkObject.NetworkShow(newClientID);

        NetworkManager.Singleton.ConnectedClients.TryGetValue(newClientID, out var newPlayerNetwork);
        var newPlayer = newPlayerNetwork.PlayerObject.GetComponent<PlayerNet>();
        newPlayer.playerIcon.color = playerInitialisationData.playerColor;
        newPlayer.team = playerInitialisationData.playerTeam;
        newPlayer.latestServerStatePayload = statePayload;
        newPlayer.latestInputsRecivedTick = statePayload.tick;
        newPlayer.enabled = true;

        RapierWorld.collider_set_memberships(
           RapierWorld.world,
            newPlayer.PlayerBody.gameObject.GetComponent<RapierCircleShape>().colliderHandle,
           (uint)(CollisionLayer.Player | playerInitialisationData.playerTeam)
           );

        Vector2 startForceDir = Vector2.zero;
        switch (playerInitialisationData.playerTeam)
        {
            case CollisionLayer.TeamA:
                startForceDir = map.TeamAspawner.transform.rotation * Vector2.up;
                break;
            case CollisionLayer.TeamB:
                startForceDir = map.TeamBspawner.transform.rotation * Vector2.up;
                break;
            case CollisionLayer.TeamC:
                break;
            case CollisionLayer.TeamD:
                break;
            default:
                break;
        }

        //RapierWorld.body_add_force(
        //      RapierWorld.world,
        //      newPlayer.PlayerBody.entityHandle,
        //      startForceDir.x * 100,
        //      startForceDir.y * 100,
        //      0
        //  );


        /// Sync stick
        if (newPlayer.IsLocalPlayer)
        {
            ServerManagerNet.tick = statePayload.tick;
            //Debug.Log(newPlayer.IsLocalPlayer);
        }
        Debug.Log("spawn at  " + ServerManagerNet.tick);

        //for (int i = 0; i < GameSyncManager.Players.Count; i++)
        //{
        //    if (GameSyncManager.Players[i].IsOwner)
        //    {
        //        GameSyncManager.Players[i].GetComponent<Player>().enabled = true;
        //        GameSyncManager.Players[i].GetComponent<Player>().syncTick(statePayload.tick);
        //    }
        //}

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
        //Debug.Log("rrrrrr  " + tick);
        //Debug.LogError("ROLLBACKsave");
        GameSyncManager.GameSyncSave();
        latestServerStatePayloadTick = tick;
        syncedTick = tick;
        
    }


    public void Reconciliation()
    {
        if (latestServerStatePayloadTick>tick) Debug.LogError("azeazeazeazeazeazeazeazeazeazeazeazeaze");

        uint latestCommonInputTick = uint.MaxValue;
        foreach (PlayerNet player in GameSyncManager.Players)
        {
            latestCommonInputTick = latestCommonInputTick > player.latestInputsRecivedTick ? player.latestInputsRecivedTick : latestCommonInputTick;
        }
        //if (latestCommonInputTick <= latestServerStatePayloadTick)
        //{
        //    if (latestCommonInputTick > syncedTick)
        //        SimulationJump(latestCommonInputTick);
        //}
        //else
        //{
        //    if(pendingServerData)
        //        CompleteReconciliation();
        //}

        //if (latestCommonInputTick > syncedTick)
        //{
        //    if (latestCommonInputTick <= latestServerStatePayloadTick | !pendingServerData)
        //        SimulationJump(latestCommonInputTick);
        //    else
        //        CompleteReconciliation();
        //}
       
        if (latestServerStatePayloadTick > syncedTick && latestCommonInputTick > latestServerStatePayloadTick)
        {
            CompleteReconciliation();
        }
        else if(latestCommonInputTick > syncedTick)
        {
            //SimulationJump(latestCommonInputTick);
        }


    }
    private void SimulationJump(uint latestCommonInputTick)
    {

        var referenceSyncTick = syncedTick;

        /// WORLD STATE AT LAST SYNC
        {
            GameSyncManager.GameSyncRestore();
        }
        /// syncing
        {
            //GameSyncManager.syncingPhase = true;

            short RollbackTicksTillSync = (short)(referenceSyncTick - latestCommonInputTick);
            short relativeRollbackTicksTillSync = 0;
            while (referenceSyncTick <= latestCommonInputTick)
            {
                foreach (PlayerNet playerNet in GameSyncManager.Players)
                {
                    relativeRollbackTicksTillSync = (short)(RollbackTicksTillSync + (latestCommonInputTick - playerNet.latestInputsRecivedTick));
                    playerNet.Tick(relativeRollbackTicksTillSync);
                    ///Debug.Log(player.inputPayloadRBuffer.Read(relativeRollbackTicksTillSync).direction);
                }
                GameSyncManager.gameManager.Tick((short)(referenceSyncTick - tick));

                RapierWorld.PhysicsStep(PlayerNet.gameFixedDeltaTime);
                RollbackTicksTillSync++;
                referenceSyncTick++;
            }
            //GameSyncManager.syncingPhase = false;
        }

        short rollbackTicksForProjection = (short)(referenceSyncTick - tick);
        if (rollbackTicksForProjection > 0) Debug.Log("wxcwxcwxc " + rollbackTicksForProjection);

        /// projection
        {
            GameSyncManager.DEBUGprojection = true;
            while (rollbackTicksForProjection < 0)
            {
                foreach (PlayerNet playerNet in GameSyncManager.Players)
                {
                    short relativeRollbackTicksForProjection = (short)Mathf.Min((rollbackTicksForProjection + (tick - playerNet.latestInputsRecivedTick)), 0);
                    playerNet.Tick(relativeRollbackTicksForProjection);
                }
                GameSyncManager.gameManager.Tick(rollbackTicksForProjection);

                RapierWorld.PhysicsStep(PlayerNet.gameFixedDeltaTime);
                rollbackTicksForProjection++;
            }
            foreach (PlayerNet playerNet in GameSyncManager.Players)
            {
                playerNet.tickDelay = 0;
            }
            GameSyncManager.DEBUGprojection = false;
        }

    }
    private void CompleteReconciliation()
    {
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

        //Debug.Log("recon " + ServerManagerNet.tick);

        /// WORLD STATE AT LAST SYNC
        {
            GameSyncManager.GameSyncRestore();
        }
        /// syncing
        {
            GameSyncManager.syncingPhase = true;

            while (syncedTick < latestServerStatePayloadTick)
            {
                syncedTick++;
                foreach (PlayerNet playerNet in GameSyncManager.Players)
                {
                    playerNet.Tick((short)(syncedTick - playerNet.latestInputsRecivedTick));
                    ///Debug.Log(player.inputPayloadRBuffer.Read(relativeRollbackTicksTillSync).direction);
                }
                GameSyncManager.gameManager.Tick((short)(syncedTick - tick));

                RapierWorld.PhysicsStep(PlayerNet.gameFixedDeltaTime);
            }
            GameSyncManager.syncingPhase = false;
        }

        /// NEW LATEST WORLD SYNC
        foreach (PlayerNet player in GameSyncManager.Players)
        {
            /// Debuging restore and condition, its supposed to be the same
            /// IT Breakes non synced dynamic objects somehow -> exacts data wrong?
            //var temp = RapierWorld.body_get_state(RapierWorld.world, player.PlayerBody.entityHandle);
            ///////////player.RestoreState(player.latestServerStatePayload);
            //Debug.Log(new Vector2(player.latestServerStatePayload.playerPhyState.x- temp.x, player.latestServerStatePayload.playerPhyState.y-temp.y));

            if (player.latestServerStatePayload.playerMechanicsState.ticksTillPistonPushActivation != player.mechanicalState.ticksTillPistonPushActivation) Debug.LogError("ccccccccc " + player.latestServerStatePayload.playerMechanicsState.ticksTillPistonPushActivation + "   " + player.mechanicalState.ticksTillPistonPushActivation);
        }

        //Debug.Log("ROLLBACKsave");

        RapierWorld.body_set_state(RapierWorld.world, CyclicSyncObjectsHandle[GameSyncManager.CyclingRigidbodyStateIdx].entityHandle, GameSyncManager.CyclingRigidbodyState);
        //Debug.LogError(latestServerStatePayloadTick + "   " + GameSyncManager.CyclingRigidbodyState.rotation);

        GameSyncManager.GameSyncSave();
        if (syncedTick != latestServerStatePayloadTick) Debug.Log("azeaze  " + syncedTick + "    " + latestServerStatePayloadTick );
   
        short rollbackTicksForProjection = (short)(syncedTick - tick);
        if (rollbackTicksForProjection > 0) Debug.Log("wxcwxcwxc");

        /// projection
        {
            GameSyncManager.DEBUGprojection = true;
            while (rollbackTicksForProjection < -1)
            {
                rollbackTicksForProjection++;
                foreach (PlayerNet playerNet in GameSyncManager.Players)
                {
                    short relativeRollbackTicksForProjection = (short)Mathf.Min((rollbackTicksForProjection + (short)(tick - playerNet.latestInputsRecivedTick)), 0);
                    playerNet.Tick(relativeRollbackTicksForProjection);
                }
                GameSyncManager.gameManager.Tick(rollbackTicksForProjection);

                RapierWorld.PhysicsStep(PlayerNet.gameFixedDeltaTime);
            }
            foreach (PlayerNet playerNet in GameSyncManager.Players)
            {
                playerNet.tickDelay = (short)Mathf.Min((short)(tick - playerNet.latestInputsRecivedTick),0f);
            }
            GameSyncManager.DEBUGprojection = false;
        }


        //GameSyncManager.rapierToUnityDatabase.TryGet(CyclicSyncObjectsHandle[GameSyncManager.CyclingRigidbodyStateIdx].entityHandle, out var teete);
        //Debug.Log(ServerManagerNet.tick + "   " +  teete.GameObject.name);

        pendingServerData = false;

    }
}
