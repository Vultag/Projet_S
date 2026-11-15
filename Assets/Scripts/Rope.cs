using UnityEngine;
using UnityEngine.U2D;

public class Rope : MonoBehaviour
{
    private SpriteShapeController ropeSprite;

    [SerializeField]
    private GameObject ropeStartObj;
    [SerializeField]
    private GameObject ropeEndObj;

    private void Start()
    {
        ropeSprite = GetComponent<SpriteShapeController>(); 
        ropeSprite.spline.SetPosition(0, transform.InverseTransformPoint(transform.position));
    }

    void Update()
    {
        if(ropeStartObj!=null)
        {
            ropeSprite.spline.SetPosition(0, transform.InverseTransformPoint(ropeStartObj.transform.position));
        }
        ropeSprite.spline.SetPosition(1, transform.InverseTransformPoint(ropeEndObj.transform.position));
        ropeSprite.RefreshSpriteShape();
    }
}
