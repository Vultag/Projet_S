using System.Numerics;
using Unity.Mathematics;
using UnityEngine;

public class BasicRotation : MonoBehaviour
{
    [SerializeField] private float rotationDelta;

    void Update()
    {
        transform.rotation *= quaternion.RotateZ(rotationDelta);
    }
}
