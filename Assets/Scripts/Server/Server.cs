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
        Namie,
        Kolegijoje,
    }
    // [SerializeField] private IPOptions selectedIP = IPOptions.Server;
#if UNITY_EDITOR
    [Header("Editor only")]
    [SerializeField]
    private IPOptions selectedIP = IPOptions.Namie;
#endif

    // [SerializeField]
    // private IPOptions selectedIP = IPOptions.Server;

    private string ClientIP
    {
        get
        {
#if UNITY_EDITOR
            return selectedIP == IPOptions.Namie ? "192.168.1.69" : "172.16.19.167";
#elif UNITY_ANDROID
            return "192.168.4.1"; // or public IP
#else
            return "0.0.0.0";
#endif
        }
    }

    [SerializeField]
    UnityTransport transport;

    void Start()
    {
#if UNITY_EDITOR || UNITY_ANDROID
        SetIpAddress();
        StartClient();
#elif UNITY_SERVER
        StartServer();
#endif
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
}
