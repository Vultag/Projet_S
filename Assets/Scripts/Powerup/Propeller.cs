using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Propeller : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset GameControls;
    private InputActionMap propellerControls;
    private InputAction firstTapPress;
    private InputAction firstTapPos;
    private InputAction secondTapPress;
    private InputAction secondTapPos;

    //[SerializeField]
    //private GameObject Thruster;
    [SerializeField]
    private GameObject flames;

    private bool thrusting;
    private byte thrustingIndex;
    //private Vector2 thrustingDirection;

    [HideInInspector]
    public Player player;
    [HideInInspector]
    public PropellerNet propellerNet;

    private void Awake()
    {

        propellerControls = GameControls.FindActionMap("Propeller");
        firstTapPress = propellerControls.FindAction("FirstTapPress");
        firstTapPos = propellerControls.FindAction("FirstTapPos");
        secondTapPress = propellerControls.FindAction("SecondTapPress");
        secondTapPos = propellerControls.FindAction("SecondTapPos");
        player = this.transform.parent.parent.parent.GetComponent<Player>();
        propellerNet = this.transform.parent.GetComponent<PropellerNet>();

        //propellerControls.Enable();
        //firstTapPress.performed += ctx => TryActivate(0, ctx);
        //secondTapPress.started += ctx => TryActivate(1, ctx);
        //firstTapPress.canceled += ctx => TryActivate(0, ctx);
        //secondTapPress.canceled += ctx => TryActivate(1, ctx);
    }

    //public override void OnNetworkSpawn()
    //{
    //    if (!IsOwner)
    //    {
    //        this.enabled = false;
    //        return;
    //    }
    //    //this.gameObject.SetActive(false);
    //}
    private void Start()
    {


    }
    private void OnEnable()
    {
        propellerControls.Enable();
        firstTapPress.performed += ctx => TryActivate(0, ctx);
        secondTapPress.started += ctx => TryActivate(1, ctx);
        firstTapPress.canceled += ctx => TryActivate(0, ctx);
        secondTapPress.canceled += ctx => TryActivate(1, ctx);
        //activateTruster(true);
    }

    private void OnDisable()
    {
        firstTapPress.performed -= ctx => TryActivate(0, ctx);
        secondTapPress.started -= ctx => TryActivate(1, ctx);
        firstTapPress.canceled -= ctx => TryActivate(0, ctx);
        secondTapPress.canceled -= ctx => TryActivate(1, ctx);
        propellerControls.Disable();
        //activateTruster(false);
    }

    private void FixedUpdate()
    {
        if (thrusting)
        {

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(thrustingIndex == 0 ? firstTapPos.ReadValue<Vector2>() : secondTapPos.ReadValue<Vector2>());
            mouseWorldPos.z = 0f;
            Vector2 aimTarget = mouseWorldPos - transform.position;
            float angle = Mathf.Atan2(aimTarget.y, aimTarget.x) * Mathf.Rad2Deg;

            //propellerNet.transform.rotation = Quaternion.Euler(0f, 0f, angle + 90);
            player.AddPlayerForceServerRpc(-aimTarget.normalized * 2500);
            player.ConsumePowerup(PowerUps.Propeller, 0.6f);

            propellerNet.UpdateRotationServerRpc(angle);

        }
    }

    //public void Disable()
    //{
    //    activateFlames(false);
    //    //activateTruster(false);
    //    this.gameObject.SetActive(false);
    //}

    void TryActivate(byte index,InputAction.CallbackContext ctx)
    {
        bool inProgress = ctx.ReadValue<float>() == 0 ? false : true;
        if (thrusting && thrustingIndex!=index) return;
        var pos = index==0?firstTapPos.ReadValue<Vector2>(): secondTapPos.ReadValue<Vector2>();
        if (inProgress) 
        {
            if(!player.ui.uiRaycast.PointerOverUI(pos))
            {
                thrustingIndex = index;
                thrusting = true;
                activateFlames(true);
            }
        }
        else
        {
            activateFlames(false);
            thrusting = false;
        }
    }

    private void activateFlames(bool state)
    {
        propellerNet.ActivateFlamesServerRpc(state);
    }
    //private void activateTruster(bool state)
    //{
    //    //Thruster.SetActive(state);
    //    propellerNet.ActivateThrusterClientRpc(state);
    //}

}
