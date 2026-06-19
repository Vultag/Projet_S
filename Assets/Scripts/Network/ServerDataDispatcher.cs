using Unity.Netcode;
using UnityEngine;

public class ServerDataDispatcher : MonoBehaviour
{
    private ServerManagerNet serverManagerNet;
    private ServerManager serverManager;

    private void Awake()
    {
        serverManagerNet = this.GetComponent<ServerManagerNet>();
        serverManager = this.GetComponent<ServerManager>();
    }

    private void FixedUpdate()
    {

        {
            byte i = 0;
            foreach (PlayerNet playerNet in serverManagerNet.Players)
            {
                serverManager.statePayloads[i] = new StatePayload
                {
                    tick = serverManager.tick,
                    playerPhyState = new PhysicsState
                    {
                        position = playerNet.PlayerBody.position,
                        rotation = playerNet.PlayerBody.rotation,
                        linearVelocity = playerNet.PlayerBody.linearVelocity,
                        angularVelocity = playerNet.PlayerBody.angularVelocity,
                    },
                    pistonPhyState = new PhysicsState
                    {
                        position = playerNet.PistonBody.position,
                        rotation = playerNet.PistonBody.rotation,
                        linearVelocity = playerNet.PistonBody.linearVelocity,
                        angularVelocity = playerNet.PistonBody.angularVelocity,
                    },
                    cogPhyState = new PhysicsState
                    {
                        position = playerNet.CogBody.position,
                        rotation = playerNet.CogBody.rotation,
                        linearVelocity = playerNet.CogBody.linearVelocity,
                        angularVelocity = playerNet.CogBody.angularVelocity,
                    },
                    activeRevertCooldown = playerNet.activeRevertCooldown,
                    revertCooldown = playerNet.revertCooldown,
                    pistonPushOrPull = playerNet.pistonPushOrPull,
                    ticksTillPistonPushActivation = playerNet.ticksTillPistonPushActivation,
                    pistonAngle = playerNet.Pistonjoint.angle,
                    pistonPushArmed = playerNet.pistonPushArmed == 1 ? true : false,
                };
                i++;
            }
        }

        for (int i = 0; i < serverManager.targetClientIds.Count; i++)
        {
            var playerRigidbodyStates = serverManager.player1RigidbodyStates;
            switch (i)
            {
                case 1:
                    playerRigidbodyStates = serverManager.player2RigidbodyStates;
                    break;
                case 2:
                    playerRigidbodyStates = serverManager.player3RigidbodyStates;
                    break;
                case 3:
                    playerRigidbodyStates = serverManager.player4RigidbodyStates;
                    break;
            }
            ;

            serverManagerNet.SendLatestDataPayloadsClientRpc(
             serverManager.tick,
             playerRigidbodyStates.playerRigidbodyStatesCount,
             playerRigidbodyStates.playerRigidbodyStates,
             serverManager.statePayloads,
             new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { serverManager.targetClientIds[i] } } }
             );
        }
        /// detect every frame
        serverManager.player1RigidbodyStates.playerRigidbodyStatesCount = 0;
        serverManager.player2RigidbodyStates.playerRigidbodyStatesCount = 0;
        serverManager.player3RigidbodyStates.playerRigidbodyStatesCount = 0;
        serverManager.player4RigidbodyStates.playerRigidbodyStatesCount = 0;

    }

}
