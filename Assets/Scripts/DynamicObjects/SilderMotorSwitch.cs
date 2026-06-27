using UnityEngine;
using UnityEngine.UIElements;

public class SilderMotorSwitch : MonoBehaviour
{


    JointMotor2D forwardMotor;
    JointMotor2D reverseMotor;
    SliderJoint2D sliderJoint;

    private ushort startTickOffset;
    private ushort tickUntilSwitch;

    bool ForwardOrReverse = true;


    public void Init(ushort _startTickOffset, ushort _tickUntilSwitch, short _forwardMotorForce, short _reverseMotorForce)
    {
        sliderJoint = this.GetComponent<SliderJoint2D>();
        startTickOffset = _startTickOffset;
        tickUntilSwitch = _tickUntilSwitch;
        forwardMotor = new JointMotor2D {motorSpeed = _forwardMotorForce , maxMotorTorque = sliderJoint.motor.maxMotorTorque};
        reverseMotor = new JointMotor2D { motorSpeed = _reverseMotorForce, maxMotorTorque = sliderJoint.motor.maxMotorTorque };
    }


    public void Tick(uint tick)
    {
        bool evenOrOdd = (((tick + startTickOffset) / tickUntilSwitch) & 1) == 0;

        if (evenOrOdd != ForwardOrReverse)
        {
            sliderJoint.motor = evenOrOdd ? forwardMotor : reverseMotor;
            ForwardOrReverse = evenOrOdd;
        }
    }

}
