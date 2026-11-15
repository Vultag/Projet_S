using UnityEngine;

public class test : MonoBehaviour
{
    private void FixedUpdate()
    {
        var state = RapierWorld.body_get_state(RapierWorld.world, this.GetComponent<RapierBody>().entityHandle);
        //Debug.Log(ServerManagerNet.tick + "  " + state.rotation);
    }

}
