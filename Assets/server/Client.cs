using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerData
{
    public string type; // "name"
    public string variable; // player's name
}

public class Client : MonoBehaviour
{
    [SerializeField]
    public string ServerIP = "192.168.1.69";
    public ushort ServerPort = 6969;
    private TcpClient client;
    private NetworkStream stream;
    public Button myButton;

    void OnButtonClick()
    {
        Send("Test Send");
    }

    void SendJson(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        Send(json);
    }

    void Start()
    {
        myButton.onClick.AddListener(OnButtonClick);
        Connect();
        SendJson(new PlayerData { type = "name", variable = "Sugoi22" });
    }

    void Connect()
    {
        client = new TcpClient(ServerIP, ServerPort);
        stream = client.GetStream();
    }

    void Send(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
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
