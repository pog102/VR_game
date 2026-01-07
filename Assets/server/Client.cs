using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// [System.Serializable]
// public class PlayerData
// {
//     public string type; // "name"
//     public string variable; // player's name
// }
[System.Serializable]
public class MessageData
{
    public string type;
    public string variable;
}

public class Client : MonoBehaviour
{
    [SerializeField]
    public string ServerIP = "192.168.4.1";
    public ushort ServerPort = 7777;

    public string name = "Ernis";
    private TcpClient client;
    private NetworkStream stream;

    // public Button myButton;

    public void StartGame()
    {
        // Debug.Log("Start Game");
        Send(new MessageData { type = "start" });
    }

    public void SelectChoice()
    {
        Send(new MessageData { type = "answer", variable = "C" });
    }

    // void SendJson(PlayerData data) { }

    void Send(MessageData data)
    {
        string json = JsonUtility.ToJson(data);
        Debug.Log(json.ToString());
        byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
        stream.Write(bytes, 0, bytes.Length);
    }

    void Start()
    {
        // myButton.onClick.AddListener(OnButtonClick);
        Connect();
        Send(new MessageData { type = "name", variable = name });
        // StartGame();
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
