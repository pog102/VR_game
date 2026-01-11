using System.Net.Sockets;
using UnityEngine;

public class ShowErrorPingPanel : MonoBehaviour
{
    public string serverIP = "192.168.1.69";
    public ushort serverPort = 7777;
    public GameObject errorMessage;

    void Connect()
    {
        // TcpClient client = new TcpClient(serverIP, serverPort);
        //  StaticData.client = client;
        //  StaticData.stream = client.GetStream();
    }

    void Start()
    {
        CheckServer();
    }

    void CheckServer()
    {
        try
        {
            Connect();
        }
        catch (SocketException)
        {
            // errorMessage.SetActive(true);
        }
    }
}
