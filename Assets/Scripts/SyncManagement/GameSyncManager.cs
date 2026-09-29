using System.Collections.Generic;
using UnityEngine;

public static class GameSyncManager
{

    //public static GameSyncManager Instance { get; private set; }

    [HideInInspector]
    public static List<PlayerNet> Players;


    /// OPTI : bitmask to know which one to update instead of ifs ?
    public static RapierToUnityDatabase rapierToUnityDatabase { get; private set; }
    public static IDamageableDatabase damageableDatabase { get; private set; }

    public static BulletPool bulletPool;

    static GameSyncManager()
    {
        rapierToUnityDatabase = new RapierToUnityDatabase(512);
        damageableDatabase = new IDamageableDatabase(32);
        Players = new List<PlayerNet>();
    }

    //public GameSyncManager()
    //{
    //    Instance = this;
    //    RapierToUnityDatabase = new(512);
    //}

    public static void GameSyncSave()
    {
        //if(GameSyncManager.Players.Count == 2)
        //    Debug.LogError("save");
        RapierWorld.world_store_snapshot(RapierWorld.world);
        bulletPool.SaveState();

        if (!rapierToUnityDatabase.IsDatabaseSynced())
        {
            rapierToUnityDatabase.ROLLBACKsave();
        }
        /// have to do it regardless for damageable fields
        ///if (!damageableDatabase.IsDatabaseSynced())
        {
            damageableDatabase.ROLLBACKsave();
        }

        foreach (PlayerNet player in GameSyncManager.Players)
        {
            player.UpdateSyncedStates();
            ///player.latestSyncedMechanicsStatePayload = player.mechanicalState;
        }
    }

    public static void GameSyncRestore()
    {
        //Debug.LogError("restore");
        RapierWorld.world_restore_snapshot(RapierWorld.world);
        
        bulletPool.RestoreState();

        if (!rapierToUnityDatabase.IsDatabaseSynced())
        {
            //Debug.Log("ROLLBACKrestore");
            rapierToUnityDatabase.ROLLBACKrestore();
        }
        /// have to do it regardless for damageable fields
        ///if (!damageableDatabase.IsDatabaseSynced())
        {
            damageableDatabase.ROLLBACKrestore();
        }

        foreach (PlayerNet player in Players)
        {
            player.RestoreState(player.latestSyncedMechanicsStatePayload);
        }
    }

}
