using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    private InputAction mouvementActivateInput;
    private InputAction finger1Delta;
    private InputAction finger1Press;
    private InputAction finger2Delta;
    private InputAction finger2Press;

    public InputActionAsset PlayerInputs;

    private float rightPressed;
    private float leftPressed;
    private byte LeftJoySFingerIdx;
    private byte RightJoySFingerIdx;
    private Vector2 finger1HeldDelta;
    private Vector2 finger2HeldDelta;
    Vector2 leftjoystickDisplace;
    Vector2 rightjoystickDisplace;

    private float jumpCooldown = 1;
    private byte activeJumpCooldown = 0;


    private void OnEnable()
    {
        PlayerInputs.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        PlayerInputs.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        player.ui = this.GetComponent<UI>();

        finger1Delta = InputSystem.actions.FindAction("Finger1Delta");
        finger1Press = InputSystem.actions.FindAction("Finger1Press");
        finger2Delta = InputSystem.actions.FindAction("Finger2Delta");
        finger2Press = InputSystem.actions.FindAction("Finger2Press");

    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        uiRaycast = new UIRaycast(this.GetComponent<Canvas>());
    }

    private void Update()
    {
        //finger1HeldDelta = finger1HeldDelta * finger1Press.ReadValue<float>() + finger1Delta.ReadValue<Vector2>();
        //finger2HeldDelta = finger2HeldDelta * finger2Press.ReadValue<float>() + finger2Delta.ReadValue<Vector2>();

        //leftjoystickDisplace = finger1HeldDelta * BitwiseUtils.CompareBytes(LeftJoySFingerIdx, 1) + finger2HeldDelta * BitwiseUtils.CompareBytes(LeftJoySFingerIdx, 2);
        //rightjoystickDisplace = finger1HeldDelta * BitwiseUtils.CompareBytes(RightJoySFingerIdx, 1) + finger2HeldDelta * BitwiseUtils.CompareBytes(RightJoySFingerIdx, 2);

        //leftjoystickRect.anchoredPosition = leftjoystickDisplace.normalized * Mathf.Min(leftjoystickDisplace.magnitude, 60f);
        //rightjoystickRect.anchoredPosition = rightjoystickDisplace.normalized * Mathf.Min(rightjoystickDisplace.magnitude,60f);

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


    //public void LeftJoySPressed()
    //{
    //    //LeftJoySFingerIdx = (byte);
    //    Player.PistonPullServerRpc();
    //}
    //public void RightJoySPressed()
    //{
    //    RightJoySFingerIdx = (byte)(finger1Press.ReadValue<float>() + finger2Press.ReadValue<float>());
    //    Player.PistonPullServerRpc();
    //}
    //public void LeftJoySReleased()
    //{
    //    if (activeJumpCooldown == 0)
    //    {
    //        LeftJoySFingerIdx = 0;
    //        Player.tryJumping(leftjoystickDisplace.normalized);
    //        activeJumpCooldown = 1;
    //        leftJSImage.color = buttonIdleColor;
    //        rightJSImage.color = buttonIdleColor;
    //    }
    //}
    //public void RightJoySReleased()
    //{
    //    if(activeJumpCooldown == 0)
    //    {
    //        RightJoySFingerIdx = 0;
    //        Player.tryJumping(rightjoystickDisplace.normalized);
    //        activeJumpCooldown = 1;
    //        rightJSImage.color = buttonIdleColor;
    //        leftJSImage.color = buttonIdleColor;
    //    }
    //}

    public void LeftDirPressed()
    {
        leftPressed = -1;
        if (rightPressed + leftPressed == 0)
        {
            player.activeDirection = 0;
            leftDirImage.color = buttonIdleColor;
            rightDirImage.color = buttonIdleColor;
        }
        else
        {
            player.activeDirection = -1;
            leftDirImage.color = buttonPressedColor;
        }
    }
    public void LeftDirReleased()
    {
        leftPressed = 0;
        leftDirImage.color = buttonIdleColor;
        if (rightPressed == 1)
        {
            player.activeDirection = 1;
            rightDirImage.color = buttonPressedColor;
        }
        else
        {
            player.activeDirection = 0;
        }
    }
    public void RightDirPressed()
    {
        rightPressed = 1;
        if (rightPressed + leftPressed == 0)
        {
            player.activeDirection = 0;
            leftDirImage.color = buttonIdleColor;
            rightDirImage.color = buttonIdleColor;
        }
        else
        {
            player.activeDirection = 1;
            rightDirImage.color = buttonPressedColor;
        }
    }
    public void RightDirReleased()
    {
        rightPressed = 0;
        rightDirImage.color = buttonIdleColor;
        if (leftPressed == 1)
        {
            player.activeDirection = -1;
            leftDirImage.color = buttonPressedColor;
        }
        else
        {
            player.activeDirection = 0;
        }
    }
    public void JoystickPressed()
    {
        //playerNet.PistonPull();
    }
    public void JoystickReleased(Vector2 dir)
    {
        if (activeJumpCooldown == 0)
        {
            RightJoySFingerIdx = 0;
            player.ArmJumping(dir);
            activeJumpCooldown = 1;
            rightJSImage.color = buttonIdleColor;
            leftJSImage.color = buttonIdleColor;
        }
    }
}
