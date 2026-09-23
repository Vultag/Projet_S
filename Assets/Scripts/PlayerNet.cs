using System;
using NUnit.Framework.Internal;
using Unity.Collections;
using Unity.Netcode;
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

    public PowerUp selectedPowerUp;

    //public byte powerUp1State;
    //public byte powerUp2State;
    //public byte powerUp3State;
    //public byte powerUp4State;
    //public byte powerUp5State;
    //public byte powerUp6State;
    //public byte powerUp7State;
    //public byte powerUp8State;

    public byte PowerUpsStates;

    public float energy;
    public static MechanicsState Default()
    {
        return new MechanicsState
        {
            ticksTillPistonPushActivation = 50,
            revertCooldown = 120,
            activeRevertCooldown = 0,
            pistonPushOrPull = 0,
            pistonPushArmed = 0,
            pistonAngle = 0,
            //PackedPowerUpsStates = 0,
            //activePowerUp = 0,
            energy = 0,
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

        serializer.SerializeValue(ref selectedPowerUp);

        //serializer.SerializeValue(ref powerUp1State);
        //serializer.SerializeValue(ref powerUp2State);
        //serializer.SerializeValue(ref powerUp3State);
        //serializer.SerializeValue(ref powerUp4State);
        //serializer.SerializeValue(ref powerUp5State);
        //serializer.SerializeValue(ref powerUp6State);
        //serializer.SerializeValue(ref powerUp7State);
        //serializer.SerializeValue(ref powerUp8State);

        serializer.SerializeValue(ref PowerUpsStates);

        serializer.SerializeValue(ref energy);
    }
}

public enum Action // *
{
    None,
    Roll,
    Push,
    Aim,
    ChangeSelectedPowerUp,
    ChangePassivePowerUp,
    CancelPowerUp,
    PowerUpAction1,
    PowerUpAction2,
    PowerUpPassif
    //GraplingShoot,
    //GraplingDetatch,
    //PropellingStart,
    //PropellingStop
    /// TO DO
}
public struct InputPayload : INetworkSerializable
{
    public uint tick;

    public Action action1;
    public Action action2;
    public Vector2 action1Delta;
    public Vector2 action2Delta;

    public void Clear()
    {
        action1 = 0;
        action2 = 0;
        action1Delta = Vector2.zero;
        action2Delta = Vector2.zero;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref action1);
        serializer.SerializeValue(ref action2);
        serializer.SerializeValue(ref action1Delta);
        serializer.SerializeValue(ref action2Delta);
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

public class PlayerNet : NetworkBehaviour, IDamageable
{
    public static short PayloadRBufferSize = 512;
    public static short PayloadTransmiotorRBufferSize = 32;
    
    public RapierBody PlayerBody;
    public RapierBody PistonBody;
    public RapierBody CogBody;
    public RapierSliderJoint Pistonjoint;
    //public GameObject graplingHook;
    //public GameObject propeller;
    public SpriteRenderer playerIcon;

    private Vector2 nextPistonPushDirection;
    //private JointMotor2D PushM;
    //private JointMotor2D PullM;

    [HideInInspector]
    public MechanicsState mechanicalState;
    [HideInInspector]
    public CollisionLayer team;
    //[HideInInspector]
    //public PowerUp activePowerUp;

    [HideInInspector]
    public PowerUp[] equipedPowerUpMap = new PowerUp[8];
    [HideInInspector]
    public GameObject[] powerUpsGB;
    [SerializeField]
    private PowerUpInterface[] powerUps;


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


    //[HideInInspector]
    //public NetworkVariable<PowerUps> activePowerup =
    //   new NetworkVariable<PowerUps>(
    //       PowerUps.None,
    //       NetworkVariableReadPermission.Everyone,
    //       NetworkVariableWritePermission.Server
    //   );

    private Player player;
    private ClientPlayer clientPlayer;

    public float health { get; set; } = 100;
    public float ROLLBACKhealth { get; set; }

    private void Start()
    {

        //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");

        GameSyncManager.damageableDatabase.Add(PlayerBody.entityHandle, this);

        /// TEMP -> SETUP IN MENU
        equipedPowerUpMap[0] = PowerUp.GraplinHook;
        equipedPowerUpMap[1] = PowerUp.Propeller;
        equipedPowerUpMap[4] = PowerUp.AutoTurret;


        powerUps = new PowerUpInterface[8];
        powerUpsGB = new GameObject[8];
        for (int i = 0; i < 8; i++)
        {
            switch (equipedPowerUpMap[i])
            {
                case PowerUp.None:
                    break;
                case PowerUp.GraplinHook:

                    GameObject GraplinHookPrefab = Resources.Load<GameObject>("Prefabs/PowerUps/Grapling");
                    GameObject GraplinHookInstance = Instantiate(GraplinHookPrefab, PlayerBody.transform);

                    GraplinHookInstance.GetComponent<Grapling>().hook.playerBody = PlayerBody;

                    powerUpsGB[i] = GraplinHookInstance;
                    if (powerUpsGB[i].GetComponent<PowerUpInterface>() == null) Debug.Log("eeeeeeee");
                    powerUps[i] = powerUpsGB[i].GetComponent<PowerUpInterface>();
                    GraplinHookPrefab = null;

                    break;
                case PowerUp.Propeller:

                    GameObject PropellerPrefab = Resources.Load<GameObject>("Prefabs/PowerUps/Propeller");
                    GameObject PropellerInstance = Instantiate(PropellerPrefab, PlayerBody.transform);

                    PropellerInstance.GetComponent<Propeller>().playerBody = PlayerBody;

                    powerUpsGB[i] = PropellerInstance;
                    powerUps[i] = powerUpsGB[i].GetComponent<PowerUpInterface>();
                    PropellerPrefab = null;

                    break;
                case PowerUp.AutoTurret:

                    GameObject AutoTurretPrefab = Resources.Load<GameObject>("Prefabs/PowerUps/AutoTurret");
                    GameObject AutoTurretInstance = Instantiate(AutoTurretPrefab, PlayerBody.transform);
                    AutoTurretInstance.GetComponent<AutoTurret>().playerBodyHandle = PlayerBody.entityHandle;
                    AutoTurretInstance.GetComponent<AutoTurret>().team = team;

                    powerUpsGB[i] = AutoTurretInstance;
                    powerUps[i] = powerUpsGB[i].GetComponent<PowerUpInterface>();
                    AutoTurretPrefab = null;

                    break;
            }
        }
        Resources.UnloadUnusedAssets();
        GameSyncManager.GameSyncSave();
        ////RapierWorld.world_store_snapshot(RapierWorld.world);
        ////RapierToUnityDatabase.ROLLBACKsave();
    }

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();
        clientPlayer = GetComponent<ClientPlayer>();
        mechanicalState = MechanicsState.Default();

        latestSyncedMechanicsStatePayload = MechanicsState.Default();
        latestServerStatePayload.playerMechanicsState = MechanicsState.Default();

        ///graplingHook = powerUps[(int)PowerUp.GraplinHook-1].ga.GetComponent<Grapling>();

        //var ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);

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
        }
        else
        {
            Destroy(player);
            clientPlayer.enabled = true;
        }


        //graplingHookActive.OnValueChanged += (_, v) => UpdatePowerupClientRpc(PowerUps.GraplinHook, v);
        //propellerActive.OnValueChanged += (_, v) => UpdatePowerupClientRpc(PowerUps.Propeller, v);
    }

    public void UpdateSyncedStates()
    {
        for (int i = 0; i < 8; i++)
        {
            /// temp
            if (powerUps[i] == null) continue;
            powerUps[i].SavePowerUpState();
        }
        latestSyncedMechanicsStatePayload = latestServerStatePayload.playerMechanicsState;
    }
    public void RestoreState(MechanicsState mechState)
    {

        //if (mechanicalState.activePowerUp != mechState.activePowerUp)
        //    ProcessAction(Action.ChangePowerUp,new Vector2((int)mechState.activePowerUp, 0));

        mechanicalState.ticksTillPistonPushActivation = mechState.ticksTillPistonPushActivation;
        mechanicalState.activeRevertCooldown = mechState.activeRevertCooldown;
        mechanicalState.revertCooldown = mechState.revertCooldown;
        ////Pistonjoint.motor = latestServerStatePayload.pistonPushOrPull ? PushM : PullM;
        ///PistonRotate(mechState.pistonAngle);
        mechanicalState.pistonAngle = mechState.pistonAngle;
        mechanicalState.pistonPushOrPull = mechState.pistonPushOrPull;
        mechanicalState.pistonPushArmed = mechState.pistonPushArmed;
        mechanicalState.selectedPowerUp = mechState.selectedPowerUp;
        mechanicalState.energy = mechState.energy;
        mechanicalState.PowerUpsStates = mechState.PowerUpsStates;

        for (int i = 0; i < 8; i++)
        {
            if (powerUps[i] == null) continue;
            powerUps[i].RestorePowerUpState(((mechState.PowerUpsStates >> i) & 1) == 1);
        }

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

        ///RestoreState(State.playerMechanicsState);
    }

    private void ProcessAction(Action action, Vector2 delta)
    {
        int activePowerUp = (int)mechanicalState.selectedPowerUp - 1;
        switch (action)
        {
            case Action.None:
                break;
            case Action.Roll:
                var state = RapierWorld.body_get_state(RapierWorld.world, PlayerBody.entityHandle);
                int direction = Math.Sign(delta.x);
                RapierWorld.AddForce(PlayerBody.entityHandle, Vector2.zero, (60 * -direction) - (state.angularVelocity * 2f * Mathf.Abs(-direction)));
                break;
            case Action.Push:
                mechanicalState.pistonPushArmed = 1;
                nextPistonPushDirection = delta;
                break;
            case Action.ChangeSelectedPowerUp:

                PowerUp newPowerUp = (PowerUp)delta.x;
                if (mechanicalState.selectedPowerUp != 0)
                {
                    powerUps[activePowerUp].PowerUpSelection(false);
                }
                if (newPowerUp != 0)
                {
                    powerUps[(int)newPowerUp - 1].PowerUpSelection(true);
                }
                mechanicalState.selectedPowerUp = newPowerUp;

                break;
            case Action.ChangePassivePowerUp:
                int powerUpIdx = (int)delta.x;
                int newPowerUpState = ((mechanicalState.PowerUpsStates >> powerUpIdx) & 1)^1;
                powerUps[powerUpIdx].PowerUpSelection(newPowerUpState == 1);
                mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates & ~(1 << powerUpIdx) | (newPowerUpState << powerUpIdx));
                break;
            case Action.CancelPowerUp:

                powerUps[(int)delta.x - 1].PowerUpAction2(Vector2.zero);
                mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates & ~(1 << (byte)delta.x - 1));

                break;
            case Action.Aim:
                if(mechanicalState.selectedPowerUp>0)
                    powerUps[activePowerUp].PowerUpAim(delta);
                break;
            case Action.PowerUpAction1:
                powerUps[activePowerUp].PowerUpAction1(delta);
                mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates | (1 << (byte)activePowerUp));
                break;

            case Action.PowerUpAction2:
                powerUps[activePowerUp].PowerUpAction2(delta);
                mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates & ~(1 << (byte)activePowerUp));
                break;
            case Action.PowerUpPassif:
                powerUps[(int)delta.x].PowerUpAction1(Vector2.zero);
                break;
                //case Action.GraplingShoot:
                //    powerUps[(int)PowerUp.GraplinHook-1].PowerUpAction1(delta);
                //    mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates | (1 << (byte)PowerUp.GraplinHook-1));
                //    break;
                //case Action.GraplingDetatch:
                //    powerUps[(int)PowerUp.GraplinHook-1].PowerUpAction2(delta);
                //    mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates & ~(1 << (byte)PowerUp.GraplinHook-1));
                //    break;
                //case Action.PropellingStart:
                //    powerUps[(int)PowerUp.Propeller - 1].PowerUpAction1(delta);
                //    break;
                //case Action.PropellingStop:
                //    powerUps[(int)PowerUp.Propeller - 1].PowerUpAction2(delta);
                //    break;
        }

    }

    public void Tick(short relativeTick)
    {
        InputPayload payload = inputPayloadRBuffer.Read(relativeTick);

        //mechanicalState.energy = Mathf.Min(mechanicalState.energy + 0.25f,100);
        mechanicalState.energy = Mathf.Max(mechanicalState.energy - 0.1f, 0);

        ProcessAction(payload.action1, payload.action1Delta);
        ProcessAction(payload.action2, payload.action2Delta);

        mechanicalState.revertCooldown = (byte)(mechanicalState.revertCooldown - mechanicalState.activeRevertCooldown);
        mechanicalState.ticksTillPistonPushActivation = (byte)(mechanicalState.ticksTillPistonPushActivation - mechanicalState.pistonPushArmed);


        if (mechanicalState.ticksTillPistonPushActivation > 50 | mechanicalState.ticksTillPistonPushActivation < 0) Debug.Log("bug222");
        if (mechanicalState.revertCooldown > 120 | mechanicalState.revertCooldown < 0) Debug.Log("bugsss2222");

        if (mechanicalState.revertCooldown == 0)
        {

            setMotorPull();

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
            PistonRotate(nextPistonPushDirection);
            setMotorPush();
            ////Debug.Log("push at index in buffer : " + ((inputPayloadRBuffer.head + relativeTick) % 512) + "   rollback : " + relativeTick + "    local tick : " + ServerManagerNet.tick + "    combined : " + (ServerManagerNet.tick + relativeTick) + "    head : " + inputPayloadRBuffer.head);
            //Debug.Log("push at " + (ServerManagerNet.tick + relativeTick));

        }

        //PlayerBody.AddTorque((3000 * -payload.direction) - (PlayerBody.angularVelocity * 2f * Mathf.Abs(payload.direction)), ForceMode2D.Force);
        //RapierWorld.AddForce(PlayerBody.entityHandle,Vector2.zero, (300 * -payload.direction) - (0 * 2f * Mathf.Abs(payload.direction)));
        ///if (IsServer) Debug.Log(payload.direction);
    }

    public void ConsumePowerupEnergy(float quantity)
    {

        mechanicalState.energy-=quantity;
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
            //Debug.Log(((transmitorHead - i) + PlayerNet.PayloadTransmiotorRBufferSize) % PlayerNet.PayloadTransmiotorRBufferSize);
            inputPayloadRBuffer.Write(inputPayloadTransmitor[((transmitorHead - i)+ PlayerNet.PayloadTransmiotorRBufferSize) % PlayerNet.PayloadTransmiotorRBufferSize]);
            //if (inputPayloadRBuffer.Read(0).pistonPush) Debug.Log("push recived at " + Time.realtimeSinceStartup);
        }
        latestInputsRecivedTick = tick;
    }

    public void Die()
    {
        //Debug.Log(health);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        //Debug.Log(health);
        if (health <= 0)
            Die();
    }




    //public void Bump(Vector2 force)
    //{
    //    PlayerBody.AddForce(force, ForceMode2D.Impulse);
    //}


}
