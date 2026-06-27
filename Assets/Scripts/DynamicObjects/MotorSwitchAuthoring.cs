using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SilderMotorSwitch))]
public class MotorSwitchAuthoring : MonoBehaviour
{

    [SerializeField]
    float switchTimer;
    ushort switchTimerInTicks;

    [SerializeField]
    [Range(0f, 1f)]
    float StartswitchTimerProgress;

    [SerializeField]
    short forwardMotorForce;
    [SerializeField]
    short reverseMotorForce;

    void Start()
    {
        switchTimerInTicks = (ushort)Mathf.RoundToInt(switchTimer * (1 / PlayerNet.gameFixedDeltaTime));

        if (TryGetComponent(out HingeJoint2D hinge))
        {

        }
        else if (TryGetComponent(out SliderJoint2D slider))
        {
            GetComponent<SilderMotorSwitch>().Init(
                switchTimerInTicks,
                (ushort)(switchTimerInTicks * Mathf.Abs(StartswitchTimerProgress - 1)),
                forwardMotorForce,
                reverseMotorForce
                );
        }
        else
        {
            Debug.LogError("No valid motorized 2D joint found.");
        }

        Destroy(this);

    }

}
