using UnityEngine;

public class ConveyerBelt : MonoBehaviour
{

    [SerializeField]
    private float beltSpeed;
    [SerializeField]
    private float loopbackDistance;


    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 pos = transform.localPosition;
        this.transform.localPosition = new Vector3((pos.x+beltSpeed)%loopbackDistance, pos.y, pos.z);
    }
}
