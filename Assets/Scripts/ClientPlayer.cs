using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ClientPlayer : MonoBehaviour
{

    private PlayerNet playerNet;
    //private uint tick;

    public SpriteRenderer playerIcon;

    public SliderJoint2D Pistonjoint;
    public Rigidbody2D PlayerBody;

    void Start()
    {
        playerNet = GetComponent<PlayerNet>();

        //playerNet.statePayloadRBuffer = new RingBuffer<StatePayload>(PlayerNet.PayloadRBufferSize);

        ////playerNet.inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);
        ////playerNet.inputPayloadRBufferTransmitor = new RingBuffer<InputPayload>(PlayerNet.PayloadTransmiotorRBufferSize);

        //for (int i = 0; i < PlayerNet.PayloadTransmiotorRBufferSize; i++)
        //{
        //    playerNet.inputPayloadRBuffer.Write(new InputPayload(0));
        //}
        //FindFirstObjectByType<ServerManagerNet>(FindObjectsInactive.Include).PromoteTickAsSynced();
        //GameSyncManager.GameSyncSave();

    }
    void Update()
    {

    }
    private void FixedUpdate()
    {
        //Debug.Log(playerNet.team);
        //if (playerNet.inputPayloadRBuffer.Read(0).action1 == Action.Push) Debug.Log("azeazeaze");
        /// Curently reads the latest input from the buffer, skipping the previous ones and relying on reconciliation to recover them
        playerNet.Tick(0);
    }
}
