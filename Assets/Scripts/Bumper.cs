
using UnityEngine;

public class Bumper : MonoBehaviour,IRapierCollisionListener
{
    [SerializeField]
    private float bumperStrenght;
    [SerializeField]
    private float angularVel;

    private void Start()
    {
        RapierWorld.body_set_state(RapierWorld.world, this.GetComponent<RapierBody>().entityHandle,
        new BodyStateFFI
            {
                x = transform.position.x,
                y = transform.position.y,
                rotation = transform.rotation.eulerAngles.z * Mathf.Deg2Rad,
                velocityX = 0,
                velocityY = 0,
                angularVelocity  = angularVel,
            });
    }

    public void OnRapierCollisionEnter(ulong handleBodyEntering, Vector2 normal)
    {
        if(GameSyncManager.damageableDatabase.TryGet(handleBodyEntering, out IDamageable damageable))
        {
            RapierWorld.body_add_force(
                  RapierWorld.world,
                  ((PlayerNet)damageable).PlayerBody.entityHandle,
                  normal.x * 1000 * bumperStrenght,
                  normal.y * 1000 * bumperStrenght,
                  0
              );

            //Debug.Log(normal + "   " + RapierWorld.body_get_state(RapierWorld.world, this.GetComponent<RapierBody>().entityHandle).rotation + "   " + handleBodyEntering + "   " + ServerManagerNet.tick);
        }
    }

    public void OnRapierCollisionExit(ulong HandleColiderExiting, Vector2 normal)
    {
        throw new System.NotImplementedException();
    }

}
