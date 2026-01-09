using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[System.Serializable]
public class MessageData
{
    public string type;
    public string variable;
    public string gender;
}

public class Client : MonoBehaviour
{
    public static Client Instance;
    [Header("Server")]
    public string ServerIP = "192.168.4.1";
    public ushort ServerPort = 7777;
    [Header("ErrorMeesage")]
    public GameObject ErrorMeesage;
    private TcpClient client;
    private NetworkStream stream;
    private ServerLabel serverLabel; // Reference to label in current scene
    public void StartGame()
    {
        Send("start");
    }

    public void SelectChoice(string choice)
    {
        Send("answer", choice);
    }

void Awake()
{
     if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
     Instance = this;
    DontDestroyOnLoad(gameObject);
}
    public void Send(string type, string variable="",string gender="")
    {
        string json = JsonUtility.ToJson(new MessageData { type = type,variable = variable, gender=gender });
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
            if (ErrorMeesage != null)
            {
                ErrorMeesage.SetActive(true);
            }
            // Debug.LogError("Neijungtas serveris/ne tas tinklas" );
            return;
        }

        // Send("name", name);
    }

    void Connect()
    {
        client = new TcpClient(ServerIP, ServerPort);
        stream = client.GetStream();
    }

    void Update()
    {
          if (serverLabel == null)
        {
                        serverLabel = FindObjectOfType<ServerLabel>();
        }
        if (stream != null && stream.DataAvailable)
        {
            byte[] buffer = new byte[1024];
            int bytes = stream.Read(buffer, 0, buffer.Length);
            string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
            Debug.Log("Server says: " + msg);
            if (serverLabel != null)
            {
                serverLabel.UpdateLabel(msg);
            }
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}
