using Unity.Netcode;
using UnityEngine;

public class ConfigureNetwork : MonoBehaviour
{
    [SerializeField] private UI gameUI;

    [SerializeField] private GameObject serverManagerGB;

    //public void HostServer()
    //{
    //    NetworkManager.Singleton.StartHost();
    //    //gameUI.Player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
    //    //gameUI.gameObject.SetActive(true);
    //}
    void Awake()
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
        //if (clientId != NetworkManager.Singleton.LocalClientId) Debug.LogError("PB");

        NetworkObject playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;


        if (NetworkManager.Singleton.IsServer)
        {
            //Debug.Log(playerObj.GetComponent<PlayerNet>());
            serverManagerGB.GetComponent<ServerManager>().playerNets.Add(playerObj.GetComponent<PlayerNet>());
            serverManagerGB.SetActive(true);
        }
        else
        {
            if(serverManagerGB != null)
                Destroy(serverManagerGB);
        }
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        //gameUI.Player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        //gameUI.gameObject.SetActive(true);
    }
    public void JoinServer()
    {
        NetworkManager.Singleton.StartClient();
        //gameUI.Player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        //gameUI.gameObject.SetActive(true);
    }

}
