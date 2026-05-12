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
    }
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        /// NEEDS TO HAPPEN SYNCED WITH ALL IN  SYSTEM
        ///playerNet.Reconciliation(tick);

        var latestInputs = playerNet.latestServerInputPayload;

        playerNet.revertCooldown = (byte)(playerNet.revertCooldown - playerNet.activeRevertCooldown);
        playerNet.ticksTillPistonPushActivation = (byte)(playerNet.ticksTillPistonPushActivation - latestInputs.pistonPushArmed);

        if (playerNet.revertCooldown == 0)
        {
            //Debug.Log("pull");
            playerNet.PistonPull();
            playerNet.pistonPushOrPull = false;
        }

        if (playerNet.ticksTillPistonPushActivation == 0)
        {
            //Debug.Log("jump frame = "+tick);
            //Debug.Log("push");
            playerNet.Bump(new Vector2(0.71f, 0.71f) * 2000);
            playerNet.PistonPush(latestInputs.pistonDirection);
            playerNet.ticksTillPistonPushActivation = 50;
            playerNet.pistonPushOrPull = true;
        }

        PlayerBody.AddTorque((3000 * -latestInputs.direction) - (PlayerBody.angularVelocity * 2f * Mathf.Abs(latestInputs.direction)), ForceMode2D.Force);

        /// steped by owner player. must execute before.
        ///Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);

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
            pistonAngle = Pistonjoint.angle
        });

        tick++;
    }
}
