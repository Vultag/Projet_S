using NUnit.Framework.Internal;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

//public NetworkVariable<bool> propellerActive =
//   new NetworkVariable<bool>(
//       false,
//       NetworkVariableReadPermission.Everyone,
//       NetworkVariableWritePermission.Server
//   );
//public NetworkVariable<bool> graplingHookActive =
//new NetworkVariable<bool>(
//    false,
//    NetworkVariableReadPermission.Everyone,
//    NetworkVariableWritePermission.Server
//);
public struct PhysicsState : INetworkSerializable
{
    public Vector2 position;
    public float rotation;
    public Vector2 linearVelocity;
    public float angularVelocity;

    public void NetworkSerialize<T>(BufferSerializer<T> s)
        where T : IReaderWriter
    {
        s.SerializeValue(ref position);
        s.SerializeValue(ref rotation);
        s.SerializeValue(ref linearVelocity);
        s.SerializeValue(ref angularVelocity);
    }
}

public struct InputPayload : INetworkSerializable
{
    public uint tick;
    public sbyte direction;
    public bool pistonPush;
    //public byte ticksTillPistonPushActivation;
    public Vector2 pistonDirection;
    public bool finger1press;
    public Vector2 finger1pressPosition; 
    public bool finger2press;
    public Vector2 finger2pressPosition;
    public static InputPayload Default(uint atTick)
    {
        return new InputPayload
        {
            tick = atTick,
            direction = 0,
            pistonPush = false,
            //ticksTillPistonPushActivation = 50,
            pistonDirection = Vector2.zero,
            finger1press = false,
            finger1pressPosition = Vector2.zero,
            finger2press = false,
            finger2pressPosition = Vector2.zero
        };
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref pistonPush);
        //serializer.SerializeValue(ref ticksTillPistonPushActivation);
        serializer.SerializeValue(ref pistonDirection);
        serializer.SerializeValue(ref finger1press);
        serializer.SerializeValue(ref finger1pressPosition);
        serializer.SerializeValue(ref finger2press);
        serializer.SerializeValue(ref finger2pressPosition);
    }
}
public struct StatePayload : INetworkSerializable
{
    public uint tick;

    public PhysicsState playerPhyState;
    public PhysicsState pistonPhyState;
    public PhysicsState cogPhyState;

    public byte ticksTillPistonPushActivation;
    public byte revertCooldown;
    public byte activeRevertCooldown;
    public bool pistonPushOrPull;
    public bool pistonPushArmed;
    public float pistonAngle;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref playerPhyState);
        serializer.SerializeValue(ref pistonPhyState);
        serializer.SerializeValue(ref cogPhyState);
        serializer.SerializeValue(ref ticksTillPistonPushActivation);
        serializer.SerializeValue(ref revertCooldown);
        serializer.SerializeValue(ref activeRevertCooldown);
        serializer.SerializeValue(ref pistonPushOrPull);
        serializer.SerializeValue(ref pistonPushArmed);
        serializer.SerializeValue(ref pistonAngle);
    }
}

public class PlayerNet : NetworkBehaviour
{
    public static short PayloadRBufferSize = 512;

    private PowerupManager powerupManager;
    public Rigidbody2D PlayerBody;
    public Rigidbody2D PistonBody;
    public Rigidbody2D CogBody;
    public SliderJoint2D Pistonjoint;
    public GameObject graplingHook;
    public GameObject propeller;
    public SpriteRenderer playerIcon;

    //[HideInInspector]
    private JointMotor2D PushM;
    //[HideInInspector]
    private JointMotor2D PullM;

    [HideInInspector]
    public byte ticksTillPistonPushActivation = 50;
    [HideInInspector]
    public byte revertCooldown;
    [HideInInspector]
    public byte activeRevertCooldown = 0;
    [HideInInspector]
    public byte pistonPushArmed = 0;
    [HideInInspector]
    public bool pistonPushOrPull;

    public const float gameFixedDeltaTime = 1f / 60f;


    [HideInInspector]
    public RingBuffer<InputPayload> inputPayloadRBuffer;
    [HideInInspector]
    public RingBuffer<StatePayload> statePayloadRBuffer;

    [HideInInspector]
    public StatePayload latestServerStatePayload;
    [HideInInspector]
    public InputPayload latestServerInputPayload;

    [HideInInspector]
    public NetworkVariable<PowerUps> activePowerup =
       new NetworkVariable<PowerUps>(
           PowerUps.None,
           NetworkVariableReadPermission.Everyone,
           NetworkVariableWritePermission.Server
       );

    private Player player;
    private ClientPlayer clientPlayer;

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");

        player = GetComponent<Player>();
        clientPlayer = GetComponent<ClientPlayer>();

        var ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);
        powerupManager = ui.GetComponent<PowerupManager>();

        activePowerup.OnValueChanged += (_, v) => UpdatePowerupClientRpc(v);


        PushM = new JointMotor2D { motorSpeed = 100, maxMotorTorque = Pistonjoint.motor.maxMotorTorque };
        PullM = Pistonjoint.motor;
        Physics2D.IgnoreCollision(PlayerBody.GetComponent<Collider2D>(), Pistonjoint.GetComponent<Collider2D>(), true);

        if (IsServer)
        {
            Destroy(player);
            Destroy(clientPlayer);
        }
        else if (IsOwner)
        {
            //Destroy(serverPlayer);
            Destroy(clientPlayer);
            //player.enabled = true;
        }
        else
        {
            Destroy(player);
            clientPlayer.enabled = true;
        }


        //graplingHookActive.OnValueChanged += (_, v) => UpdatePowerupClientRpc(PowerUps.GraplinHook, v);
        //propellerActive.OnValueChanged += (_, v) => UpdatePowerupClientRpc(PowerUps.Propeller, v);
    }
 

    public void SynchronizeState()
    {
        PlayerBody.position = latestServerStatePayload.playerPhyState.position;
        PlayerBody.rotation = latestServerStatePayload.playerPhyState.rotation;
        PlayerBody.linearVelocity = latestServerStatePayload.playerPhyState.linearVelocity;
        PlayerBody.angularVelocity = latestServerStatePayload.playerPhyState.angularVelocity;
        PistonBody.position = latestServerStatePayload.pistonPhyState.position;
        PistonBody.rotation = latestServerStatePayload.pistonPhyState.rotation;
        PistonBody.linearVelocity = latestServerStatePayload.pistonPhyState.linearVelocity;
        PistonBody.angularVelocity = latestServerStatePayload.pistonPhyState.angularVelocity;
        CogBody.position = latestServerStatePayload.cogPhyState.position;
        CogBody.rotation = latestServerStatePayload.cogPhyState.rotation;
        CogBody.linearVelocity = latestServerStatePayload.cogPhyState.linearVelocity;
        CogBody.angularVelocity = latestServerStatePayload.cogPhyState.angularVelocity;
        ticksTillPistonPushActivation = latestServerStatePayload.ticksTillPistonPushActivation;
        activeRevertCooldown = latestServerStatePayload.activeRevertCooldown;
        revertCooldown = latestServerStatePayload.revertCooldown;
        Pistonjoint.motor = latestServerStatePayload.pistonPushOrPull ? PushM : PullM;
        PistonRotate(latestServerStatePayload.pistonAngle);
        pistonPushOrPull = latestServerStatePayload.pistonPushOrPull;
        pistonPushArmed = latestServerStatePayload.pistonPushArmed ? (byte)1 : (byte)0;
    }

    public void Tick(short relativeTick)
    {
        InputPayload payload = inputPayloadRBuffer.Read(relativeTick);

        pistonPushArmed = (pistonPushArmed == 1 | payload.pistonPush == true) ? (byte)1 : (byte)0;
        revertCooldown = (byte)(revertCooldown - activeRevertCooldown);
        ticksTillPistonPushActivation = (byte)(ticksTillPistonPushActivation - pistonPushArmed);

        if (ticksTillPistonPushActivation > 50) Debug.Log("bug222");
        if (revertCooldown > 120) Debug.Log("bugsss2222");

        if (revertCooldown == 0)
        {
            PistonPull();
            pistonPushOrPull = false;
        }
        if (ticksTillPistonPushActivation == 0)
        {
            PistonPush(payload.pistonDirection);
            ticksTillPistonPushActivation = 50;
            pistonPushOrPull = true;
        }

        PlayerBody.AddTorque((3000 * -payload.direction) - (PlayerBody.angularVelocity * 2f * Mathf.Abs(payload.direction)), ForceMode2D.Force);

    }

    public void PistonPush(Vector2 dir)
    {
        PistonRotate(dir);
        Pistonjoint.motor = PushM;
        activeRevertCooldown = 1;
        pistonPushArmed = 0;
        revertCooldown = 120;
    }
    private void PistonRotate(Vector2 dir)
    {
        //Pistonjoint.angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg - 90 - PlayerBody.rotation;
        Pistonjoint.angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg - 90;
    }
    private void PistonRotate(float angle)
    {
        Pistonjoint.angle = angle;
    }
    public void PistonPull()
    {
        Pistonjoint.motor = PullM;
        revertCooldown = 120;
        activeRevertCooldown = 0;
    }
    public void ProcessInputPayload(InputPayload inputPayload)
    {
        inputPayloadRBuffer.Write(inputPayload);
        SendInputPayloadServerRPC(inputPayload);
    }

    [ServerRpc]
    private void SendInputPayloadServerRPC(InputPayload inputPayload)//(FixedList64Bytes<InputPayload> clientInputBatch)
    {
        /// batch and send as unreliable ?
        inputPayloadRBuffer.Write(inputPayload);
    }
   
    [ClientRpc]
    private void UpdatePowerupClientRpc(PowerUps powerup)
    {
        //player.UpdatePowerupLocal(powerup);
        switch (powerupManager.activePowerup)
        {
            case PowerUps.GraplinHook:
                graplingHook.SetActive(false);
                break;
            case PowerUps.Propeller:
                propeller.SetActive(false);
                break;



            default:
                break;
        }

        /// handled beforehand
        //if (powerupManager.activePowerup == powerup) return;

        switch (powerup)
        {
            case PowerUps.GraplinHook:
                graplingHook.SetActive(true);
                break;
            case PowerUps.Propeller:
                propeller.SetActive(true);
                break;



            default:
                break;
        }
    }
    [ServerRpc]
    public void UpdatePowerupServerRpc(PowerUps powerup)
    {
        activePowerup.Value = powerup;
        //switch (powerup)
        //{
        //    case PowerUps.GraplinHook:
        //        graplingHookActive.Value = state;
        //        break;
        //    case PowerUps.Propeller:
        //        propellerActive.Value = state;
        //        break;



        //    default:
        //        break;
        //}
    }





    public void Bump(Vector2 force)
    {
        PlayerBody.AddForce(force, ForceMode2D.Impulse);
    }


}
