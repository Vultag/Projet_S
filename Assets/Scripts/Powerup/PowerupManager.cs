using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public enum PowerUps
{
    None,
    GraplinHook,
    Propeller
}

public class PowerupManager : MonoBehaviour
{
    private UI ui;
    [SerializeField]
    private GameObject[] powerupUIs;
    [HideInInspector]
    public PowerUps[] powerupUIids = new PowerUps[2];
    [HideInInspector]
    public float[] powerupcharges = new float[2];
    private TextMeshProUGUI[] powerupUIcharges = new TextMeshProUGUI[2];

    [HideInInspector]
    public PowerUps activePowerup;

    private void Start()
    {
        ui = GetComponent<UI>();
        powerupUIids[1] = (PowerUps)1;
        for (int i = 0; i < powerupcharges.Length;i++)
        {
            powerupUIcharges[i] = powerupUIs[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            powerupUIcharges[i].text = "0";
        }
    }
    public void AddPowerup(PowerUps powerup)
    {
        var porwerupId = (int)(powerup - 1);
        switch (powerup)
        {
            case PowerUps.GraplinHook:
                powerupcharges[porwerupId] += 2;
                break;
            case PowerUps.Propeller:
                powerupcharges[porwerupId] += 50;
                break;



            default:
                break;
        }
        if(!powerupUIs[porwerupId].activeInHierarchy)
        {
            powerupUIs[porwerupId].SetActive(true);
            ui.uiRaycast.RebuildCache();
        }
        powerupUIcharges[porwerupId].text = ((int)powerupcharges[porwerupId]).ToString();
    }

    public void ConsumePowerup(PowerUps powerup, float quantity)
    {
        var porwerupId = (int)(powerup - 1);
        var remainingCharge = powerupcharges[porwerupId] - quantity;
        if (remainingCharge <= 0)
        {
            DisableActivePowerup();
            powerupcharges[porwerupId] = 0;
            return;
        }
        powerupcharges[porwerupId] = remainingCharge;
        powerupUIcharges[porwerupId].text = ((int)powerupcharges[porwerupId]).ToString();
    }

    public void ActivatePowerup(int id)
    {
        if (activePowerup == (PowerUps)id | powerupcharges[(id-1)] <= 0)
        {
            id = 0;
        }
        ui.playerNet.UpdatePowerupServerRpc((PowerUps)id);
        activePowerup = (PowerUps)id;
    }

    public void DisableActivePowerup()
    {

        if (activePowerup == 0)
        {
            /// delay deactivating -> still thrusting when disable on server
            Debug.LogError("activePowerup = 0 on disable !!!");
        }

        var porwerupId = (int)(activePowerup - 1);
        //switch (activePowerup)
        //{
        //    case PowerUps.GraplinHook:
        //        break;
        //    case PowerUps.Propeller:
        //        ui.Player.propeller.GetComponent<Propeller>().Disable();
        //        break;



        //    default:
        //        break;
        //}
        ActivatePowerup((int)activePowerup);
        powerupUIs[porwerupId].SetActive(false);
        powerupcharges[porwerupId] = 0;
        activePowerup = PowerUps.None;
        ui.uiRaycast.RebuildCache();
    }

    //private void GraplinHook(bool activation)
    //{
    //    if(activation)
    //    {
    //        ui.Player.graplingHook.SetActive(true);
    //        activePowerup = PowerUps.GraplinHook;
    //    }
    //    else
    //    {
    //        ui.Player.graplingHook.GetComponent<Grapling>().Disable();
    //        activePowerup = PowerUps.None;
    //    }

    //}
    //private void Propeller(bool activation)
    //{
    //    if (activation)
    //    {
    //        ui.Player.propeller.SetActive(true);
    //        activePowerup = PowerUps.Propeller;
    //    }
    //    else
    //    {
    //        ui.Player.propeller.GetComponent<Propeller>().Disable();
    //        activePowerup = PowerUps.None;
    //    }

    //}


}
