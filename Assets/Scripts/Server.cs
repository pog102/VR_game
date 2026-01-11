using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Server : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
                // StartCoroutine(PeriodicLog());
                SendClientRpc("Just a send");
                // serverOff=false;
            }
        }
           // if (serverOff)  
           //  { 
           //      Debug.Log("Running Client.");
           //      NetworkManager.Singleton.StartClient();
           //      // Serverines Logs
           //      // NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectMessage;        
           //      // StartCoroutine(PeriodicLog());
           //      // SendClientRpc("Just a send");
           //  }

    }
    public void ClientConnectMessage(ulong connectionId)
    {
        // Serverines Logs
        Debug.Log("------ client " + connectionId + " connected.");
    }
    public IEnumerator PeriodicLog()
    {
        string msg = "Holla";
        yield return new WaitForSeconds(10f);
        while (true)
        {
        yield return new WaitForSeconds(10f);
            Debug.Log("Sending"+msg);
            SendClientRpc(msg);
        }
    }
    [ClientRpc]
    public void SendClientRpc(string message)
    {
        // Kliento Logs
        Debug.Log("Message from server: " + message);
    }
}
