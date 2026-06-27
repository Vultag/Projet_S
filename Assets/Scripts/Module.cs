using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;

    [SerializeField]
    private UI ui;

    [SerializeField]
    private List<SliderOscillator> sliderOscillators = new List<SliderOscillator>();

    [SerializeField]
    private List<SilderMotorSwitch> SilderMotorObjects = new List<SilderMotorSwitch>();


    public void Tick(uint tick)
    {
        foreach (var obj in sliderOscillators)
        {
            obj.Tick(tick);
        }
        foreach (var obj in SilderMotorObjects)
        {
            obj.Tick(tick);
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    collision.gameObject.transform.parent.GetComponent<Player>().respawnPoint = respawnPoint.position;
    //    ui.respawnPoint.transform.position = respawnPoint.position;

    //}
}
