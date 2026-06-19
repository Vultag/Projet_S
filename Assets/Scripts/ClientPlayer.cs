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

        playerNet.statePayloadRBuffer = new RingBuffer<StatePayload>(PlayerNet.PayloadRBufferSize);
        playerNet.inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);

    }
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        /// NEEDS TO HAPPEN SYNCED WITH ALL IN  SYSTEM
        ///playerNet.Reconciliation(tick);

        playerNet.inputPayloadRBuffer.Write(playerNet.latestServerInputPayload);

        playerNet.Tick(0);

        playerNet.statePayloadRBuffer.Write(new StatePayload
        {
            tick = tick,
            playerPhyState = new PhysicsState
            {
                position = PlayerBody.position,
                rotation = PlayerBody.rotation,
                linearVelocity = PlayerBody.linearVelocity,
                angularVelocity = PlayerBody.angularVelocity,
            },
            pistonPhyState = new PhysicsState
            {
                position = playerNet.PistonBody.position,
                rotation = playerNet.PistonBody.rotation,
                linearVelocity = playerNet.PistonBody.linearVelocity,
                angularVelocity = playerNet.PistonBody.angularVelocity,
            },
            cogPhyState = new PhysicsState
            {
                position = playerNet.CogBody.position,
                rotation = playerNet.CogBody.rotation,
                linearVelocity = playerNet.CogBody.linearVelocity,
                angularVelocity = playerNet.CogBody.angularVelocity,
            },
            ticksTillPistonPushActivation = playerNet.ticksTillPistonPushActivation,
            activeRevertCooldown = playerNet.activeRevertCooldown,
            revertCooldown = playerNet.revertCooldown,
            pistonPushOrPull = playerNet.pistonPushOrPull,
            pistonAngle = Pistonjoint.angle,
            pistonPushArmed = playerNet.pistonPushArmed == 0 ? false : true,
        });

        tick++;
    }
}
