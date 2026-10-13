using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [HideInInspector]
    public Map map;
    [HideInInspector]
    public int localPlayerIdx;

    uint respawnRowBitMask;
    PlayerNet[] respawnRow = new PlayerNet[8];
    float[] respawnRowTimer = new float[8];

    uint respawnRowBitMaskROLLBACK;
    PlayerNet[] respawnRowROLLBACK = new PlayerNet[8];
    float[] respawnRowTimerROLLBACK = new float[8];

    const float respawnTimer = 3f;

    public float GetPlayerTimer()
    {
        return respawnRowTimer[localPlayerIdx];
    }

    public void InitiateRespawn(PlayerNet player)
    {
        int insertIdx = (31-math.lzcnt((~respawnRowBitMask)&255));
        uint insertMask = 1u << insertIdx;
        respawnRow[insertIdx] = player;
        respawnRowTimer[insertIdx] = respawnTimer;
        respawnRowBitMask |= (uint)insertMask;
        localPlayerIdx = insertIdx;
    }


    public void Tick()
    {
        uint respawnRowBitMaskUpdate = respawnRowBitMask;
        while (respawnRowBitMaskUpdate > 0)
        {
            int bitPlacement = (31 - math.lzcnt(respawnRowBitMaskUpdate));
            uint evaluatedBit = 1u << bitPlacement;

            respawnRowTimer[bitPlacement] -= PlayerNet.gameFixedDeltaTime;

            if(respawnRowTimer[bitPlacement] <=0)
            {
                Respawn(respawnRow[bitPlacement]);
                respawnRowTimer[bitPlacement] = 0;
                respawnRowBitMask &= ~evaluatedBit;
            }

            respawnRowBitMaskUpdate &= ~evaluatedBit;
        }
    }

    public void Respawn(PlayerNet player)
    {

        Vector3 spawnPos = new Vector3();
        Vector2 spawnForce = new Vector2();
        switch (player.team)
        {
            case CollisionLayer.TeamA:
                //Debug.Log("a");
                spawnForce = (map.TeamAspawner.transform.rotation * Vector2.up) * 100;
                spawnPos = map.TeamAspawner.transform.position;
                break;
            case CollisionLayer.TeamB:
                //Debug.Log("B");
                spawnForce = (map.TeamBspawner.transform.rotation * Vector2.up) * 100;
                spawnPos = map.TeamBspawner.transform.position;
                break;
            case CollisionLayer.TeamC:

                break;
            case CollisionLayer.TeamD:

                break;
            default:
                break;
        }

        player.Spawn(spawnPos,spawnForce);

    }


    public void ROLLBACKsave()
    {
        respawnRowBitMaskROLLBACK = respawnRowBitMask;
        for (int i = 0; i < 8; i++)
        {
            respawnRowROLLBACK[i] = respawnRow[i];
            respawnRowTimerROLLBACK[i] = respawnRowTimer[i];
        }
    }

    public void ROLLBACKrestore()
    {
        respawnRowBitMask = respawnRowBitMaskROLLBACK;
        for (int i = 0; i < 8; i++)
        {
            respawnRow[i] = respawnRowROLLBACK[i];
            respawnRowTimer[i] = respawnRowTimerROLLBACK[i];
        }
    }

}
