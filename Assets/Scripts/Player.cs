using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public UI ui;
    private ServerManagerNet serverManagerNet;

    private PlayerNet playerNet;
    //private uint tick;

    //public SliderJoint2D Pistonjoint;
    //public Rigidbody2D PlayerBody;
    public RapierSliderJoint Pistonjoint;
    public RapierBody PlayerBody;

    public InputActionAsset PlayerInputs;

    private InputAction mouvementActivateInput;
    private InputAction mouvementDirectionInput;
    private InputAction finger1Delta;
    private InputAction finger1Press;
    private InputAction finger2Delta;
    private InputAction finger2Press;

    //[HideInInspector]
    //public sbyte activeDirection;

    [HideInInspector]
    public Vector2 respawnPoint;

    [SerializeField]
    private GameObject leftjoystickParent;
    [SerializeField]
    private GameObject rightjoystickParent;

    private Vector2 finger1HeldDelta;
    private Vector2 finger2HeldDelta;

    private byte LeftJoySFingerIdx;
    private byte RightJoySFingerIdx;

    private Vector2 rightJumpForceArmed;
    private Vector2 leftJumpForceArmed;

    [HideInInspector]
    public InputPayload activeInputPayload;
    [HideInInspector]
    public byte actionNumThisFrame;
    [HideInInspector]
    public float rollDirection;

    [SerializeField]
    private GameObject hookGB;


    private GameManager gameManager;

    //public void syncTick(uint atTick)
    //{
    //    ServerManagerNet.tick = atTick;
    //    //Debug.Log("sync " + atTick);
    //}


    private void Start()
    {

        //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");

        playerNet = GetComponent<PlayerNet>();
        //Debug.Log(playerNet.NetworkObjectId);


        ////playerNet.inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);
        ////playerNet.inputPayloadRBufferTransmitor = new RingBuffer<InputPayload>(PlayerNet.PayloadTransmiotorRBufferSize);

        //for (int i = 0; i < PlayerNet.PayloadTransmiotorRBufferSize; i++)
        //{
        //    playerNet.inputPayloadRBuffer.Write(new InputPayload(0));
        //}

        var ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);
        ui.player = this;
        ui.playerNet = GetComponent<PlayerNet>();
        //var hookUiLink =hookGB.AddComponent<HookUiLink>();
        //hookUiLink.UiButton = ui.hookDisableButton;

        gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);

        Camera.main.GetComponent<TrackPlayer>().PlayerGB = playerNet.PlayerBody.gameObject;
        Camera.main.GetComponent<TrackPlayer>().enabled = true;

        ui.gameObject.SetActive(true);

        serverManagerNet = FindFirstObjectByType<ServerManagerNet>(FindObjectsInactive.Include).GetComponent<ServerManagerNet>();
        //serverManagerNet.PromoteTickAsSynced();

        //GameSyncManager.GameSyncSave();
        //GameSyncManager.GameSyncSave();
        ///activeInputPayload = InputPayload.Default(0);

        // RapierWorld.PhysicsStep(1/60f);

    }

    private void FixedUpdate()
    {
        //if (ServerManagerNet.tick == 100) ArmJumping(Vector2.down);

        activeInputPayload.tick = ServerManagerNet.tick;

        if(rollDirection !=0)
            RegisterAction(Action.Roll, new Vector2(rollDirection, 0));

        playerNet.ProcessInputPayload(activeInputPayload);


        serverManagerNet.Reconciliation();


        if (actionNumThisFrame > 2)
        {
            Debug.LogWarning("more tow action this frame");
        }

        gameManager.Tick(ServerManagerNet.tick);

        playerNet.Tick(0);

        RapierWorld.PhysicsStep(PlayerNet.gameFixedDeltaTime);


        /// Debug purpose for now
        //playerNet.statePayloadRBuffer.Write(new StatePayload
        //{
        //    tick = ServerManagerNet.tick,
        //    playerPhyState = new PhysicsState
        //    {
        //        position = PlayerBody.position,
        //        rotation = PlayerBody.rotation,
        //        linearVelocity = PlayerBody.linearVelocity,
        //        angularVelocity = PlayerBody.angularVelocity,
        //    },
        //    pistonPhyState = new PhysicsState
        //    {
        //        position = playerNet.PistonBody.position,
        //        rotation = playerNet.PistonBody.rotation,
        //        linearVelocity = playerNet.PistonBody.linearVelocity,
        //        angularVelocity = playerNet.PistonBody.angularVelocity,
        //    },
        //    cogPhyState = new PhysicsState
        //    {
        //        position = playerNet.CogBody.position,
        //        rotation = playerNet.CogBody.rotation,
        //        linearVelocity = playerNet.CogBody.linearVelocity,
        //        angularVelocity = playerNet.CogBody.angularVelocity,
        //    },
        //    ticksTillPistonPushActivation = playerNet.ticksTillPistonPushActivation, 
        //    activeRevertCooldown = playerNet.activeRevertCooldown,
        //    revertCooldown = playerNet.revertCooldown,
        //    pistonPushOrPull = playerNet.pistonPushOrPull,
        //    pistonAngle = Pistonjoint.angle,
        //    pistonPushArmed = playerNet.pistonPushArmed == 1 ? true : false,
        //});

        ServerManagerNet.tick++;
        activeInputPayload.Clear();
        actionNumThisFrame = 0;
    }

    public void RegisterAction(Action action, Vector2 delta)
    {
        switch (actionNumThisFrame)
        {
            case 0:
                activeInputPayload.action1 = action;
                activeInputPayload.action1Delta = delta;
                break;
            case 1:
                activeInputPayload.action2 = action;
                activeInputPayload.action2Delta = delta;
                break;
        }
        actionNumThisFrame++;

    }

    public void ArmJumping(Vector2 dir)
    {
        RegisterAction(Action.Push, dir);
    }
    //[ServerRpc]
    //public void AddPlayerForceServerRpc(Vector2 force)
    //{
    //    PlayerBody.AddForce(force, ForceMode2D.Force);
    //}
    //[ServerRpc]
    //public void PistonPushServerRpc(Vector2 dir)
    //{
    //    //Pistonjoint.angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg - 90 - PlayerBody.rotation;
    //    Pistonjoint.angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg - 90;
    //    Pistonjoint.motor = PushM;
    //}
    //[ServerRpc]
    //public void PistonPullServerRpc()
    //{
    //    Pistonjoint.motor = PullM;
    //}


    public void Respawn()
    {
        //if(IsOwner)
        //    ui.GetComponent<PowerupManager>().ActivatePowerup((int)ui.GetComponent<PowerupManager>().activePowerup);
        //var pistonBody = Pistonjoint.GetComponent<Rigidbody2D>();
        //PlayerBody.position = respawnPoint;
        //PlayerBody.angularVelocity = 0;
        //PlayerBody.linearVelocity = Vector2.zero;

        //pistonBody.position = respawnPoint;
        //pistonBody.angularVelocity = 0;
        //pistonBody.linearVelocity = Vector2.zero;

    }
    //[ClientRpc]
    //public void RespawnClientRpc()
    //{
    //    ui.GetComponent<PowerupManager>().ActivatePowerup(0);
    //}
  


    //public void UpdatePowerupWithNetwork(PowerUps powerup, bool state)
    //{
    //    UpdatePowerupServerRpc(powerup, state);
    //    UpdatePowerupLocal(powerup, state);
    //}
    //public void UpdatePowerupLocal(PowerUps powerup)
    //{
    //    switch (powerupManager.activePowerup)
    //    {
    //        case PowerUps.GraplinHook:
    //            graplingHook.SetActive(false);
    //            break;
    //        case PowerUps.Propeller:
    //            propeller.SetActive(false);
    //            break;



    //        default:
    //            break;
    //    }

    //    /// handled beforehand
    //    //if (powerupManager.activePowerup == powerup) return;

    //    switch (powerup)
    //    {
    //        case PowerUps.GraplinHook:
    //            graplingHook.SetActive(true);
    //            break;
    //        case PowerUps.Propeller:
    //            propeller.SetActive(true);
    //            break;



    //        default:
    //            break;
    //    }
    //}


}
