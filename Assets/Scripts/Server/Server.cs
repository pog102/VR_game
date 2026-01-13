using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Server : NetworkBehaviour
{
    public enum IPOptions
    {
        Server,
        Client,
        Test,
    }

    [SerializeField]
    private IPOptions selectedIP = IPOptions.Server;

    public string ClientIP
    {
        get
        {
            switch (selectedIP)
            {
                case IPOptions.Server:
                    return "0.0.0.0";
                case IPOptions.Client:
                    return "192.168.1.4";
                case IPOptions.Test:
                    return "192.168.1.69";
                default:
                    return "0.0.0.0";
            }
        }
    }

    [SerializeField]
    UnityTransport transport;

    private int totalPlayers = 0;

    void Start()
    {
        SetIpAddress();
        if (selectedIP == IPOptions.Server)
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
    }

    public void StartServer()
    {
        Debug.Log("Starting server...");
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectMessage;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void SetIpAddress()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ClientIP;
    }

    public void StartClient()
    {
        Debug.Log("Starting client...");
        NetworkManager.Singleton.StartClient();
    }

    public void ClientConnectMessage(ulong connectionId)
    {
        Debug.Log("client " + connectionId + " connected.");
        Debug.Log($"total players: {NetworkManager.Singleton.ConnectedClients.Count}.");
        SendToClientRpc("Welcome to the server!");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}.");
        Debug.Log($"total players: {NetworkManager.Singleton.ConnectedClients.Count}.");
    }

    [Rpc(SendTo.NotServer)]
    public void SendToClientRpc(string message)
    {
        Debug.Log($"[CLIENT] Received message from server: {message}");
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            SubmitPlayerDataServerRpc(Globals.playerName, Globals.gender);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitPlayerDataServerRpc(
        string playerName,
        string gender,
        ServerRpcParams rpcParams = default
    )
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        Debug.Log($"[SERVER] Client {clientId} | Name: {playerName} | Gender: {gender}");
    }
}
