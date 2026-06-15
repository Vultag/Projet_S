using Unity.Netcode;
using UnityEngine;

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
            serverManagerGB.GetComponent<ServerManagerNet>().Players.Add(playerObj.GetComponent<PlayerNet>());
            serverManagerGB.GetComponent<ServerManager>().playerJoin(playerObj.GetComponent<PlayerNet>());
            Color playerColor = Color.white;
            switch (serverManagerGB.GetComponent<ServerManagerNet>().Players.Count)
            {
                case 1:
                    playerColor = Color.red;
                    break;
                case 2:
                    playerColor = Color.blue;
                    break;
                case 3:
                    playerColor = Color.green;
                    break;
                case 4:
                    playerColor = Color.grey;
                    break;

            }
            playerObj.GetComponent<PlayerNet>().playerIcon.color = playerColor;
        }
        else
        {
            if(serverManagerGB.GetComponent<ServerManager>() != null)
                Destroy(serverManagerGB.GetComponent<ServerManager>());
        }
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    public void JoinServer()
    {
        NetworkManager.Singleton.StartClient();
    }

}
