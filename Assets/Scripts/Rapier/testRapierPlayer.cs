using UnityEngine;
using UnityEngine.InputSystem;

public class testRapierPlayer : MonoBehaviour
{
    [SerializeField]
    RapierBody pistonBody;
    [SerializeField]
    RapierSliderJoint slider;
    [SerializeField]
    RapierBody cogBody;

    void Start()
    {
        if (!gameObject.TryGetComponent<RapierBody>(out var body)) Debug.Log("reaezrezr");
        RapierWorld.ignore_collision(RapierWorld.world, body.entityHandle,pistonBody.entityHandle,true);
        RapierWorld.body_set_rotation(RapierWorld.world,cogBody.entityHandle,90*Mathf.Deg2Rad);
    }

    public void pistonRight()
    {
        //slider.remplaceJoint(1, 0, 0, 0);
        RapierWorld.joint_set_prismatic_motor(RapierWorld.world, slider.handle,2.5f, 1500,20,150000);
        RapierWorld.body_set_rotation(RapierWorld.world,cogBody.entityHandle,Time.deltaTime*10000*Mathf.Deg2Rad);
    }

    public void pistonLeft()
    {
        //slider.remplaceJoint(0, 1, 0, 0);
        RapierWorld.joint_set_prismatic_motor(RapierWorld.world, slider.handle, 0, 500, 70, 10000);
        //RapierWorld.body_set_rotation(RapierWorld.world, cogBody.entityHandle, 0 * Mathf.Deg2Rad);
    }
    public void pushup()
    {

    }
    public void pushdown()
    {

    }

}
