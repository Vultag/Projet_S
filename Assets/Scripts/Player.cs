using System;
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

    private PlayerNet playerNet;
    private uint tick;

    public SpriteRenderer playerIcon;

    public SliderJoint2D Pistonjoint;
    public Rigidbody2D PlayerBody;

    public InputActionAsset PlayerInputs;

    private InputAction mouvementActivateInput;
    private InputAction mouvementDirectionInput;
    private InputAction finger1Delta;
    private InputAction finger1Press;
    private InputAction finger2Delta;
    private InputAction finger2Press;

    private float mouvementDirection;
    //[HideInInspector]
    //public float rightPressed;
    //[HideInInspector]
    //public float leftPressed;
    [HideInInspector]
    public sbyte activeDirection;


    //public TextMeshProUGUI tempText1;
    //public TextMeshProUGUI tempText2;

    [HideInInspector]
    public Vector2 respawnPoint;
    public GameObject graplingHook;
    public GameObject propeller;

    [SerializeField]
    private GameObject leftjoystickParent;
    private RectTransform leftjoystickRect;
    [SerializeField]
    private GameObject rightjoystickParent;
    private RectTransform rightjoystickRect;

    private Vector2 finger1HeldDelta;
    private Vector2 finger2HeldDelta;

    private byte LeftJoySFingerIdx;
    private byte RightJoySFingerIdx;

    private Vector2 rightJumpForceArmed;
    private Vector2 leftJumpForceArmed;

    //private float jumpCooldown = 1f;
    //private byte activeJumpCooldown = 0;
    //private bool canJump = true;

    private InputPayload activeInputPayload;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    private void Awake()
    {


    }
    private void Start()
    {
        //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");

        playerNet = GetComponent<PlayerNet>();

        playerNet.statePayloadRBuffer = new RingBuffer<StatePayload>(PlayerNet.PayloadRBufferSize);


        //if (IsOwnedByServer) playerIcon.color = Color.red;

        //if (!IsOwner)
        //{
        //    this.enabled = false;
        //    return;
        //}

        var ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);
        powerupManager = ui.GetComponent<PowerupManager>();
        ui.player = this;
        ui.playerNet = GetComponent<PlayerNet>();

        Camera.main.GetComponent<TrackPlayer>().PlayerGB = PlayerBody.gameObject;
        Camera.main.GetComponent<TrackPlayer>().enabled = true;
        //PullM = new JointMotor2D { motorSpeed = -10, maxMotorTorque = Pistonjoint.motor.maxMotorTorque };

        //if (IsServer) playerIcon.color = Color.red;

        ui.gameObject.SetActive(true);
        var gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        gameManager.gameObject.SetActive(true);

        activeInputPayload = InputPayload.Default();
    }

    private void Update()
    {
        //leftJumpForceArmed = Mathf.Min(leftjoystickDisplace.magnitude, 100) * leftjoystickDisplace.normalized;
        //rightJumpForceArmed = Mathf.Min(rightjoystickDisplace.magnitude, 100) * rightjoystickDisplace.normalized;
        //leftjoystickRect.anchoredPosition = leftJumpForceArmed;
        //rightjoystickRect.anchoredPosition = rightJumpForceArmed;

        //tempText1.text = (Mathf.Atan2(rightJumpForceArmed.x, rightJumpForceArmed.y) * Mathf.Rad2Deg).ToString();



    }

    private void FixedUpdate()
    {
        /// NEEDS TO HAPPEN SYNCED WITH ALL IN  SYSTEM
        ///playerNet.Reconciliation(tick);

        activeInputPayload.tick = tick;
        activeInputPayload.direction = activeDirection;

        playerNet.ProcessInputPayload(activeInputPayload);


        playerNet.revertCooldown = (byte)(playerNet.revertCooldown - playerNet.activeRevertCooldown);
        playerNet.ticksTillPistonPushActivation = (byte)(playerNet.ticksTillPistonPushActivation - activeInputPayload.pistonPushArmed);


        //if (playerNet.ticksTillPistonPushActivation < 50) Debug.Log("ticking = " + playerNet.ticksTillPistonPushActivation);

        //if (activeInputPayload.pistonPushArmed==1) Debug.Log("ticking active = " + activeInputPayload.pistonPushArmed + " at tick : " + tick);


        //var stateActiveRevertCooldown = playerNet.activeRevertCooldown;
        //var stateRevertCooldown = playerNet.revertCooldown;
        //var pistonPullOrPush = activeInputPayload.pistonPushArmed == 1 ? false : true;

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
            playerNet.PistonPush(activeInputPayload.pistonDirection);
            activeInputPayload.pistonPushArmed = 0;
            playerNet.ticksTillPistonPushActivation = 50;
            playerNet.pistonPushOrPull = true;
        }

        //RotateForceServerRpc((3000 * -activeDirection)- (PlayerBody.angularVelocity*2f*Mathf.Abs(activeDirection)));
        PlayerBody.AddTorque((3000 * -activeDirection) - (PlayerBody.angularVelocity * 2f * Mathf.Abs(activeDirection)), ForceMode2D.Force);

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
            pistonAngle = Pistonjoint.angle
        });
  
        tick++;
    }

    public void ArmJumping(Vector2 dir)
    {
        activeInputPayload.pistonPushArmed = 1;
        activeInputPayload.pistonDirection = dir;
        //activeInputPayload.ticksTillPistonPushActivation = 50;
    }
    //public void tryJumping(Vector2 dir)
    //{
    //    //if(canJump)
    //    {
    //        PistonPushServerRpc(dir);
    //        activeInputPayload.pistonPushArmed = 1;
    //        activeInputPayload.ticksTillPistonPushActivation = 50;
    //       // activeRevertCooldown = 1;
    //    }
    //}


    //public void LeftDirPressed()
    //{
    //    leftPressed = -1;
    //}
    //public void LeftDirReleased()
    //{
    //    leftPressed = 0;
    //}
    //public void RightDirPressed()
    //{
    //    rightPressed = 1;
    //}
    //public void RightDirReleased()
    //{
    //    rightPressed = 0;
    //}

    //public void LeftJoySPressed()
    //{
    //    LeftJoySFingerIdx = (byte)(finger1Press.ReadValue<float>() + finger2Press.ReadValue<float>());
    //}
    //public void LeftJoySReleased()
    //{
    //    LeftJoySFingerIdx = 0;
    //    //this.GetComponent<Rigidbody2D>().AddForce(leftJumpForceArmed * new Vector2(-1, -1) * 800);
    //    //Pistonjoint.motor =  new JointMotor2D { motorSpeed = -10, maxMotorTorque = Pistonjoint.motor.maxMotorTorque};
    //}
    //public void RightJoySPressed()
    //{
    //    RightJoySFingerIdx = (byte)(finger1Press.ReadValue<float>() + finger2Press.ReadValue<float>());
    //}
    //public void RightJoySReleased()
    //{
    //    RightJoySFingerIdx = 0;
    //    //this.GetComponent<Rigidbody2D>().AddForce(rightJumpForceArmed*new Vector2(-1,-1)*800);
    //    Pistonjoint.angle = -Mathf.Atan2(rightJumpForceArmed.x, rightJumpForceArmed.y) * Mathf.Rad2Deg - 90 - PlayerBody.rotation ;
    //    Pistonjoint.motor = new JointMotor2D { motorSpeed = 100, maxMotorTorque = Pistonjoint.motor.maxMotorTorque };
    //    //Debug.Log(Pistonjoint.angle);
    //}

    //public void AddForce(Vector2 dir)
    //{
    //    //PlayerBody.AddForce(dir);

    //    //Debug.Log($"Spawned | IsOwner={IsOwner} | OwnerClientId={OwnerClientId} | LocalClientId={NetworkManager.Singleton.LocalClientId}");
    //    //Debug.Log(IsOwner);
    //    if(IsOwner)
    //        AddPlayerForceServerRpc(dir);
    //}


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
