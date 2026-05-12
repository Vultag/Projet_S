using Unity.Netcode.Components;
using UnityEngine;


public enum HauthorityMode
{
    Server,
    Client
}

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    public HauthorityMode authorityMode = HauthorityMode.Client;

    protected override bool OnIsServerAuthoritative()
    {
        return authorityMode == HauthorityMode.Server;
    }

}


