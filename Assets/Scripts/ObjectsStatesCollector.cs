using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class ObjectsStatesCollector : MonoBehaviour
{

    [HideInInspector]
    public PlayerRigidbodyStates playerRigidbodyStates;

    //private void FixedUpdate()
    //{
    //    var selfShape = GetComponent<CircleCollider2D>();

    //    var overlapedBodies = Physics2D.OverlapCircleAll(transform.position, selfShape.radius, LayerMask.GetMask("dynamicObstacle"));

    //    foreach (var shape in overlapedBodies) 
    //    {
    //        var body = shape.attachedRigidbody;
    //        playerRigidbodyStates.playerRigidbodyStates[playerRigidbodyStates.playerRigidbodyStatesCount] = new rigidbodyState
    //        {
    //            rigidbodyNetId = (ushort)body.gameObject.GetComponent<NetworkObject>().NetworkObjectId,
    //            phyState = new PhysicsState
    //            {
    //                position = body.position,
    //                rotation = body.rotation,
    //                linearVelocity = body.linearVelocity,
    //                angularVelocity = body.angularVelocity,
    //            }
    //        };
    //        playerRigidbodyStates.playerRigidbodyStatesCount++;
    //    }
    //}
}
