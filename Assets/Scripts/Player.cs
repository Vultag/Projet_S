using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public UI ui;
    private PowerupManager powerupManager;
    private ServerManagerNet serverManagerNet;

    private PlayerNet playerNet;
    private uint tick;

    public SliderJoint2D Pistonjoint;
    public Rigidbody2D PlayerBody;

    public InputActionAsset PlayerInputs;

    private InputAction mouvementActivateInput;
    private InputAction mouvementDirectionInput;
    private InputAction finger1Delta;
    private InputAction finger1Press;
    private InputAction finger2Delta;
    private InputAction finger2Press;

    [HideInInspector]
    public sbyte activeDirection;

    [HideInInspector]
    public Vector2 respawnPoint;
    public GameObject graplingHook;
    public GameObject propeller;

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

    private InputPayload activeInputPayload;

    public void syncTick(uint atTick)
    {
        tick = atTick;
    }

    private void Start()
    {
        //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");

        playerNet = GetComponent<PlayerNet>();
        //Debug.Log(playerNet.NetworkObjectId);

        playerNet.statePayloadRBuffer = new RingBuffer<StatePayload>(PlayerNet.PayloadRBufferSize);
        playerNet.inputPayloadRBuffer = new RingBuffer<InputPayload>(PlayerNet.PayloadRBufferSize);

        var propellerS = propeller.GetComponent<Propeller>();
        var graplingS = graplingHook.GetComponent<Grapling>();
        /// remplace by active powerup from manager network var
        propellerS.enabled = true;
        graplingS.enabled = true;

        var ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);
        powerupManager = ui.GetComponent<PowerupManager>();
        ui.player = this;
        ui.playerNet = GetComponent<PlayerNet>();

        Camera.main.GetComponent<TrackPlayer>().PlayerGB = playerNet.PlayerBody.gameObject;
        Camera.main.GetComponent<TrackPlayer>().enabled = true;

        ui.gameObject.SetActive(true);
        var gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        gameManager.gameObject.SetActive(true);

        serverManagerNet = FindFirstObjectByType<ServerManagerNet>(FindObjectsInactive.Include).GetComponent<ServerManagerNet>();

        activeInputPayload = InputPayload.Default(0);

    }

    private void FixedUpdate()
    {
        serverManagerNet.Reconciliation(tick);

        activeInputPayload.tick = tick;
        activeInputPayload.direction = activeDirection;

        playerNet.ProcessInputPayload(activeInputPayload);

        activeInputPayload.pistonPush = false;


        playerNet.Tick(0);

        Physics2D.Simulate(PlayerNet.gameFixedDeltaTime);

        /// NO NEED FOR ENTIRE STATE ? JUST POSITION ?
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
            pistonPushArmed = playerNet.pistonPushArmed == 0 ? false : true,
            pistonAngle = Pistonjoint.angle
        });
  
        tick++;
    }

    public void ArmJumping(Vector2 dir)
    {
        activeInputPayload.pistonPush = true;
        activeInputPayload.pistonDirection = dir;
    }
    [ServerRpc]
    public void RotateForceServerRpc(float force)
    {
        PlayerBody.AddTorque(force, ForceMode2D.Force);
    }
    [ServerRpc]
    public void AddPlayerForceServerRpc(Vector2 force)
    {
        PlayerBody.AddForce(force, ForceMode2D.Force);
    }
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
   

    [ServerRpc]
    public void BumpServerRpc(Vector2 force)
    {
        PlayerBody.AddForce(force,ForceMode2D.Impulse);
    }
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
    public void GainPowerup(PowerUps powerup)
    {
        powerupManager.AddPowerup(powerup);
    }
    public void ConsumePowerup(PowerUps powerup, float quantity)
    {
        powerupManager.ConsumePowerup(powerup, quantity);
    }
  

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
  

    //public void EnablePowerup(PowerUps powerup, bool state)
    //{
    //    //switch (powerup)
    //    //{
    //    //    case PowerUps.GraplinHook:
    //    //        graplingHook.SetActive(state);
    //    //        break;
    //    //    case PowerUps.Propeller:
    //    //        propeller.SetActive(state);
    //    //        break;



    //    //    default:
    //    //        break;
    //    //}
    //    EnablePowerupServerRpc(powerup, state);
    //}

}
