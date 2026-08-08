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
        playerNet.inputPayloadRBufferTransmitor = new RingBuffer<InputPayload>(PlayerNet.PayloadTransmiotorRBufferSize);

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
