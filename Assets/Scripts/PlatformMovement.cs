using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlatformMovement : MonoBehaviour, ISyncronizable
{
    private Vector2 mouvStart;
    [SerializeField]
    private Vector2 mouvTarget;
    [SerializeField]
    private ushort travelTickDuration;
    [SerializeField, Range(0f, 0.75f)]
    private float startupProgression;

    private Vector2 distanceDelta;

    private bool backwardOrForward;
    private bool backwardOrForwardROLLBACK;

    RapierBody body;

    private void Start()
    {
        GameSyncManager.gameManager.map.Syncronized.Add(this);

        body = this.GetComponent<RapierBody>();
        mouvStart = new Vector2(this.transform.position.x, this.transform.position.y);

        /// local to world
        mouvTarget = mouvStart + mouvTarget;
        distanceDelta = ((mouvTarget - mouvStart) / (1f - startupProgression)) / (float)(travelTickDuration / 60f);

        RapierWorld.body_set_state(RapierWorld.world,body.entityHandle,
            new BodyStateFFI
            {
                x = this.transform.position.x,
                y = this.transform.position.y,
                velocityX = distanceDelta.x,
                velocityY = distanceDelta.y,
                angularVelocity =0,
                rotation = Mathf.Deg2Rad * transform.eulerAngles.z,
            });

        //travelDelta = travelDurartion==0?Vector2.zero: distance /(travelDurartion);

    }

    public void Tick(short relativeTick)
    {
        bool backOrForth = ((ServerManagerNet.tick + relativeTick + (travelTickDuration * startupProgression)) % (travelTickDuration * 2)) - travelTickDuration > 0;

        if (backwardOrForward != backOrForth)
        {
            //if(!GameSyncManager.DEBUGreconciliating)Debug.Log("switch");

            var newDirection = backOrForth ? -distanceDelta : distanceDelta;

            var bodyState = RapierWorld.body_get_state(RapierWorld.world, body.entityHandle);
            RapierWorld.body_set_state(RapierWorld.world, body.entityHandle,
               new BodyStateFFI
               {
                   x = bodyState.x,
                   y = bodyState.y,
                   velocityX = newDirection.x,
                   velocityY = newDirection.y,
                   angularVelocity = bodyState.angularVelocity,
                   rotation = bodyState.rotation,
               });

            backwardOrForward = backOrForth;
            //startupProgression = 0;
        }


    }

    public void ROLLBACKsave()
    {
        backwardOrForwardROLLBACK = backwardOrForward;
    }
    public void ROLLBACKrestore()
    {
        backwardOrForward = backwardOrForwardROLLBACK;
    }

}
