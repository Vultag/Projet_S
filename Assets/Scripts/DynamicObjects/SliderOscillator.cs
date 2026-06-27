using UnityEngine;

public class SliderOscillator : MonoBehaviour
{

    [SerializeField]
    short upwardForce;
    [SerializeField]
    short downwardForce;
    [SerializeField]
    float middleForcesPadding;
    [SerializeField]
    float YvelocityClamp;

    bool upwardOrDownward = true;

    float initialY;
    Rigidbody2D body;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        initialY = this.transform.position.y;
        //body.AddForceY(upwardForce*4);
    }

    //public void Init()
    //{

    //}

    public void Tick(uint tick)
    {

        if(upwardOrDownward)
        {
            body.AddForceY(upwardForce);
            upwardOrDownward = body.position.y < (initialY + middleForcesPadding);
        }
        else
        {
            body.AddForceY(downwardForce);
            upwardOrDownward = body.position.y < (initialY - middleForcesPadding);
        }
        body.linearVelocityY = Mathf.Clamp(body.linearVelocityY,-YvelocityClamp, YvelocityClamp);


    }

}
