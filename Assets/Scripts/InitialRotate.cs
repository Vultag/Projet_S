using UnityEngine;

public class InitialRotate : MonoBehaviour
{
    [SerializeField]
    private float angularSpeed;
    void Start()
    {
        this.GetComponent<Rigidbody2D>().angularVelocity = angularSpeed;
        Destroy(GetComponent<InitialRotate>());
    }
}
