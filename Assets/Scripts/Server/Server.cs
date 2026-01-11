using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Server : NetworkBehaviour
{
    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-server")
            {
                StartServer();
            }
        }
    }

    public void StartServer()
    {
        Debug.Log("Starting server...");
        NetworkManager.Singleton.StartServer();
        // Serverines Logs
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectMessage;
        SendClientRpc("Just a send");
    }

    public void StartClient()
    {
        Debug.Log("Starting client...");
        NetworkManager.Singleton.StartClient();
    }

    public void ClientConnectMessage(ulong connectionId)
    {
        // Serverines Logs
        Debug.Log("------ client " + connectionId + " connected.");
    }

    [ClientRpc]
    public void SendClientRpc(string message)
    {
        // Kliento Logs (siunciamas klientui is serverio)
        Debug.Log("Message from server: " + message);
    }
}
