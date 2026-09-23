using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum PowerUp
{
    None,
    GraplinHook,
    Propeller,
    AutoTurret,
    Rifle
}

public class UI : MonoBehaviour
{
    [HideInInspector]
    public UIRaycast uiRaycast; 

    [HideInInspector]
    public Player player;
    [HideInInspector]
    public PlayerNet playerNet;

    [SerializeField]
    private Image rightDirImage;
    [SerializeField]
    private Image leftDirImage;
    private Color buttonPressedColor = new Color ( 1,1,1,0.8f);
    private Color buttonIdleColor = new Color(1, 1, 1, 0.2f);
    [SerializeField]
    private Image leftJScooldownImage;
    [SerializeField]
    private Image rightJScooldownImage;
    [SerializeField]
    private Image leftJSImage;
    [SerializeField]
    private Image rightJSImage;

    public GameObject respawnPoint;

    [SerializeField]
    private RectTransform leftjoystickRect;
    [SerializeField]
    private RectTransform rightjoystickRect;

    [SerializeField]
    private Slider energyBar;
    [SerializeField]
    private Slider healthBar;

    private InputAction finger1Pos;
    private InputAction finger1Press;
    private InputAction finger2Pos;
    private InputAction finger2Press;
    private byte activeFinger;
    private bool aimAction;

    public InputActionAsset PlayerInputs;

    private float rightPressed;
    private float leftPressed;

    private float jumpCooldown = 1;
    private byte activeJumpCooldown = 0;

    public GameObject hookDisableButton;

    [SerializeField]
    private GameObject[] powerUpsUISlot;


    private void Awake()
    {
        player.ui = this.GetComponent<UI>();

        finger1Pos = InputSystem.actions.FindAction("Finger1Pos");
        finger1Press = InputSystem.actions.FindAction("Finger1Press");
        finger2Pos = InputSystem.actions.FindAction("Finger2Pos");
        finger2Press = InputSystem.actions.FindAction("Finger2Press");

    }

    private void OnEnable()
    {
        PlayerInputs.FindActionMap("Player").Enable();
        finger1Press.started += ctx => ProcessPressStarted(1, ctx);
        finger1Press.canceled += ctx => ProcessPressReleased(1, ctx);
        finger2Press.started += ctx => ProcessPressStarted(2, ctx);
        finger2Press.canceled += ctx => ProcessPressReleased(2, ctx);
    }

    private void OnDisable()
    {
        finger1Press.started -= ctx => ProcessPressStarted(1, ctx);
        finger1Press.canceled -= ctx => ProcessPressReleased(1, ctx);
        finger2Press.started -= ctx => ProcessPressStarted(2, ctx);
        finger2Press.canceled -= ctx => ProcessPressReleased(2, ctx);

        //if (shot)
        //{
        //    graplingNet.grapleActive.Value = false;
        //    player.ConsumePowerup(PowerUps.GraplinHook, 1);
        //    shot = false;
        //}
        //shootAtempt = false;
        PlayerInputs.FindActionMap("Player").Disable();
    }
    



    private void Start()
    {
        Application.targetFrameRate = 60;
        uiRaycast = new UIRaycast(this.GetComponent<Canvas>());

        GameObject CancelPowerUpUIPrefab = Resources.Load<GameObject>("Prefabs/PowerUpsUI/CancelPowerUpUI");
        for (int i = 0; i < 8; i++)
        {
            switch (playerNet.equipedPowerUpMap[i])
            {
                case PowerUp.None:
                    break;
                case PowerUp.GraplinHook:

                    GameObject GraplinHookUIPrefab = Resources.Load<GameObject>("Prefabs/PowerUpsUI/GraplingHookUIButton");
                    GameObject GraplinHookUIInstance = Instantiate(GraplinHookUIPrefab, powerUpsUISlot[i].transform);
                    GraplinHookUIInstance.GetComponent<Button>().onClick.AddListener(() => SelectPowerUp(PowerUp.GraplinHook));
                    var GraplinHookUiLink = playerNet.powerUpsGB[i].gameObject.AddComponent<PowerUpUiLink>();
                    GraplinHookUiLink.UIobject = GraplinHookUIInstance.transform.GetChild(0).gameObject;

                    GraplinHookUIPrefab = null;

                    GameObject GraplinHookUICancelInstance = Instantiate(CancelPowerUpUIPrefab, GraplinHookUIInstance.transform);
                    GraplinHookUICancelInstance.GetComponent<Button>().onClick.AddListener(() => CancelPowerUp(PowerUp.GraplinHook));
                    var GraplinHookCancelUiLink = playerNet.powerUpsGB[i].GetComponent<Grapling>().hook.gameObject.AddComponent<PowerUpUiLink>();
                    GraplinHookCancelUiLink.UIobject = GraplinHookUICancelInstance;

                    break;
                case PowerUp.Propeller:

                    GameObject PropellerUIPrefab = Resources.Load<GameObject>("Prefabs/PowerUpsUI/PropellerUIButton");
                    GameObject PropellerUIInstance = Instantiate(PropellerUIPrefab, powerUpsUISlot[i].transform);
                    PropellerUIInstance.GetComponent<Button>().onClick.AddListener(() => SelectPowerUp(PowerUp.Propeller));
                    var PropellerSelectUiLink = playerNet.powerUpsGB[i].gameObject.AddComponent<PowerUpUiLink>();
                    PropellerSelectUiLink.UIobject = PropellerUIInstance.transform.GetChild(0).gameObject;

                    PropellerUIPrefab = null;

                    break;
                case PowerUp.AutoTurret:

                    GameObject AutoTurretUIPrefab = Resources.Load<GameObject>("Prefabs/PowerUpsUI/AutoTurretUIButton");
                    GameObject AutoTurretUIInstance = Instantiate(AutoTurretUIPrefab, powerUpsUISlot[i].transform);
                    AutoTurretUIInstance.GetComponent<Image>().color = Color.blue;
                    int AutoTurretIdx = i;
                    AutoTurretUIInstance.GetComponent<Button>().onClick.AddListener(() => ChangePassivePowerUp(AutoTurretIdx));

                    var AutoTurretActiveUiLink = playerNet.powerUpsGB[i].gameObject.AddComponent<PowerUpUiLink>();
                    AutoTurretActiveUiLink.UIobject = AutoTurretUIInstance.transform.GetChild(0).gameObject;
                    var autoTurretDetection = playerNet.powerUpsGB[i].gameObject.AddComponent<PowerUpPassif>();
                    autoTurretDetection.player = player;
                    autoTurretDetection.powerUpIdx = i;

                    PropellerUIPrefab = null;

                    break;
            }
        }
        CancelPowerUpUIPrefab = null;
        Resources.UnloadUnusedAssets();


    }


    private void FixedUpdate()
    {
        if (aimAction)
        {
            //switch (playerNet.ac)
            //{
            //    default:
            //        break;
            //}
            Vector2 aimTarget = Camera.main.ScreenToWorldPoint(activeFinger == 1 ? finger1Pos.ReadValue<Vector2>() : finger2Pos.ReadValue<Vector2>()) - transform.position;
            player.RegisterAction(Action.Aim, aimTarget);
        }
    }
    private void Update()
    {

        /// OPTI
        uiRaycast.RebuildCache();
        energyBar.value = playerNet.mechanicalState.energy / 100f;
        healthBar.value = playerNet.health / 100f;

        jumpCooldown = jumpCooldown - (Time.deltaTime * activeJumpCooldown);
        leftJScooldownImage.fillAmount = 1 - jumpCooldown / 1f;
        rightJScooldownImage.fillAmount = 1 - jumpCooldown / 1f;
        if (jumpCooldown <= 0)
        {
            jumpCooldown = 1;
            activeJumpCooldown = 0;
            leftJSImage.color = new Color(1, 1, 1, 1);
            rightJSImage.color = new Color(1, 1, 1, 1);
        }


    }
    public void ProcessPressStarted(byte fingerIdx, InputAction.CallbackContext ctx)
    {
        if (activeFinger != 0)
            return;

        var pos = fingerIdx == 1 ? finger1Pos.ReadValue<Vector2>() : finger2Pos.ReadValue<Vector2>();


        if (!uiRaycast.PointerOverUI(pos))
        {
            activeFinger = fingerIdx;

            switch (playerNet.mechanicalState.selectedPowerUp)
            {
                case PowerUp.None:

                    break;
                case PowerUp.GraplinHook:
                    aimAction = true;
                    player.RegisterAction(Action.PowerUpAction2, Vector2.zero);
                    break;
                case PowerUp.Propeller:
                    aimAction = true;
                    player.RegisterAction(Action.PowerUpAction1, Vector2.zero);
                    break;
            }
        }
    }
    public void ProcessPressReleased(byte fingerIdx, InputAction.CallbackContext ctx)
    {

        if (activeFinger != fingerIdx)
            return;

        var pos = fingerIdx == 1 ? finger1Pos.ReadValue<Vector2>() : finger2Pos.ReadValue<Vector2>();
        //if (!uiRaycast.PointerOverUI(pos))
        {

            switch (playerNet.mechanicalState.selectedPowerUp)
            {
                case PowerUp.None:

                    break;
                case PowerUp.GraplinHook:
                    player.RegisterAction(Action.PowerUpAction1, Camera.main.ScreenToWorldPoint(pos) - transform.position);
                    break;
                case PowerUp.Propeller:
                    aimAction = false;
                    player.RegisterAction(Action.PowerUpAction2, Vector2.zero);
                    break;
            }
        }
        aimAction = false;
        activeFinger = 0;
    }

    public void SelectPowerUp(PowerUp powerUp)
    {
        if (playerNet.mechanicalState.selectedPowerUp == powerUp) powerUp = 0;
        player.RegisterAction(Action.ChangeSelectedPowerUp, new Vector2((int)powerUp, 0));
    }
    public void ChangePassivePowerUp(int powerUpIdx)
    {
        player.RegisterAction(Action.ChangePassivePowerUp, new Vector2(powerUpIdx, 0));
    }
    public void CancelPowerUp(PowerUp powerUp)
    {
        player.RegisterAction(Action.CancelPowerUp, new Vector2((int)powerUp, 0));
    }

    public void LeftDirPressed()
    {
        leftPressed = -1;
        if (rightPressed + leftPressed == 0)
        {
            leftDirImage.color = buttonIdleColor;
            rightDirImage.color = buttonIdleColor;
        }
        else
        {
            leftDirImage.color = buttonPressedColor;
        }
        player.rollDirection = rightPressed + leftPressed;
    }
    public void LeftDirReleased()
    {
        leftPressed = 0;
        leftDirImage.color = buttonIdleColor;
        if (rightPressed == 1)
        {
            rightDirImage.color = buttonPressedColor;
        }
        player.rollDirection = rightPressed + leftPressed;
    }
    public void RightDirPressed()
    {
        rightPressed = 1;
        if (rightPressed + leftPressed == 0)
        {
            leftDirImage.color = buttonIdleColor;
            rightDirImage.color = buttonIdleColor;
        }
        else
        {
            rightDirImage.color = buttonPressedColor;
        }
        player.rollDirection = rightPressed + leftPressed;
    }
    public void RightDirReleased()
    {
        rightPressed = 0;
        rightDirImage.color = buttonIdleColor;
        if (leftPressed == 1)
        {
            leftDirImage.color = buttonPressedColor;
        }
        player.rollDirection = rightPressed + leftPressed;
    }
    public void JoystickReleased(Vector2 dir)
    {
        if (activeJumpCooldown == 0)
        {
            player.ArmJumping(dir);
            activeJumpCooldown = 1;
            rightJSImage.color = buttonIdleColor;
            leftJSImage.color = buttonIdleColor;
        }
    }
}
