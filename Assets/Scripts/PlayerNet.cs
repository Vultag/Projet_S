using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

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

    public int selectedPowerUpIdx;
    //public PowerUp selectedPowerUp;

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
            selectedPowerUpIdx = -1,
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

        serializer.SerializeValue(ref selectedPowerUpIdx);

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
    //CancelPowerUp,
    PowerUpAction1,
    PowerUpAction2,
    PowerUpPassifAction1,
    PowerUpPassifAction2
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
    public RapierHingeJoint Cogjoint;
    //public GameObject graplingHook;
    //public GameObject propeller;
    public SpriteRenderer playerIcon;

    private Vector2 nextPistonPushDirection;
    public Vector2 nextPistonPushDirectionRollback;
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
    ////[HideInInspector]
    ////public Dictionary<PowerUp,int> equipedPowerUpSlotMap = new Dictionary<PowerUp, int>(8);
    [HideInInspector]
    public GameObject[] powerUpsGB;
    private PowerUpInterface[] powerUps;
    [HideInInspector]
    public int[] weaponsAmmo = new int[8];
    private int[] weaponsAmmoROLLBACK = new int[8];


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

    public uint syncedUIstatesBitmask;
    public Queue<InputPayload> anteriorConfigurationActionQueue;

    public float health { get; set; } = 100;
    public float ROLLBACKhealth { get; set; }

    private void Start()
    {


        //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");

        GameSyncManager.damageableDatabase.Add(PlayerBody.entityHandle, this);
        GameSyncManager.damageableDatabase.Add(PistonBody.entityHandle, this);
        mechanicalState = MechanicsState.Default();
        latestSyncedMechanicsStatePayload = MechanicsState.Default();
        latestServerStatePayload.playerMechanicsState = MechanicsState.Default();

        //Debug.Log("initiate at tick "+ ServerManagerNet.tick + team);

        /// TEMP -> SETUP IN MENU
        equipedPowerUpMap[0] = PowerUp.GraplinHook;
        equipedPowerUpMap[1] = PowerUp.Propeller;
        equipedPowerUpMap[4] = PowerUp.Rifle;
        equipedPowerUpMap[5] = PowerUp.AutoTurret;

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
                    GraplinHookInstance.GetComponent<Grapling>().player = this;

                    powerUpsGB[i] = GraplinHookInstance;
                    if (powerUpsGB[i].GetComponent<PowerUpInterface>() == null) Debug.Log("eeeeeeee");
                    powerUps[i] = powerUpsGB[i].GetComponent<PowerUpInterface>();
                    GraplinHookPrefab = null;

                    break;
                case PowerUp.Propeller:

                    GameObject PropellerPrefab = Resources.Load<GameObject>("Prefabs/PowerUps/Propeller");
                    GameObject PropellerInstance = Instantiate(PropellerPrefab, PlayerBody.transform);

                    PropellerInstance.GetComponent<Propeller>().player = this;

                    powerUpsGB[i] = PropellerInstance;
                    powerUps[i] = powerUpsGB[i].GetComponent<PowerUpInterface>();
                    PropellerPrefab = null;

                    break;
                case PowerUp.AutoTurret:

                    GameObject AutoTurretPrefab = Resources.Load<GameObject>("Prefabs/PowerUps/AutoTurret");
                    GameObject AutoTurretInstance = Instantiate(AutoTurretPrefab, PlayerBody.transform);

                    AutoTurret autoTurret = AutoTurretInstance.GetComponent<AutoTurret>();
                    autoTurret.player = this;
                    autoTurret.equipmentIdx = i;
                    autoTurret.team = team;

                    powerUpsGB[i] = AutoTurretInstance;
                    powerUps[i] = powerUpsGB[i].GetComponent<PowerUpInterface>();
                    weaponsAmmo[i] = 10;
                    syncedUIstatesBitmask |= 1u << (i+16);
                    
                    AutoTurretPrefab = null;

                    break;
                case PowerUp.Rifle:

                    GameObject RiflePrefab = Resources.Load<GameObject>("Prefabs/PowerUps/Rifle");
                    GameObject RifleInstance = Instantiate(RiflePrefab, PlayerBody.transform);

                    Rifle rifle = RifleInstance.GetComponent<Rifle>();
                    rifle.player = this;
                    rifle.equipmentIdx = i;
                    rifle.team = team;

                    powerUpsGB[i] = RifleInstance;
                    if (powerUpsGB[i].GetComponent<PowerUpInterface>() == null) Debug.Log("eeeeeeee");
                    powerUps[i] = powerUpsGB[i].GetComponent<PowerUpInterface>();
                    weaponsAmmo[i] = 6;
                    syncedUIstatesBitmask |= 1u << (i + 16);

                    RiflePrefab = null;

                    break;
            }
        }
        Resources.UnloadUnusedAssets();
        var servernet = FindFirstObjectByType<ServerManagerNet>(FindObjectsInactive.Include).GetComponent<ServerManagerNet>();

        GameSyncManager.Players.Add(this);

        //if(NetworkManager.Singleton.ConnectedClientsIds.Count == GameSyncManager.Players.Count)
        servernet.PromoteTickAsSynced();

        if (IsServer)
        {
            Destroy(player);
            Destroy(clientPlayer);
        }
        else if (IsOwner)
        {
            player.enabled = true;
            Destroy(clientPlayer);
        }
        else
        {
            Destroy(player);
            clientPlayer.enabled = true;
        }
    }

    public override void OnNetworkSpawn()
    {

        player = GetComponent<Player>();
        clientPlayer = GetComponent<ClientPlayer>();

        inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);
        inputPayloadRBufferTransmitor = new RingBuffer<InputPayload>(PlayerNet.PayloadTransmiotorRBufferSize);

        var queueSize = player != null ? 2 : 64;
        anteriorConfigurationActionQueue = new Queue<InputPayload>(queueSize);


        PlayerBody.gameObject.SetActive(true);
        CogBody.gameObject.SetActive(true);
        PistonBody.gameObject.SetActive(true);

        ///GameSyncManager.GameSyncSave();

    }

    public void UpdateSyncedStates()
    {
        /// Find better place for this ?
        nextPistonPushDirectionRollback = nextPistonPushDirection;

        for (int i = 0; i < 8; i++)
        {
            /// temp
            if (powerUps[i] == null) continue;
            powerUps[i].SavePowerUpState();
        }
        Array.Copy(weaponsAmmo, weaponsAmmoROLLBACK, 8);
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
        mechanicalState.selectedPowerUpIdx = mechState.selectedPowerUpIdx;
        //if (mechState.selectedPowerUpIdx < 0) Debug.Log(mechState.selectedPowerUpIdx);

        mechanicalState.energy = mechState.energy;
        mechanicalState.PowerUpsStates = mechState.PowerUpsStates;

        nextPistonPushDirection = nextPistonPushDirectionRollback;

        /// opti
        for (int i = 0; i < 8; i++)
        {
            if (powerUps[i] == null) continue;
            powerUps[i].RestorePowerUpState(((mechState.PowerUpsStates >> i) & 1) == 1);
        }
        Array.Copy(weaponsAmmoROLLBACK, weaponsAmmo, 8);

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
        ///
        //var tempa = RapierWorld.body_get_state(RapierWorld.world, PlayerBody.entityHandle);
        //var tempb = RapierWorld.body_get_state(RapierWorld.world, PistonBody.entityHandle);
        //var tempc = RapierWorld.body_get_state(RapierWorld.world, CogBody.entityHandle);

        //RapierWorld.body_set_state(RapierWorld.world, PlayerBody.entityHandle, tempa);
        //RapierWorld.body_set_state(RapierWorld.world, PistonBody.entityHandle, tempb);
        //RapierWorld.body_set_state(RapierWorld.world, CogBody.entityHandle, tempc);

        ///RestoreState(State.playerMechanicsState);
    }

    #region Action Processing
    public void ProcessAnteriorConfigurationAction(Action configurationAction, Vector2 delta)
    {
        //int activePowerUpZeroedIdx = (int)mechanicalState.selectedPowerUpZeroedIdx;
        switch (configurationAction)
        {
            case Action.ChangeSelectedPowerUp:

                if (mechanicalState.selectedPowerUpIdx >= 0)
                {
                    powerUps[mechanicalState.selectedPowerUpIdx].PowerUpSelection(false);
                }
                if (delta.x >= 0)
                {
                    powerUps[(int)delta.x].PowerUpSelection(true);
                }
                mechanicalState.selectedPowerUpIdx = (int)delta.x;
                syncedUIstatesBitmask |= (1u << (int)(delta.x));

                break;
            case Action.ChangePassivePowerUp:
                int powerUpIdx = (int)delta.x;
                uint newPowerUpState = (uint)((mechanicalState.PowerUpsStates >> powerUpIdx) & 1)^1;
                powerUps[powerUpIdx].PowerUpSelection(newPowerUpState == 1);
                mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates & ~(1u << powerUpIdx) | (newPowerUpState << powerUpIdx));
                syncedUIstatesBitmask = (uint)(syncedUIstatesBitmask & ~(1u << powerUpIdx) | (newPowerUpState << powerUpIdx));

                break;
        }

    }
    private void ProcessAction(Action action, Vector2 delta)
    {
        switch (action)
        {
            case Action.None:
                break;
            case Action.Roll:
                var state = RapierWorld.body_get_state(RapierWorld.world, PlayerBody.entityHandle);
                int direction = Math.Sign(delta.x);
                RapierWorld.body_add_force(
                    RapierWorld.world,
                    PlayerBody.entityHandle,
                    0,
                    0,
                    (60 * -direction) - (state.angularVelocity * 2f * Mathf.Abs(-direction))
                );
                break;
            case Action.Push:
                mechanicalState.pistonPushArmed = 1;
                nextPistonPushDirection = delta;
                break;
            case Action.ChangeSelectedPowerUp:

                if (mechanicalState.selectedPowerUpIdx >= 0)
                {
                    powerUps[mechanicalState.selectedPowerUpIdx].PowerUpSelection(false);
                }
                if (delta.x >= 0)
                {
                    powerUps[(int)delta.x].PowerUpSelection(true);
                }
                mechanicalState.selectedPowerUpIdx = (int)delta.x;
                syncedUIstatesBitmask |= (1u << (int)(delta.x));

                break;
            case Action.ChangePassivePowerUp:
                int powerUpIdx = (int)delta.x;
                uint newPowerUpState = (uint)((mechanicalState.PowerUpsStates >> powerUpIdx) & 1)^1;
                powerUps[powerUpIdx].PowerUpSelection(newPowerUpState == 1);
                mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates & ~(1u << powerUpIdx) | (newPowerUpState << powerUpIdx));
                syncedUIstatesBitmask = (uint)(syncedUIstatesBitmask & ~(1u << powerUpIdx) | (newPowerUpState << powerUpIdx));

                break;
            case Action.Aim:
                /// OPTI: useless condition ?
                if(mechanicalState.selectedPowerUpIdx >= 0)
                {
                    powerUps[mechanicalState.selectedPowerUpIdx].PowerUpAim(delta);
                }
                break;
            case Action.PowerUpAction1:
                if (mechanicalState.selectedPowerUpIdx < 0) Debug.LogError(mechanicalState.selectedPowerUpIdx);
                powerUps[mechanicalState.selectedPowerUpIdx].PowerUpAction1(delta);

                break;

            case Action.PowerUpAction2:
                if (mechanicalState.selectedPowerUpIdx < 0) Debug.LogError(mechanicalState.selectedPowerUpIdx);
                powerUps[mechanicalState.selectedPowerUpIdx].PowerUpAction2(delta);
                mechanicalState.PowerUpsStates = (byte)(mechanicalState.PowerUpsStates & ~(1 << mechanicalState.selectedPowerUpIdx));
                break;
            case Action.PowerUpPassifAction1:
                powerUps[(int)delta.x].PowerUpAction1(Vector2.zero);
                break;
            case Action.PowerUpPassifAction2:
                powerUps[(int)delta.x].PowerUpAction2(Vector2.zero);
                break;
        }

    }
    #endregion

    public void Tick(short relativeTick)
    {
        InputPayload payload = inputPayloadRBuffer.Read(relativeTick);

        mechanicalState.energy = Mathf.Min(mechanicalState.energy + 0.08f*50, 100);

        /// OPTI ?
        for (int i = 0; i < 8; i++)
        {
            if (powerUps[i] == null) continue;

            powerUps[i].Tick(((mechanicalState.PowerUpsStates>>i)&1) ==1);
        }

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
            //if(GetComponent<Player>() != null)
            //{
            //    RapierWorld.body_set_rotation(RapierWorld.world, PlayerBody.entityHandle, 0);
            //}

            BodyStateFFI newPistonState = RapierWorld.body_get_state(RapierWorld.world, PistonBody.entityHandle);
            BodyStateFFI cogState = RapierWorld.body_get_state(RapierWorld.world, CogBody.entityHandle);
            newPistonState.x = cogState.x;
            newPistonState.y = cogState.y;

            //RapierWorld.body_set_state(RapierWorld.world, PistonBody.entityHandle, newPistonState);
            setMotorPush();
            PistonRotate(nextPistonPushDirection);
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
        RapierWorld.joint_set_prismatic_motor(RapierWorld.world, Pistonjoint.handle, 2.5f, 15000, 160, 20000);
        //Debug.Log("aaaa");
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
        var playerState = RapierWorld.body_get_state(RapierWorld.world,PlayerBody.entityHandle);
        var pistonState = RapierWorld.body_get_state(RapierWorld.world, PistonBody.entityHandle);
        RapierWorld.body_set_state(RapierWorld.world, PistonBody.entityHandle, new BodyStateFFI
        {
            x = playerState.x,
            y = playerState.y,
            velocityX = 0,
            velocityY = 0,
            angularVelocity = 0,
            rotation = mechanicalState.pistonAngle,
        });
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
            Debug.LogError(" will require resync before resuming because of action order breaking ");
            for (uint y = 0; y < lostInputs; y++)
            {
                inputPayloadRBuffer.Write(latestInput);
            }
            i = PlayerNet.PayloadTransmiotorRBufferSize;
        }
        while (i > 0)
        {
            i--;
            InputPayload inputs = inputPayloadTransmitor[((transmitorHead - i) + PlayerNet.PayloadTransmiotorRBufferSize) % PlayerNet.PayloadTransmiotorRBufferSize];
            inputPayloadRBuffer.Write(inputs);
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
            Debug.LogError(" will require resync before resuming because of action order breaking ");
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
            InputPayload inputs = inputPayloadTransmitor[((transmitorHead - i) + PlayerNet.PayloadTransmiotorRBufferSize) % PlayerNet.PayloadTransmiotorRBufferSize];
            inputPayloadRBuffer.Write(inputs);
            anteriorConfigurationActionQueue.Enqueue(inputs);
            //if (inputPayloadRBuffer.Read(0).action1 == Action.Push) Debug.Log("push recived at " + Time.realtimeSinceStartup);
        }
        latestInputsRecivedTick = tick;
    }

    public void Die()
    {

        //Debug.Log("die");
        if (IsOwner)
            player.ui.RespawnSwitch(false);
        else if(IsClient)
            clientPlayer.gameObject.SetActive(false);

        if (mechanicalState.selectedPowerUpIdx >= 0)
        {
            powerUps[mechanicalState.selectedPowerUpIdx].PowerUpSelection(false);
        }
        for (int i = 0; i < 8; i++)
        {
            syncedUIstatesBitmask &= ~(1u << i);
            syncedUIstatesBitmask &= ~(1u << (i+8));
            if (powerUps[i] == null) continue;
            powerUps[i].PowerUpDisable();
        }
        mechanicalState.PowerUpsStates = 0;
        mechanicalState.selectedPowerUpIdx = -1;


        RapierWorld.joint_set_enabled(RapierWorld.world, Pistonjoint.handle, false);
        RapierWorld.joint_set_enabled(RapierWorld.world, Cogjoint.handle, false);
        RapierWorld.body_set_enabled(RapierWorld.world,PistonBody.entityHandle,false);
        RapierWorld.body_set_enabled(RapierWorld.world, PlayerBody.entityHandle, false);
        RapierWorld.body_set_enabled(RapierWorld.world, CogBody.entityHandle, false);
        PistonBody.gameObject.SetActive(false);
        PlayerBody.gameObject.SetActive(false);

        GameSyncManager.gameManager.map.playerSpawner.InitiateRespawn(this);
    }

    public void Spawn(Vector3 pos,Vector2 force)
    {
        BodyStateFFI spawnState = new BodyStateFFI
        {
            x = pos.x,
            y = pos.y
        };

        RapierWorld.body_set_state(RapierWorld.world, PistonBody.entityHandle, spawnState);
        RapierWorld.body_set_state(RapierWorld.world, PlayerBody.entityHandle, spawnState);
        RapierWorld.body_set_state(RapierWorld.world, CogBody.entityHandle, spawnState);

        RapierWorld.joint_set_enabled(RapierWorld.world, Pistonjoint.handle, true);
        RapierWorld.joint_set_enabled(RapierWorld.world, Cogjoint.handle, true);
        RapierWorld.body_set_enabled(RapierWorld.world, PistonBody.entityHandle, true);
        RapierWorld.body_set_enabled(RapierWorld.world, PlayerBody.entityHandle, true);
        RapierWorld.body_set_enabled(RapierWorld.world, CogBody.entityHandle, true);
        PistonBody.gameObject.SetActive(true);
        PlayerBody.gameObject.SetActive(true);
        health = 100;

        RapierWorld.body_add_force(
               RapierWorld.world,
               player.PlayerBody.entityHandle,
               force.x,
               force.y,
               0
           );

        if (IsOwner)
            player.ui.RespawnSwitch(true);
        else if (IsClient)
            clientPlayer.gameObject.SetActive(true);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        //Debug.Log(health);
        if (health <= 0)
            Die();
    }

    public void AmmoPickUp()
    {
        for (int i = 0; i < 8; i++)
        {
            switch (equipedPowerUpMap[i])
            {
                case PowerUp.AutoTurret:
                    weaponsAmmo[i] += 5;
                    syncedUIstatesBitmask |= 1u << (i + 16);
                    break;
                case PowerUp.Rifle:
                    weaponsAmmo[i] += 3;
                    syncedUIstatesBitmask |= 1u << (i + 16);
                    break;
                default:
                    break;
            }
        }

    }


    //public void Bump(Vector2 force)
    //{
    //    PlayerBody.AddForce(force, ForceMode2D.Impulse);
    //}


}
