using UnityEngine;

public class RapierDisableCollision : MonoBehaviour
{
    [SerializeField]
    private RapierBody withBody;

    void Awake()
    {

        RapierWorld.ignore_collision(RapierWorld.world, this.GetComponent<RapierBody>().entityHandle, withBody.entityHandle, true);
    }
}
