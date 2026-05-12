using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Grapling : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset GameControls;
    private InputActionMap graplinghookControls;
    private InputAction firstTapPress;
    private InputAction firstTapPos;
    private InputAction secondTapPress;
    private InputAction secondTapPos;

    [SerializeField]
    private GameObject hook;
    [SerializeField]
    private GameObject HookSprite;

    private byte aiming;
    private byte aimingIndex;
    //[HideInInspector]
    //public Vector2 aimTarget;
    private bool shootAtempt;
    private bool shot;

    [HideInInspector]
    public Player player;
    [HideInInspector]
    public GraplingNet graplingNet;

    private void Awake()
    {
        graplinghookControls = GameControls.FindActionMap("Graplinghook");
        firstTapPress = graplinghookControls.FindAction("FirstTapPress");
        firstTapPos = graplinghookControls.FindAction("FirstTapPos");
        secondTapPress = graplinghookControls.FindAction("SecondTapPress");
        secondTapPos = graplinghookControls.FindAction("SecondTapPos");
        player = this.transform.parent.parent.GetComponent<Player>();
        graplingNet = this.GetComponent<GraplingNet>();

    }

    private void OnEnable()
    {
        graplinghookControls.Enable();
        firstTapPress.performed += ctx => TryShooting(0, ctx);
        secondTapPress.started += ctx => TryShooting(1, ctx);
        firstTapPress.canceled += ctx => TryShooting(0, ctx);
        secondTapPress.canceled += ctx => TryShooting(1, ctx);
    }

    private void OnDisable()
    {
        firstTapPress.performed -= ctx => TryShooting(0, ctx);
        secondTapPress.started -= ctx => TryShooting(1, ctx);
        firstTapPress.canceled -= ctx => TryShooting(0, ctx);
        secondTapPress.canceled -= ctx => TryShooting(1, ctx);
        graplinghookControls.Disable();

        if (shot)
        {
            graplingNet.grapleActive.Value = false;
            player.ConsumePowerup(PowerUps.GraplinHook, 1);
            shot = false;
        }
        shootAtempt = false;
    }

    void Update()
    {

        Quaternion newRot = Quaternion.identity;

        Vector3 targetWorldPos;
        if (hook.activeInHierarchy == true)
        {
            targetWorldPos = hook.transform.position;
            //Debug.Log(targetWorldPos);
        }
        else
        {
            var aimPoint = aimingIndex == 0 ? firstTapPos.ReadValue<Vector2>() : secondTapPos.ReadValue<Vector2>();
            targetWorldPos = Camera.main.ScreenToWorldPoint(aimPoint);
        }
        targetWorldPos.z = 0f;


        Vector2 aimTarget = targetWorldPos - transform.position;

        float angle = Mathf.Atan2(aimTarget.y, aimTarget.x) * Mathf.Rad2Deg;

        //transform.rotation =  Quaternion.Euler(0f, 0f, angle);

        //graplingNet.UpdateRotationServerRpc(angle);
        graplingNet.graplingAngle.Value = angle;
    }
    void TryShooting(byte index, InputAction.CallbackContext ctx)
    {
        if (aimingIndex != index) return;

        aiming = (byte)(ctx.ReadValue<float>());
        var pos = index == 0 ? firstTapPos.ReadValue<Vector2>() : secondTapPos.ReadValue<Vector2>();

        if (aiming == 1)
        {
            shootAtempt = !player.ui.uiRaycast.PointerOverUI(pos);
            if (shot & shootAtempt)
            {
                aimingIndex = 0;
                graplingNet.grapleActive.Value = false;
                shot = false;
                player.ConsumePowerup(PowerUps.GraplinHook,1);
            }
            return;
        }
        else if(shootAtempt)
        {
            shootAtempt = false;
            if (!shot)
            {
                hook.transform.rotation = transform.rotation;
                aimingIndex = index;
                graplingNet.grapleActive.Value = true;
                shot = true;
            }
        }
    }

    //public void Disable()
    //{
    
    //    gameObject.SetActive(false);
    //}

    //private void HookActivation(bool state)
    //{
    //    hook.SetActive(state);
    //    HookSprite.SetActive(!state);
    //}

}
