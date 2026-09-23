using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class ConfigureNetwork : MonoBehaviour
{
    [SerializeField] private UI gameUI;

    [SerializeField] private GameObject serverManagerGB;

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    void OnClientConnected(ulong clientId)
    {
        NetworkObject playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (NetworkManager.Singleton.IsServer)
        {
            serverManagerGB.GetComponent<ServerManager>().enabled = true;
            GameSyncManager.Players.Add(playerObj.GetComponent<PlayerNet>());
            //int playerCount = GameSyncManager.Players.Count;
            serverManagerGB.GetComponent<ServerManager>().playerJoin(playerObj.GetComponent<PlayerNet>());
            ///playerObj.GetComponentInChildren<ObjectsStatesCollector>().enabled = true;
            
        }
        else
        {
            if (serverManagerGB.GetComponent<ServerManager>() != null)
                Destroy(serverManagerGB.GetComponent<ServerManager>());
        }
    }

    public void StartServer()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(
             "192.168.1.52",
             7777,
             "0.0.0.0"
         );

        bool result = NetworkManager.Singleton.StartServer();


    }
    public void JoinServer()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(
            "192.168.1.52",
            7777
        );
        var test = NetworkManager.Singleton.StartClient();
    }

}
