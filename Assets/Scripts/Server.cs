using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Server : NetworkBehaviour
{
    void Start()
    {
    string[] args = System.Environment.GetCommandLineArgs(); 
    Debug.Log("Command line arguments: " + args.Length);
        for (int i = 0; i < args.Length; i++) 
        { 
            if (args[i] == "-server" ) 
            { 
                Debug.Log("Running Server.");
                NetworkManager.Singleton.StartServer();
                // Serverines Logs
                NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectMessage;        
                SendClientRpc("Just a send");
            }
        }

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
