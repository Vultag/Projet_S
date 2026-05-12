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
    public byte pistonPushArmed;
    //public byte ticksTillPistonPushActivation;
    public Vector2 pistonDirection;
    public bool finger1press;
    public Vector2 finger1pressPosition; 
    public bool finger2press;
    public Vector2 finger2pressPosition;
    public static InputPayload Default()
    {
        return new InputPayload
        {
            tick = 0,
            direction = 0,
            pistonPushArmed = 0,
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
        serializer.SerializeValue(ref pistonPushArmed);
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
    public bool pistonPushOrPull;

    public const float gameFixedDeltaTime = 1f / 60f;


    [HideInInspector]
    public RingBuffer<InputPayload> inputPayloadRBuffer;
    [HideInInspector]
    public RingBuffer<StatePayload> statePayloadRBuffer;

    [HideInInspector]
    public StatePayload latestServerStatePayload;
    private uint latestServerStatePayloadTick = 0;
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
    private ServerPlayer serverPlayer;
    private ClientPlayer clientPlayer;
    //public uint gameTick { get; private set; }

    private void FixedUpdate()
    {

        //Debug.Log(inputPayloadRBuffer.Read(0).tick);
    }

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");

        player = GetComponent<Player>();
        serverPlayer = GetComponent<ServerPlayer>();
        clientPlayer = GetComponent<ClientPlayer>();

        var ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);
        powerupManager = ui.GetComponent<PowerupManager>();

        activePowerup.OnValueChanged += (_, v) => UpdatePowerupClientRpc(v);


        PushM = new JointMotor2D { motorSpeed = 100, maxMotorTorque = Pistonjoint.motor.maxMotorTorque };
        PullM = Pistonjoint.motor;
        Physics2D.IgnoreCollision(PlayerBody.GetComponent<Collider2D>(), Pistonjoint.GetComponent<Collider2D>(), true);

        var propellerS = propeller.GetComponent<Propeller>();
        var graplingS = graplingHook.GetComponent<Grapling>();
        /// remplace by active powerup from manager network var
        propellerS.enabled = IsOwner;
        graplingS.enabled = IsOwner;

        //if (IsServer)
        //{
        //    Destroy(player);
        //    Destroy(clientPlayer);
        //    serverPlayer.enabled = true;
        //}

        inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);

        if(IsServer)
        {
            Destroy(player);
            Destroy(clientPlayer);
        }
        else if (IsOwner)
        {
            //Destroy(serverPlayer);
            Destroy(clientPlayer);
            player.enabled = true;
        }
        else
        {
            //Destroy(serverPlayer);
            Destroy(player);
            clientPlayer.enabled = true;
        }


        //graplingHookActive.OnValueChanged += (_, v) => UpdatePowerupClientRpc(PowerUps.GraplinHook, v);
        //propellerActive.OnValueChanged += (_, v) => UpdatePowerupClientRpc(PowerUps.Propeller, v);

        //if (IsOwnedByServer) playerIcon.color = Color.red;

        //if (!IsOwner)
        //{
        //    this.enabled = false;
        //    return;
        //}

        //var ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);
        //powerupManager = ui.GetComponent<PowerupManager>();
        //ui.Player = this;

        //Camera.main.GetComponent<TrackPlayer>().PlayerGB = PlayerBody.gameObject;
        //Camera.main.GetComponent<TrackPlayer>().enabled = true;
        ////PullM = new JointMotor2D { motorSpeed = -10, maxMotorTorque = Pistonjoint.motor.maxMotorTorque };

        //if (IsServer) playerIcon.color = Color.red;


        //ui.gameObject.SetActive(true);
        //var gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        //gameManager.gameObject.SetActive(true);


    }

    public void Reconciliation(uint tick)
    {
        /// Discard delayed ServerStatePayloads
        if (latestServerStatePayload.tick < latestServerStatePayloadTick)
        {
            return;
        }
        latestServerStatePayloadTick = latestServerStatePayload.tick;


        //StatePayload latestServerSPayload = latestServerStatePayload;
        short rollbackTicks = (short)((latestServerStatePayload.tick - tick));
        if (Mathf.Abs(rollbackTicks) > PlayerNet.PayloadRBufferSize) Debug.LogError("capacity exeeded : " + (short)(latestServerStatePayload.tick - tick));

        var dist = Vector2.Distance(latestServerStatePayload.playerPhyState.position, statePayloadRBuffer.Read((short)(rollbackTicks + 1)).playerPhyState.position);

        //if(dist>0.0001f)Debug.Log(dist);

        if (!(Vector2.Distance(latestServerStatePayload.playerPhyState.position, statePayloadRBuffer.Read((short)(rollbackTicks+1)).playerPhyState.position) > 0.5f)) return;


        Debug.Log("reconcile");

        //Debug.Log("dddss = " + statePayloadRBuffer.Read((short)(rollbackTicks + 1)).playerPhyState.position);
        //Debug.Log("qqq = " + latestServerStatePayload.playerPhyState.position);
        //Debug.Log("latestServerStatePayloadtick = " + latestServerStatePayload.tick);
        //Debug.Log(statePayloadRBuffer.Read((short)(rollbackTicks + 1)).tick);

        //Debug.Break();

        //var head1 = statePayloadRBuffer.head - (rollbackTicks+1);
        //Debug.Log("head1 " + head1);
        //Debug.Log("head1??? " + statePayloadRBuffer.head);

        //rollbackTicks++;

        //Debug.Log("head = " + statePayloadRBuffer.head);

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

        statePayloadRBuffer.SlideHead((short)(rollbackTicks));
        InputPayload tickInputs = inputPayloadRBuffer.Read(rollbackTicks);


        //Debug.LogError("tick start = " + ticksTillPistonPushActivation);

        //Debug.Log(rollbackTicks);

        //var test01 = statePayloadRBuffer.Read(1).tick;
        //var test02 = latestServerStatePayload.tick;
        //if (test01 != test02) Debug.Log(test01 + " : " + test02);

        //Debug.Log("latestServerStatePayloadtick = " + latestServerStatePayload.tick);
        //Debug.Log("head = " + statePayloadRBuffer.head);
        //Debug.Log((rollbackTicks));

        statePayloadRBuffer.Write(new StatePayload
        {
            tick = (uint)(tick + rollbackTicks),
            playerPhyState = new PhysicsState
            {
                position = PlayerBody.position,
                rotation = PlayerBody.rotation,
                linearVelocity = PlayerBody.linearVelocity,
                angularVelocity = PlayerBody.angularVelocity,
            },
            pistonPhyState = new PhysicsState
            {
                position = PistonBody.position,
                rotation = PistonBody.rotation,
                linearVelocity = PistonBody.linearVelocity,
                angularVelocity = PistonBody.angularVelocity,
            },
            cogPhyState = new PhysicsState
            {
                position = CogBody.position,
                rotation = CogBody.rotation,
                linearVelocity = CogBody.linearVelocity,
                angularVelocity = CogBody.angularVelocity,
            },
            ticksTillPistonPushActivation = ticksTillPistonPushActivation,
            revertCooldown = revertCooldown,
            activeRevertCooldown = activeRevertCooldown,
            pistonPushOrPull = latestServerStatePayload.pistonPushOrPull,
            pistonAngle = latestServerStatePayload.pistonAngle,
        });

        //Debug.Log("last : " + latestServerSPayload.tick);
        //Debug.Log("current : " + tick);
        //Debug.Log("start : " + (tick + rollbackTicks));

        var temp = rollbackTicks;
        rollbackTicks++;

        bool pPushorPull = latestServerStatePayload.pistonPushOrPull;

        while (rollbackTicks < 0)
        {
            tickInputs = inputPayloadRBuffer.Read((short)(rollbackTicks+1));

            revertCooldown = (byte)(revertCooldown - activeRevertCooldown);
            ticksTillPistonPushActivation = (byte)(ticksTillPistonPushActivation - tickInputs.pistonPushArmed);


            //Debug.LogError("ticking = " + ticksTillPistonPushActivation);
            //Debug.Log("ticking active = " + tickInputs.pistonPushArmed + "at tick : "+(tick + rollbackTicks));

            if (ticksTillPistonPushActivation > 50) Debug.LogError("bug");
            if (revertCooldown > 120) Debug.LogError("bugsss");

            if (revertCooldown == 0)
            {
                PistonPull();
                pPushorPull = false;
            }

            if (ticksTillPistonPushActivation == 0)
            {
                //Debug.LogError("jump frame = "+(tick + rollbackTicks));
                PistonPush(tickInputs.pistonDirection);
                ticksTillPistonPushActivation = 50;
                pPushorPull = true;
            }
            //var head2 = statePayloadRBuffer.head;
            //Debug.Log("write " + (head2 + 1));


            PlayerBody.AddTorque((3000 * -tickInputs.direction) - (PlayerBody.angularVelocity * 2f * Mathf.Abs(tickInputs.direction)), ForceMode2D.Force);

            Physics2D.Simulate(gameFixedDeltaTime);

            //Debug.Log("tick : " + (tick + rollbackTicks));

            /// NOT NECESSERY ? WONT GO BACK AS BEEN VALIDATED ?
            statePayloadRBuffer.Write(new StatePayload
            {
                tick = (uint)(tick + rollbackTicks),
                playerPhyState = new PhysicsState
                {
                    position = PlayerBody.position,
                    rotation = PlayerBody.rotation,
                    linearVelocity = PlayerBody.linearVelocity,
                    angularVelocity = PlayerBody.angularVelocity,
                },
                pistonPhyState = new PhysicsState
                {
                    position = PistonBody.position,
                    rotation = PistonBody.rotation,
                    linearVelocity = PistonBody.linearVelocity,
                    angularVelocity = PistonBody.angularVelocity,
                },
                cogPhyState = new PhysicsState
                {
                    position = CogBody.position,
                    rotation = CogBody.rotation,
                    linearVelocity = CogBody.linearVelocity,
                    angularVelocity = CogBody.angularVelocity,
                },
                ticksTillPistonPushActivation = ticksTillPistonPushActivation,
                revertCooldown = revertCooldown,
                activeRevertCooldown = activeRevertCooldown,
                pistonPushOrPull = pPushorPull,
                pistonAngle = Pistonjoint.angle,
            });


            rollbackTicks++;
        }

        //var test1 = latestServerStatePayload.tick;
        //var test2 = statePayloadRBuffer.Read((short)(temp + 1)).tick;
        //if (test1 != test2) Debug.Log(test1 + " : " + test2);

        var test1 = latestServerStatePayload.playerPhyState.position;
        var test2 = statePayloadRBuffer.Read((short)(temp + 1)).playerPhyState.position;
        if (test1 != test2) Debug.Log(test1 + " : " + test2);

        //Debug.Break();

    }

    public void Tick(uint tick)
    {
        var latestPayloadTick = inputPayloadRBuffer.Read(0).tick;
        InputPayload payload = inputPayloadRBuffer.Read((short)(tick - latestPayloadTick));

        revertCooldown = (byte)(revertCooldown - activeRevertCooldown);
        ticksTillPistonPushActivation = (byte)(ticksTillPistonPushActivation - payload.pistonPushArmed);

        //if (ticksTillPistonPushActivation < 50) Debug.Log("ticking = "+ ticksTillPistonPushActivation);

        if (ticksTillPistonPushActivation > 50) Debug.Log("bug222");
        if (revertCooldown > 120) Debug.Log("bugsss2222");

        if (revertCooldown == 0)
        {
            //Debug.Log("pull");
            PistonPull();
            pistonPushOrPull = false;
        }
        if (ticksTillPistonPushActivation == 0)
        {
            //Debug.Log("push" + tick);
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
    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendClientsInputsClientRpc(InputPayload inputPayload, ClientRpcParams rpcParams)
    {
        latestServerInputPayload = inputPayload;
    }
    ////[ServerRpc]
    ////public void SendInputBatchServerRPC()//(FixedList64Bytes<InputPayload> clientInputBatch)
    ////{
    ////    //inputBatch = clientInputBatch;
    ////    Debug.Log("recieve");
    ////}
    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendLatestStatePayloadClientRpc(StatePayload statePayload)
    {
        latestServerStatePayload = statePayload;
    }

    //private void UpdatePowerup(PowerUps powerup, bool state)
    //{
    //    UpdatePowerupClientRpc(powerup, state);
    //}
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
