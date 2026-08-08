using NUnit.Framework.Internal;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
////public struct PhysicsState : INetworkSerializable
////{
////    public Vector2 position;
////    public float rotation;
////    public Vector2 linearVelocity;
////    public float angularVelocity;

////    public void NetworkSerialize<T>(BufferSerializer<T> s)where T : IReaderWriter
////    {
////        s.SerializeValue(ref position);
////        s.SerializeValue(ref rotation);
////        s.SerializeValue(ref linearVelocity);
////        s.SerializeValue(ref angularVelocity);
////    }
////}
public struct MechanicsState : INetworkSerializable
{
    public byte ticksTillPistonPushActivation;
    public byte revertCooldown;
    public byte activeRevertCooldown;
    public byte pistonPushOrPull;
    public byte pistonPushArmed;
    public float pistonAngle;
    public static MechanicsState Default()
    {
        return new MechanicsState
        {
            ticksTillPistonPushActivation = 50,
            revertCooldown = 120,
            activeRevertCooldown = 0,
            pistonPushOrPull = 0,
            pistonPushArmed = 0,
            pistonAngle = 0
        };
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ticksTillPistonPushActivation);
        serializer.SerializeValue(ref revertCooldown);
        serializer.SerializeValue(ref activeRevertCooldown);
        serializer.SerializeValue(ref pistonPushOrPull);
        serializer.SerializeValue(ref pistonPushArmed);
        serializer.SerializeValue(ref pistonAngle);
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

    public BodyStateFFI playerPhyState;
    public BodyStateFFI pistonPhyState;
    public BodyStateFFI cogPhyState;
    public MechanicsState playerMechanicsState;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref playerPhyState);
        serializer.SerializeValue(ref pistonPhyState);
        serializer.SerializeValue(ref cogPhyState);
        serializer.SerializeValue(ref playerMechanicsState);
    }
}

public class PlayerNet : NetworkBehaviour
{
    public static short PayloadRBufferSize = 512;
    public static short PayloadTransmiotorRBufferSize = 32;
    
    private PowerupManager powerupManager;
    public RapierBody PlayerBody;
    public RapierBody PistonBody;
    public RapierBody CogBody;
    public RapierSliderJoint Pistonjoint;
    public GameObject graplingHook;
    public GameObject propeller;
    public SpriteRenderer playerIcon;

    //private JointMotor2D PushM;
    //private JointMotor2D PullM;

    [HideInInspector]
    public MechanicsState mechanicalState;

    public const float gameFixedDeltaTime = 1f / 60f;


    [HideInInspector]
    public RingBuffer<InputPayload> inputPayloadRBuffer;
    [HideInInspector]
    public RingBuffer<InputPayload> inputPayloadRBufferTransmitor;
    [HideInInspector]
    public uint latestInputsRecivedTick;
    //[HideInInspector]
    //public RingBuffer<StatePayload> statePayloadRBuffer;

    [HideInInspector]
    public StatePayload latestServerStatePayload;
    [HideInInspector]
    public MechanicsState latestSyncedMechanicsStatePayload;


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
        mechanicalState = MechanicsState.Default();
        latestSyncedMechanicsStatePayload = MechanicsState.Default();

        //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");

        player = GetComponent<Player>();
        clientPlayer = GetComponent<ClientPlayer>();

        var ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);
        powerupManager = ui.GetComponent<PowerupManager>();

        activePowerup.OnValueChanged += (_, v) => UpdatePowerupClientRpc(v);


        //PushM = new JointMotor2D { motorSpeed = 100, maxMotorTorque = Pistonjoint.motor.maxMotorTorque };
        //PullM = Pistonjoint.motor;
        //Physics2D.IgnoreCollision(PlayerBody.GetComponent<Collider2D>(), Pistonjoint.GetComponent<Collider2D>(), true);

        if (IsServer)
        {
            Destroy(player);
            Destroy(clientPlayer);
        }
        else if (IsOwner)
        {
            Destroy(clientPlayer);
            Destroy(GetComponentInChildren<ObjectsStatesCollector>().gameObject);
        }
        else
        {
            Destroy(player);
            clientPlayer.enabled = true;
            Destroy(GetComponentInChildren<ObjectsStatesCollector>().gameObject);
        }


        //graplingHookActive.OnValueChanged += (_, v) => UpdatePowerupClientRpc(PowerUps.GraplinHook, v);
        //propellerActive.OnValueChanged += (_, v) => UpdatePowerupClientRpc(PowerUps.Propeller, v);
    }

    public void RestoreState(MechanicsState mechState)
    {
        mechanicalState.ticksTillPistonPushActivation = mechState.ticksTillPistonPushActivation;
        mechanicalState.activeRevertCooldown = mechState.activeRevertCooldown;
        mechanicalState.revertCooldown = mechState.revertCooldown;
        ////Pistonjoint.motor = latestServerStatePayload.pistonPushOrPull ? PushM : PullM;
        ///PistonRotate(mechState.pistonAngle);
        mechanicalState.pistonAngle = mechState.pistonAngle;
        mechanicalState.pistonPushOrPull = mechState.pistonPushOrPull;
        mechanicalState.pistonPushArmed = mechState.pistonPushArmed;

        if (mechanicalState.ticksTillPistonPushActivation == 0) Debug.Log("qbsdqsbqsdq");
        if (mechanicalState.revertCooldown == 0) Debug.Log("xcwcwx");
    }
    public void RestoreState(StatePayload State)
    {
        //var temp = RapierWorld.body_get_state(RapierWorld.world, PlayerBody.entityHandle).x;
        //if (State.playerPhyState.x != temp) Debug.Log(State.playerPhyState.x- temp);

        RapierWorld.body_set_state(RapierWorld.world, PlayerBody.entityHandle, State.playerPhyState);
        RapierWorld.body_set_state(RapierWorld.world, PistonBody.entityHandle, State.pistonPhyState);
        RapierWorld.body_set_state(RapierWorld.world, CogBody.entityHandle, State.cogPhyState);

        mechanicalState.ticksTillPistonPushActivation = State.playerMechanicsState.ticksTillPistonPushActivation;
        mechanicalState.activeRevertCooldown = State.playerMechanicsState.activeRevertCooldown;
        mechanicalState.revertCooldown = State.playerMechanicsState.revertCooldown;
        ////Pistonjoint.motor = latestServerStatePayload.pistonPushOrPull ? PushM : PullM;
        //PistonRotate(State.playerMechanicsState.pistonAngle);
        mechanicalState.pistonAngle = State.playerMechanicsState.pistonAngle;
        mechanicalState.pistonPushOrPull = State.playerMechanicsState.pistonPushOrPull;
        mechanicalState.pistonPushArmed = State.playerMechanicsState.pistonPushArmed;

        if (mechanicalState.ticksTillPistonPushActivation == 0) Debug.Log("qbsdqsbqsdq");
        if (mechanicalState.revertCooldown == 0) Debug.Log("xcwcwx");
    }



    public void SynchronizeWorld()
    {
        //PlayerBody.Sleep();
        //PistonBody.Sleep();
        //CogBody.Sleep();
        //PlayerBody.WakeUp();
        //PistonBody.WakeUp();
        //CogBody.WakeUp();



        //var playerPhyState = RapierWorld.body_get_state(RapierWorld.world, PlayerBody.entityHandle);
        //var pistonPhyState = RapierWorld.body_get_state(RapierWorld.world, PistonBody.entityHandle);
        //var cogPhyState = RapierWorld.body_get_state(RapierWorld.world, CogBody.entityHandle);

        //RapierWorld.set

        //PlayerBody.position = latestServerStatePayload.playerPhyState.position;
        //PlayerBody.rotation = latestServerStatePayload.playerPhyState.rotation;
        //PlayerBody.linearVelocity = latestServerStatePayload.playerPhyState.linearVelocity;
        //PlayerBody.angularVelocity = latestServerStatePayload.playerPhyState.angularVelocity;
        //PistonBody.position = latestServerStatePayload.pistonPhyState.position;
        //PistonBody.rotation = latestServerStatePayload.pistonPhyState.rotation;
        //PistonBody.linearVelocity = latestServerStatePayload.pistonPhyState.linearVelocity;
        //PistonBody.angularVelocity = latestServerStatePayload.pistonPhyState.angularVelocity;
        //CogBody.position = latestServerStatePayload.cogPhyState.position;
        //CogBody.rotation = latestServerStatePayload.cogPhyState.rotation;
        //CogBody.linearVelocity = latestServerStatePayload.cogPhyState.linearVelocity;
        //CogBody.angularVelocity = latestServerStatePayload.cogPhyState.angularVelocity;
        mechanicalState.ticksTillPistonPushActivation = latestServerStatePayload.playerMechanicsState.ticksTillPistonPushActivation;
        mechanicalState.activeRevertCooldown = latestServerStatePayload.playerMechanicsState.activeRevertCooldown;
        mechanicalState.revertCooldown = latestServerStatePayload.playerMechanicsState.revertCooldown;
        ////Pistonjoint.motor = latestServerStatePayload.pistonPushOrPull ? PushM : PullM;
        PistonRotate(latestServerStatePayload.playerMechanicsState.pistonAngle);
        mechanicalState.pistonPushOrPull = latestServerStatePayload.playerMechanicsState.pistonPushOrPull;
        mechanicalState.pistonPushArmed = latestServerStatePayload.playerMechanicsState.pistonPushArmed;

        if (mechanicalState.ticksTillPistonPushActivation == 0) Debug.Log("qbsdqsbqsdq");
        if (mechanicalState.revertCooldown == 0) Debug.Log("xcwcwx");

        //if (pistonPushArmed == 1 & ticksTillPistonPushActivation == 49) Debug.Log("arm sync");
        //if (pistonPushArmed == 1 & ticksTillPistonPushActivation == 50) Debug.Log("qcqscqcscqscxc");
        //if (pistonPushArmed == 1 & ticksTillPistonPushActivation == 48) Debug.Log("wxccwx");
        //if (pistonPushArmed == 1 & ticksTillPistonPushActivation == 0) Debug.Log("549xwcxxcw849");

        //if (pistonPushArmed == 1) Debug.Log(ticksTillPistonPushActivation + "   " + latestServerStatePayload.tick + "   " + ServerManagerNet.tick);
    }

    public void Tick(short relativeTick)
    {
        InputPayload payload = inputPayloadRBuffer.Read(relativeTick);


        mechanicalState.pistonPushArmed = (mechanicalState.pistonPushArmed == 1 | payload.pistonPush == true) ? (byte)1 : (byte)0;
        mechanicalState.revertCooldown = (byte)(mechanicalState.revertCooldown - mechanicalState.activeRevertCooldown);
        mechanicalState.ticksTillPistonPushActivation = (byte)(mechanicalState.ticksTillPistonPushActivation - mechanicalState.pistonPushArmed);



        if (payload.pistonPush == true)
        {
            //Debug.Log("arm at index in buffer : " +  ((inputPayloadRBuffer.head + relativeTick) & (PlayerNet.PayloadRBufferSize - 1)) + "   rollback : " + relativeTick + "    local tick : " + ServerManagerNet.tick + "    combined : " + (ServerManagerNet.tick+relativeTick) + "    head : " + inputPayloadRBuffer.head);

            //if (IsServer) Debug.Log("arm at " + (ServerManagerNet.tick) + " angle " + Pistonjoint.angle);
            //else
            //    Debug.Log("arm at " + (ServerManagerNet.tick + relativeTick) + " angle " + Pistonjoint.angle);
            //Debug.Log("arm");

        }
        //if (mechanicalState.pistonPushArmed == 1)
        //{
        //    Debug.Log(mechanicalState.ticksTillPistonPushActivation + " at tick " + ServerManagerNet.tick + "  combined : " + (ServerManagerNet.tick + relativeTick));
        //}

        //if(payload.pistonPush)
        //    Debug.Log("push");

        if (IsServer)
        {
            //if (payload.pistonPush == true)
            //    Debug.Log("jump");

            //if (payload.direction != 0)
            //    Debug.Log(payload.direction);


            //if (payload.direction != 0 && inputPayloadRBuffer.Read((short)(relativeTick-1)).direction == 0)
            //    Debug.Log("start dir at " + payload.tick);


            //if (payload.direction == 0 && inputPayloadRBuffer.Read((short)(relativeTick - 1)).direction != 0)
            //    Debug.Log("end dir at " + payload.tick);

        }


        if (mechanicalState.ticksTillPistonPushActivation > 50 | mechanicalState.ticksTillPistonPushActivation < 0) Debug.Log("bug222");
        if (mechanicalState.revertCooldown > 120 | mechanicalState.revertCooldown < 0) Debug.Log("bugsss2222");

        if (mechanicalState.revertCooldown == 0)
        {

            setMotorPull();

            ////Debug.Log("revert at index in buffer : " + ((inputPayloadRBuffer.head + relativeTick) % 512) + "   rollback : " + relativeTick + "    local tick : " + ServerManagerNet.tick + "    combined : " + (ServerManagerNet.tick + relativeTick) + "    head : " + inputPayloadRBuffer.head);

            //Debug.Log("revert at " + (ServerManagerNet.tick + relativeTick));
            //if (IsServer) Debug.Log("revert at " + (ServerManagerNet.tick));
            //else Debug.Log("revert at " + (ServerManagerNet.tick + relativeTick));
            //Debug.Log("revert");

            //if (payload.pistonPush == true) Debug.Log("sdsqdsqd");
            //if (ticksTillPistonPushActivation == 0) Debug.Log("adzazd");
        }
        if (mechanicalState.ticksTillPistonPushActivation == 0)
        {
            BodyStateFFI newPistonState = RapierWorld.body_get_state(RapierWorld.world, PistonBody.entityHandle);
            BodyStateFFI cogState = RapierWorld.body_get_state(RapierWorld.world, CogBody.entityHandle);
            newPistonState.x = cogState.x;
            newPistonState.y = cogState.y;
            //Debug.Log(newPistonState.x + "   " + newPistonState.y);
            //newPistonState.velocityX = 0;
            //newPistonState.velocityY = 0;
            //newPistonState.angularVelocity = 0;
            RapierWorld.body_set_state(RapierWorld.world, PistonBody.entityHandle, newPistonState);
            PistonRotate(payload.pistonDirection);
            setMotorPush();
            ////Debug.Log("push at index in buffer : " + ((inputPayloadRBuffer.head + relativeTick) % 512) + "   rollback : " + relativeTick + "    local tick : " + ServerManagerNet.tick + "    combined : " + (ServerManagerNet.tick + relativeTick) + "    head : " + inputPayloadRBuffer.head);
            //Debug.Log("push at " + (ServerManagerNet.tick + relativeTick));

        }

        //PlayerBody.AddTorque((3000 * -payload.direction) - (PlayerBody.angularVelocity * 2f * Mathf.Abs(payload.direction)), ForceMode2D.Force);
        var state = RapierWorld.body_get_state(RapierWorld.world,PlayerBody.entityHandle);
        RapierWorld.AddForce(PlayerBody.entityHandle, Vector2.zero, (60 * -payload.direction) - (state.angularVelocity * 2f * Mathf.Abs(payload.direction)));
        //RapierWorld.AddForce(PlayerBody.entityHandle,Vector2.zero, (300 * -payload.direction) - (0 * 2f * Mathf.Abs(payload.direction)));
        ///if (IsServer) Debug.Log(payload.direction);
    }

    private void setMotorPush()
    {
        //Debug.Log("aaaa");
        //Pistonjoint.motor = PushM;
        RapierWorld.joint_set_prismatic_motor(RapierWorld.world, Pistonjoint.handle, 2.5f, 15000, 160, 20000);
        mechanicalState.activeRevertCooldown = 1;
        mechanicalState.pistonPushArmed = 0;
        mechanicalState.revertCooldown = 120;
        mechanicalState.ticksTillPistonPushActivation = 50;
        mechanicalState.pistonPushOrPull = 1;
    }
    private void setMotorPull()
    {

        //Pistonjoint.motor = PullM;
        RapierWorld.joint_set_prismatic_motor(RapierWorld.world, Pistonjoint.handle, 0, 500, 70, 10000);
        mechanicalState.revertCooldown = 120;
        mechanicalState.activeRevertCooldown = 0;
        mechanicalState.pistonPushOrPull = 0;
    }

    private void PistonRotate(Vector2 dir)
    {
        //Pistonjoint.angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg - 90 - PlayerBody.rotation;
        //Pistonjoint.angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg - 90;
        //RapierWorld.body_set_rotation(RapierWorld.world, CogBody.entityHandle, angle);
        mechanicalState.pistonAngle = -Mathf.Atan2(dir.x, dir.y);
        RapierWorld.body_set_rotation(RapierWorld.world, CogBody.entityHandle, mechanicalState.pistonAngle);
    }
    private void PistonRotate(float angle)
    {
        //Pistonjoint.angle = angle;
        mechanicalState.pistonAngle = angle;
        RapierWorld.body_set_rotation(RapierWorld.world,CogBody.entityHandle, angle);
    }
    public void ProcessInputPayload(InputPayload inputPayload)
    {

        inputPayloadRBuffer.Write(inputPayload);
        inputPayloadRBufferTransmitor.Write(inputPayload);
        SendInputPayloadServerRPC(inputPayloadRBufferTransmitor.GetArray(), inputPayloadRBufferTransmitor.head, ServerManagerNet.tick);
        latestInputsRecivedTick = ServerManagerNet.tick;

        //if (inputPayload.pistonPush) Debug.Log("push sent at " + Time.realtimeSinceStartup);
    }


    ////[ServerRpc(Delivery = RpcDelivery.Unreliable)]
    ////public void testServerRPC()
    ////{
    ////    Debug.Log("b " + Time.realtimeSinceStartup);
    ////    testClientRPC();
    ////}
    ////[ClientRpc(Delivery = RpcDelivery.Unreliable)]
    ////private void testClientRPC()
    ////{
    ////    Debug.Log("c " + Time.realtimeSinceStartup);
    ////}

    /// OPTI : GARBAGE
    /// OPTI : Dont send every frames ? overloads the network and copies anyway
    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendInputPayloadServerRPC(InputPayload[] inputPayloadTransmitor, short transmitorHead,uint tick, ServerRpcParams rpcParams = default)//(FixedList64Bytes<InputPayload> clientInputBatch)
    {
        short i = (short)(tick - latestInputsRecivedTick);
        //Debug.Log(tick);
        //Debug.Log(latestInputsRecivedTick);
        //Debug.Log(transmitorHead);
        if (i <= 0) return;
        if (i > PlayerNet.PayloadTransmiotorRBufferSize)
        {
            /// Pad the inputbuffer with copies of the latest inputpayload knowed 
            var latestInput = inputPayloadRBuffer.Read(0);
            var lostInputs = i - PlayerNet.PayloadTransmiotorRBufferSize;
            Debug.LogError("INPUTS LOSS " + lostInputs);
            for (uint y = 0; y < lostInputs; y++)
            {
                inputPayloadRBuffer.Write(latestInput);
            }
            i = PlayerNet.PayloadTransmiotorRBufferSize;
        }
        while (i > 0)
        {
            i--;
            inputPayloadRBuffer.Write(inputPayloadTransmitor[((transmitorHead - i) + PlayerNet.PayloadTransmiotorRBufferSize)% PlayerNet.PayloadTransmiotorRBufferSize]);
            //if (inputPayloadRBuffer.Read(0).pistonPush) Debug.Log("push recived at " + Time.realtimeSinceStartup);
        }
        latestInputsRecivedTick = tick;

        ulong sender = rpcParams.Receive.SenderClientId;

        //if (inputPayload.pistonPush) Debug.Log("recived at " + ServerManagerNet.tick);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId == sender)
                continue;

            SendOtherPlayersInputPayloadClientRpc(inputPayloadTransmitor, transmitorHead, tick, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new[] { clientId }}});
        }


    }
    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendOtherPlayersInputPayloadClientRpc(InputPayload[] inputPayloadTransmitor, short transmitorHead, uint tick, ClientRpcParams rpcParams)//(FixedList64Bytes<InputPayload> clientInputBatch)
    {
        //if (inputPayload.pistonPush) Debug.Log("recived at " + ServerManagerNet.tick);
        short i = (short)(tick - latestInputsRecivedTick);
        //Debug.Log(tick);
        //Debug.Log(latestInputsRecivedTick);
        //Debug.Log(transmitorHead);
        //if (i <= 0) Debug.Log(tick);
        if (i <= 0) return;
        if (i > PlayerNet.PayloadTransmiotorRBufferSize)
        {
            /// Pad the inputbuffer with copies of the latest inputpayload knowed 
            var latestInput = inputPayloadRBuffer.Read(0);
            var lostInputs = i - PlayerNet.PayloadTransmiotorRBufferSize;
            Debug.LogError("INPUTS LOSS " + lostInputs);
            for (uint y = 0; y < lostInputs; y++)
            {
                inputPayloadRBuffer.Write(latestInput); 
            }
            i = PlayerNet.PayloadTransmiotorRBufferSize;
        }
        while (i > 0)
        {
            i--;
            //Debug.Log(i);
            //Debug.Log(((transmitorHead - i) + PlayerNet.PayloadTransmiotorRBufferSize) % PlayerNet.PayloadTransmiotorRBufferSize);
            inputPayloadRBuffer.Write(inputPayloadTransmitor[((transmitorHead - i)+ PlayerNet.PayloadTransmiotorRBufferSize) % PlayerNet.PayloadTransmiotorRBufferSize]);
            if (inputPayloadRBuffer.Read(0).pistonPush) Debug.Log("push recived at " + Time.realtimeSinceStartup);
        }
        latestInputsRecivedTick = tick;
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





    //public void Bump(Vector2 force)
    //{
    //    PlayerBody.AddForce(force, ForceMode2D.Impulse);
    //}


}
