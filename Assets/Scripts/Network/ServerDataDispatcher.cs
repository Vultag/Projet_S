using Unity.Netcode;
using UnityEngine;

public class ServerDataDispatcher : MonoBehaviour
{
    private ServerManagerNet serverManagerNet;
    private ServerManager serverManager;

    private uint sentDataTick;
    public bool shouldDispatch;


    InputPayload[] p1NewInputs = new InputPayload[PlayerNet.PayloadRBufferSize];
    InputPayload[] p2NewInputs = new InputPayload[PlayerNet.PayloadRBufferSize];
    InputPayload[] p3NewInputs = new InputPayload[PlayerNet.PayloadRBufferSize];
    InputPayload[] p4NewInputs = new InputPayload[PlayerNet.PayloadRBufferSize];


    private void Awake()
    {
        serverManagerNet = this.GetComponent<ServerManagerNet>();
        serverManager = this.GetComponent<ServerManager>();
    }

    private void FixedUpdate()
    {
        if (!shouldDispatch)
            return;

        {
            byte i = 0;
            foreach (PlayerNet playerNet in serverManagerNet.Players)
            {
                serverManager.statePayloads[i] = new StatePayload
                {
                    tick = ServerManagerNet.tick,
                    playerPhyState = RapierWorld.body_get_state(RapierWorld.world,playerNet.PlayerBody.entityHandle),
                    //{
                    //    position = playerNet.PlayerBody.position,
                    //    rotation = playerNet.PlayerBody.rotation,
                    //    linearVelocity = playerNet.PlayerBody.linearVelocity,
                    //    angularVelocity = playerNet.PlayerBody.angularVelocity,
                    //},
                    pistonPhyState = RapierWorld.body_get_state(RapierWorld.world, playerNet.PistonBody.entityHandle),
                    //{
                    //    position = playerNet.PistonBody.position,
                    //    rotation = playerNet.PistonBody.rotation,
                    //    linearVelocity = playerNet.PistonBody.linearVelocity,
                    //    angularVelocity = playerNet.PistonBody.angularVelocity,
                    //},
                    cogPhyState = RapierWorld.body_get_state(RapierWorld.world, playerNet.CogBody.entityHandle),
                    //{
                    //    position = playerNet.CogBody.position,
                    //    rotation = playerNet.CogBody.rotation,
                    //    linearVelocity = playerNet.CogBody.linearVelocity,
                    //    angularVelocity = playerNet.CogBody.angularVelocity,
                    //},
                    playerMechanicsState = new MechanicsState
                    {
                        activeRevertCooldown = playerNet.mechanicalState.activeRevertCooldown,
                        revertCooldown = playerNet.mechanicalState.revertCooldown,
                        pistonPushOrPull = playerNet.mechanicalState.pistonPushOrPull,
                        ticksTillPistonPushActivation = playerNet.mechanicalState.ticksTillPistonPushActivation,
                        pistonAngle = playerNet.mechanicalState.pistonAngle,
                        pistonPushArmed = playerNet.mechanicalState.pistonPushArmed,
                    }
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

            //Debug.Log("sent tick start : " + serverManager.statePayloads[0].tick);

            serverManagerNet.SendLatestDataPayloadsClientRpc(
             ServerManagerNet.tick,
             playerRigidbodyStates.playerRigidbodyStatesCount,
             playerRigidbodyStates.playerRigidbodyStates,
             serverManager.statePayloads,
             new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { serverManager.targetClientIds[i] } } }
             );

        }

        var newInputsLenght = ServerManagerNet.tick - sentDataTick;


        //var p1Inputs = serverManagerNet.Players[0].inputPayloadRBuffer.GetArray();
        //var p2Inputs = serverManagerNet.Players.Count > 1 ? serverManagerNet.Players[1].inputPayloadRBuffer.GetArray():new InputPayload[0];
        //var p3Inputs = serverManagerNet.Players.Count > 2 ? serverManagerNet.Players[2].inputPayloadRBuffer.GetArray() : new InputPayload[0];
        //var p4Inputs = serverManagerNet.Players.Count > 3 ? serverManagerNet.Players[3].inputPayloadRBuffer.GetArray() : new InputPayload[0];

        //switch (serverManagerNet.Players.Count)
        //{
        //    case 1:
        //        for (int i = 0; i < newInputsLenght; i++)
        //        {
        //            var idx = (short)(sentDataTick - ServerManagerNet.tick + i);
        //            p1NewInputs[i] = serverManagerNet.Players[0].inputPayloadRBuffer.Read(idx);
        //        }
        //        break;
        //    case 2:
        //        for (int i = 0; i < newInputsLenght; i++)
        //        {
        //            var idx = (short)(sentDataTick - ServerManagerNet.tick + i);
        //            p1NewInputs[i] = serverManagerNet.Players[0].inputPayloadRBuffer.Read(idx);
        //            p2NewInputs[i] = serverManagerNet.Players[1].inputPayloadRBuffer.Read(idx);
        //        }
        //        break;
        //    case 3:
        //        for (int i = 0; i < newInputsLenght; i++)
        //        {
        //            var idx = (short)(sentDataTick - ServerManagerNet.tick + i);
        //            p1NewInputs[i] = serverManagerNet.Players[0].inputPayloadRBuffer.Read(idx);
        //            p2NewInputs[i] = serverManagerNet.Players[1].inputPayloadRBuffer.Read(idx);
        //            p3NewInputs[i] = serverManagerNet.Players[2].inputPayloadRBuffer.Read(idx);
        //        }
        //        break;
        //    case 4:
        //        for (int i = 0; i < newInputsLenght; i++)
        //        {
        //            var idx = (short)(sentDataTick - ServerManagerNet.tick + i);
        //            p1NewInputs[i] = serverManagerNet.Players[0].inputPayloadRBuffer.Read(idx);
        //            p2NewInputs[i] = serverManagerNet.Players[1].inputPayloadRBuffer.Read(idx);
        //            p3NewInputs[i] = serverManagerNet.Players[2].inputPayloadRBuffer.Read(idx);
        //            p4NewInputs[i] = serverManagerNet.Players[3].inputPayloadRBuffer.Read(idx);
        //        }
        //        break;
        //}


        //serverManagerNet.SendLatestDataPayloadsClientRpc(
        //ServerManagerNet.tick,
        //p1NewInputs,
        //p2NewInputs,
        //p3NewInputs,
        //p4NewInputs
        //);



        /// detect every frame
        serverManager.player1RigidbodyStates.playerRigidbodyStatesCount = 0;
        serverManager.player2RigidbodyStates.playerRigidbodyStatesCount = 0;
        serverManager.player3RigidbodyStates.playerRigidbodyStatesCount = 0;
        serverManager.player4RigidbodyStates.playerRigidbodyStatesCount = 0;

        shouldDispatch = false;
        sentDataTick = ServerManagerNet.tick;

    }

}
