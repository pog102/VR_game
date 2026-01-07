using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MessageData
{
    public string type;
    public string variable;
}

public class Client : MonoBehaviour
{
    [Header("Server")]
    public string ServerIP = "192.168.4.1";
    public ushort ServerPort = 7777;
    [Header("Player")]
    public string name = "Ernis";
    private TcpClient client;
    private NetworkStream stream;


    public void StartGame()
    {
        Send("start");
    }

    public void SelectChoice(string choice)
    {
        Send("answer", choice);
    }


    void Send(string type, string variable="")
    {
        string json = JsonUtility.ToJson(new MessageData { type = type,variable = variable });
        Debug.Log(json.ToString());
        byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
        stream.Write(bytes, 0, bytes.Length);
    }

    void Start()
    {
        try
        {
            Connect();
        }
        catch (SocketException e)
        {
            Debug.LogError("Neijungtas serveris/ne tas tinklas" );
            return;
        }

        Send("name", name);
    }

    void Connect()
    {
        client = new TcpClient(ServerIP, ServerPort);
        stream = client.GetStream();
    }

    void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            byte[] buffer = new byte[1024];
            int bytes = stream.Read(buffer, 0, buffer.Length);
            string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
            Debug.Log("Server says: " + msg);
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}
