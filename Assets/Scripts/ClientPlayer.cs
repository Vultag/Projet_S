using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ClientPlayer : MonoBehaviour
{

    private PlayerNet playerNet;
    private uint tick;

    public SpriteRenderer playerIcon;

    public SliderJoint2D Pistonjoint;
    public Rigidbody2D PlayerBody;

    void Start()
    {
        playerNet = GetComponent<PlayerNet>();

        //playerNet.statePayloadRBuffer = new RingBuffer<StatePayload>(PlayerNet.PayloadRBufferSize);
        playerNet.inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);
        //for (int i = 0; i < PlayerNet.PayloadRBufferSize; i++)
        //{
        //    playerNet.inputPayloadRBuffer.Write(new InputPayload(0));
        //}
        playerNet.inputPayloadRBufferTransmitor = new RingBuffer<InputPayload>(PlayerNet.PayloadTransmiotorRBufferSize);
        //for (int i = 0; i < PlayerNet.PayloadTransmiotorRBufferSize; i++)
        //{
        //    playerNet.inputPayloadRBuffer.Write(new InputPayload(0));
        //}

        FindFirstObjectByType<ServerManagerNet>().PromoteTickAsSynced();

    }
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        playerNet.Tick(0);
    }
}
