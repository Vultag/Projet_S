using UnityEngine;

public class WallPiston : MonoBehaviour,ISyncronizable
{

    /*
     *  DATA oriented opti : centralized piston manager that detect state change through StateFunnel system ?
     */

    [SerializeField]
    ushort tickSwitchTimer;

    RapierSliderJoint pistonJoint;

    bool pushingOrPulling;
    bool pushingOrPullingROLLBACK;

    private void Start()
    {
        GameSyncManager.gameManager.map.Syncronized.Add(this);
        pistonJoint = this.GetComponent<RapierSliderJoint>();
    }

    public void Tick(short relativeTick)
    {

        /// make tick with info tied to world tick

        bool pushOrPull = ((ServerManagerNet.tick + relativeTick) % (tickSwitchTimer * 2)) - tickSwitchTimer > 0;


        //Debug.Log(((ServerManagerNet.tick + relativeTick)));
        //Debug.Log(((ServerManagerNet.tick + relativeTick) % (tickSwitchTimer * 2)));
        //Debug.Log(((ServerManagerNet.tick + relativeTick) % (tickSwitchTimer * 2)) - tickSwitchTimer);

        if (pushingOrPulling != pushOrPull)
        {
            if(pushOrPull)
            {
                //if (!GameSyncManager.DEBUGreconciliating)Debug.Log("push at  "  + (ServerManagerNet.tick+ relativeTick));
                RapierWorld.joint_set_prismatic_motor(RapierWorld.world, pistonJoint.handle, 6f, 15000, 12, 80000);
            }
            else
            {
                //if (!GameSyncManager.DEBUGreconciliating) Debug.Log("pull at  " + (ServerManagerNet.tick + relativeTick));
                RapierWorld.joint_set_prismatic_motor(RapierWorld.world, pistonJoint.handle, -6f, 15000, 12, 80000);
            }
            pushingOrPulling = pushOrPull;
        }
    }

    public void ROLLBACKsave()
    {
        pushingOrPullingROLLBACK = pushingOrPulling;
    }

    public void ROLLBACKrestore()
    {
        pushingOrPulling = pushingOrPullingROLLBACK;
    }
}
