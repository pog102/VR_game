using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class QuizTcpClient : MonoBehaviour
{
    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    public string serverIp = "192.168.1.69";
    public int serverPort = 7777;
    public bool autoConnect = true;
    public bool autoJoin = true;
    public bool autoStart = false;

    public UnityEvent OnConnected;
    public UnityEvent OnDisconnected;
    public StringEvent OnRawMessage;
    public StringEvent OnMessageType;

    private TcpClient client;
    private NetworkStream stream;
    private Thread recvThread;
    private readonly Queue<string> incoming = new Queue<string>();
    private readonly object queueLock = new object();
    private volatile bool stopping;
    private bool joined;

    public bool IsConnected
    {
        get { return client != null && client.Connected; }
    }

    private void Start()
    {
        if (autoConnect)
        {
            Connect();
        }
    }

    private void Update()
    {
        DrainIncoming();

        if (autoJoin && !joined && IsConnected)
        {
            TrySendJoin();
        }

        if (autoStart && IsConnected)
        {
            SendStart();
            autoStart = false;
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void Connect()
    {
        if (IsConnected)
        {
            return;
        }

        try
        {
            client = new TcpClient();
            client.Connect(serverIp, serverPort);
            stream = client.GetStream();
            stopping = false;

            recvThread = new Thread(ReceiveLoop)
            {
                IsBackground = true
            };
            recvThread.Start();

            OnConnected?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[QuizTcpClient] Connect failed: " + ex.Message);
            Cleanup();
        }
    }

    public void Disconnect()
    {
        stopping = true;
        Cleanup();
    }

    public void SendStart()
    {
        SendRaw("{\"type\":\"start\"}");
    }

    public void SendAnswer(string choice)
    {
        if (string.IsNullOrWhiteSpace(choice))
        {
            return;
        }
        string normalized = choice.Trim().ToUpperInvariant();
        SendRaw("{\"type\":\"answer\",\"choice\":" + JsonEscape(normalized) + "}");
    }

    public void SendRaw(string json)
    {
        if (!IsConnected || stream == null)
        {
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[QuizTcpClient] Send failed: " + ex.Message);
            Disconnect();
        }
    }

    private void TrySendJoin()
    {
        string name = (Globals.playerName ?? "").Trim();
        string gender = (Globals.gender ?? "").Trim();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(gender))
        {
            return;
        }

        string payload = "{\"type\":\"name\",\"variable\":" + JsonEscape(name) + ",\"gender\":" + JsonEscape(gender) + "}";
        SendRaw(payload);
        joined = true;
    }

    private void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        StringBuilder sb = new StringBuilder();

        try
        {
            while (!stopping && client != null && client.Connected)
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    break;
                }

                sb.Append(Encoding.UTF8.GetString(buffer, 0, read));
                while (true)
                {
                    string current = sb.ToString();
                    int idx = current.IndexOf('\n');
                    if (idx < 0)
                    {
                        break;
                    }

                    string line = current.Substring(0, idx).Trim();
                    sb.Remove(0, idx + 1);
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    lock (queueLock)
                    {
                        incoming.Enqueue(line);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (!stopping)
            {
                Debug.LogWarning("[QuizTcpClient] Receive failed: " + ex.Message);
            }
        }
        finally
        {
            if (!stopping)
            {
                Disconnect();
            }
        }
    }

    private void DrainIncoming()
    {
        while (true)
        {
            string msg = null;
            lock (queueLock)
            {
                if (incoming.Count == 0)
                {
                    break;
                }
                msg = incoming.Dequeue();
            }

            if (msg == null)
            {
                break;
            }

            OnRawMessage?.Invoke(msg);

            string msgType = ExtractJsonString(msg, "type");
            if (!string.IsNullOrEmpty(msgType))
            {
                OnMessageType?.Invoke(msgType);
            }
        }
    }

    private static string JsonEscape(string value)
    {
        if (value == null)
        {
            return "\"\"";
        }

        StringBuilder sb = new StringBuilder();
        sb.Append('\"');
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            switch (c)
            {
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    if (c < 32)
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        sb.Append('\"');
        return sb.ToString();
    }

    private static string ExtractJsonString(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
        {
            return null;
        }

        string needle = "\"" + key + "\"";
        int idx = json.IndexOf(needle, StringComparison.Ordinal);
        if (idx < 0)
        {
            return null;
        }

        idx = json.IndexOf(':', idx + needle.Length);
        if (idx < 0)
        {
            return null;
        }

        idx++;
        while (idx < json.Length && char.IsWhiteSpace(json[idx]))
        {
            idx++;
        }

        if (idx >= json.Length || json[idx] != '"')
        {
            return null;
        }

        idx++;
        int start = idx;
        while (idx < json.Length)
        {
            if (json[idx] == '"' && json[idx - 1] != '\\')
            {
                break;
            }
            idx++;
        }

        if (idx >= json.Length)
        {
            return null;
        }

        string raw = json.Substring(start, idx - start);
        return raw.Replace("\\\"", "\"").Replace("\\\\", "\\");
    }

    private void Cleanup()
    {
        try
        {
            stream?.Close();
        }
        catch
        {
        }

        try
        {
            client?.Close();
        }
        catch
        {
        }

        stream = null;
        client = null;
        joined = false;
        OnDisconnected?.Invoke();
    }
}
