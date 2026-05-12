using UnityEngine;
using UnityEngine.U2D;

public class Hook : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D playerBody;

    private FixedJoint2D attachJoint;
    private DistanceJoint2D distanceJoint;
    private Rigidbody2D body;

    [SerializeField]
    private SpriteShapeController hookRope;

    private HookNet hookNet;

    private void Awake()
    {
        attachJoint = GetComponent<FixedJoint2D>();
        distanceJoint = GetComponent<DistanceJoint2D>();
        body = GetComponent<Rigidbody2D>();
        hookNet = GetComponent<HookNet>();
    }
    private void OnEnable()
    {
        transform.position = playerBody.transform.position;
        body.position = playerBody.position;
        body.AddForce(this.transform.right * 600, ForceMode2D.Impulse);

    }

    private void OnDisable()
    {
        attachJoint.connectedBody = null;
        distanceJoint.connectedBody = null;
        attachJoint.enabled = false;
        distanceJoint.enabled = false;
    }

    private void Update()
    {
        hookRope.spline.SetPosition(1, transform.InverseTransformPoint(playerBody.transform.position));
        hookRope.RefreshSpriteShape();

        hookNet.hookPos.Value = new Vector2(transform.position.x, transform.position.y);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        attachJoint.connectedBody = collision.attachedRigidbody;
        attachJoint.enabled = true;
        distanceJoint.connectedBody = playerBody;
        distanceJoint.distance = (playerBody.position - body.position).magnitude;
        distanceJoint.enabled = true;

    }


}
