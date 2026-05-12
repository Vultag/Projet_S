using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PistonManager : NetworkBehaviour
{

    [SerializeField]
    private List<SliderJoint2D> PistonJointList;

    // 0 = PULL ; 1 = PUSH
    private byte pistonState = 0;
    private float pistonSwitchTime = 2f;

    private JointMotor2D[] JoinMotors = new JointMotor2D[2];

    void Start()
    {
        JoinMotors[0] = new JointMotor2D { motorSpeed = -10, maxMotorTorque = 120000 };
        JoinMotors[1] = new JointMotor2D { motorSpeed = 30, maxMotorTorque = 120000 };
    }

    // Update is called once per frame
    void Update()
    {
        //if((Mathf.FloorToInt(GameManager.gameTime / pistonSwitchTime) & 1 )== pistonState)
        //{
        //    PistonSwitchServerRpc(pistonState);
        //    pistonState = (byte)Mathf.Abs(pistonState - 1f);
        //}
    }
    [ServerRpc]
    public void PistonSwitchServerRpc(byte pistonState)
    {
        for (int i = 0; i < PistonJointList.Count; i++)
        {
            PistonJointList[i].motor = JoinMotors[pistonState];
        }
    }

}
