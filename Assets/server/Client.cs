using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



[System.Serializable]
public class BaseMessage
{
    public string type;
}

[System.Serializable]
public class PlayersMessage
{
    public string type;
    public int count;
}

[System.Serializable]
public class ScoreboardPlayer
{
    public int clientId;
    public string name;
    public int points;
}

[System.Serializable]
public class ScoreboardMessage
{
    public string type;
    public ScoreboardPlayer[] scoreboard;
}

[System.Serializable]
public class MessageData
{
    public string type;
    public string variable;
    public string gender;
}

public class Client : MonoBehaviour
{
    // [Header("Server")]
    [Header("ErrorMeesage")]
    public TextMeshProUGUI board; // Assign in Inspector
    public TextMeshProUGUI Console; // Assign in Inspector
    public void StartGame()
    {
        Send("start");
    }

    public void SendChoice(string choice)
    {
        Send("answer", choice);
    }
    

    void Send(string type, string variable="",string gender="")
    {
        string json = JsonUtility.ToJson(new MessageData { type = type,variable = variable, gender=gender });
        // Debug.Log(json.ToString());
        byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
        StaticData.stream.Write(bytes, 0, bytes.Length);
    }

    void Start()
    {

        Send("name", StaticData.playerName, StaticData.Gender);
    }
void HandleServerMessage(string json)
{
    // Step 1: Read only the type
    Debug.Log("Handling server message: " + json);
    BaseMessage baseMsg = JsonUtility.FromJson<BaseMessage>(json);
    // Debug.Log("Received message of type: " + baseMsg.type);
    switch (baseMsg.type)
    {
        case "players":
            PlayersMessage playersMsg =
                JsonUtility.FromJson<PlayersMessage>(json);
            board.text = "Prisijunge: " + playersMsg.count;
            break;

        // case "scoreboard":
        //     ScoreboardMessage scoreboardMsg =
        //         JsonUtility.FromJson<ScoreboardMessage>(json);
        //     HandleScoreboard(scoreboardMsg);
        //     break;

        default:
            Debug.LogWarning("Unknown message type: " + baseMsg.type);
            break;
    }
}

    

    void Update()
    {
        if (StaticData.stream != null && StaticData.stream.DataAvailable)
        {
            byte[] buffer = new byte[1024];
            int bytes = StaticData.stream.Read(buffer, 0, buffer.Length);
            string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
            // Debug.Log("Server says: " + msg);
            // if (board != null)
            // {
            Console.text = msg;
            HandleServerMessage(msg);
            // }
        }
    }

    void OnApplicationQuit()
    {
        StaticData.stream?.Close();
       StaticData.client?.Close();
    }
}
