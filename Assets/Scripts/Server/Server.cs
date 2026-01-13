using System;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public class Server : MonoBehaviour
{
    [Header("TCP Server")]
    public string serverIp = "192.168.4.1";
    public int serverPort = 7777;
    public string playerName = "Player";
    public string gender = "male";
    public TMP_InputField nameInput;

    [Header("Remote Player")]
    public NetwrokPlayer remotePlayerPrefab;
    public Transform remoteParent;
    public float remoteSmoothing = 12f;

    [Header("Pose")]
    public float sendRateHz = 20f;

    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _recvThread;
    private readonly StringBuilder _buffer = new StringBuilder();
    private readonly object _sendLock = new object();
    private readonly ConcurrentQueue<Dictionary<string, object>> _inbox =
        new ConcurrentQueue<Dictionary<string, object>>();
    private readonly Dictionary<int, NetwrokPlayer> _remotes = new Dictionary<int, NetwrokPlayer>();
    private int _clientId = -1;
    private float _nextSendTime = 0f;
    private bool _missingPrefabLogged = false;

    private void OnDestroy()
    {
        Disconnect();
    }

    public void StartServer()
    {
        Debug.Log("TCP mode: start the server on Raspberry Pi, then use StartClient here.");
    }

    public void StartClient()
    {
        if (_client != null)
        {
            return;
        }

        if (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
        {
            playerName = nameInput.text.Trim();
        }

        Connect();
    }

    private void Update()
    {
        ProcessInbox();
        SendPoseIfReady();
    }

    private void Connect()
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(serverIp, serverPort);
            _stream = _client.GetStream();
            _recvThread = new Thread(RecvLoop) { IsBackground = true };
            _recvThread.Start();

            Send(new Dictionary<string, object>
            {
                ["type"] = "name",
                ["variable"] = playerName,
                ["gender"] = gender
            });

            Debug.Log($"Connected to {serverIp}:{serverPort} as {playerName} ({gender})");
        }
        catch (Exception exc)
        {
            Debug.LogError($"Connect failed: {exc.Message}");
            Disconnect();
        }
    }

    private void Disconnect()
    {
        try
        {
            if (_client != null && _client.Connected)
            {
                Send(new Dictionary<string, object> { ["type"] = "exit" });
            }
        }
        catch
        {
        }

        try
        {
            _stream?.Close();
        }
        catch
        {
        }

        try
        {
            _client?.Close();
        }
        catch
        {
        }

        _client = null;
        _stream = null;
        _recvThread = null;
        _clientId = -1;
    }

    private void RecvLoop()
    {
        byte[] recvBuffer = new byte[2048];
        while (_client != null)
        {
            int read;
            try
            {
                read = _stream.Read(recvBuffer, 0, recvBuffer.Length);
            }
            catch
            {
                break;
            }

            if (read <= 0)
            {
                break;
            }

            _buffer.Append(Encoding.UTF8.GetString(recvBuffer, 0, read));
            while (true)
            {
                string current = _buffer.ToString();
                int idx = current.IndexOf('\n');
                if (idx < 0)
                {
                    break;
                }
                string line = current.Substring(0, idx);
                _buffer.Remove(0, idx + 1);
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                object parsed = MiniJSON.Json.Deserialize(line);
                if (parsed is Dictionary<string, object> dict)
                {
                    _inbox.Enqueue(dict);
                }
            }
        }
    }

    private void ProcessInbox()
    {
        while (_inbox.TryDequeue(out Dictionary<string, object> msg))
        {
            if (!msg.TryGetValue("type", out object typeObj))
            {
                continue;
            }
            string type = typeObj as string;
            if (type == null)
            {
                continue;
            }

            if (type == "you_are")
            {
                if (TryGetInt(msg, "clientId", out int cid))
                {
                    _clientId = cid;
                }
            }
            else if (type == "pose")
            {
                HandlePose(msg);
            }
            else if (type == "pose_all")
            {
                HandlePoseAll(msg);
            }
            else if (type == "pose_remove")
            {
                if (TryGetInt(msg, "clientId", out int cid))
                {
                    RemoveRemote(cid);
                }
            }
            else if (type == "error")
            {
                if (msg.TryGetValue("message", out object err))
                {
                    Debug.LogWarning($"Server error: {err}");
                }
            }
        }
    }

    private void HandlePoseAll(Dictionary<string, object> msg)
    {
        if (!msg.TryGetValue("poses", out object posesObj))
        {
            return;
        }
        if (!(posesObj is List<object> poses))
        {
            return;
        }
        foreach (object item in poses)
        {
            if (item is Dictionary<string, object> poseMsg)
            {
                HandlePose(poseMsg);
            }
        }
    }

    private void HandlePose(Dictionary<string, object> msg)
    {
        if (!TryGetInt(msg, "clientId", out int cid))
        {
            return;
        }
        if (cid == _clientId)
        {
            return;
        }
        if (!TryReadPose(msg, out PoseData pose))
        {
            return;
        }

        NetwrokPlayer remote = GetOrCreateRemote(cid);
        if (remote != null)
        {
            remote.ApplyRemotePose(pose);
        }
    }

    private NetwrokPlayer GetOrCreateRemote(int clientId)
    {
        if (_remotes.TryGetValue(clientId, out NetwrokPlayer existing))
        {
            return existing;
        }
        if (remotePlayerPrefab == null)
        {
            if (!_missingPrefabLogged)
            {
                Debug.LogError("Remote player prefab not set on Server component.");
                _missingPrefabLogged = true;
            }
            return null;
        }
        NetwrokPlayer remote = Instantiate(remotePlayerPrefab, remoteParent);
        remote.name = $"RemotePlayer_{clientId}";
        remote.SetLocal(false);
        remote.RemoteSmoothing = remoteSmoothing;
        _remotes[clientId] = remote;
        return remote;
    }

    private void RemoveRemote(int clientId)
    {
        if (_remotes.TryGetValue(clientId, out NetwrokPlayer remote))
        {
            _remotes.Remove(clientId);
            if (remote != null)
            {
                Destroy(remote.gameObject);
            }
        }
    }

    private void SendPoseIfReady()
    {
        if (_client == null || _stream == null)
        {
            return;
        }
        if (VrRigReference.Singleton == null)
        {
            return;
        }
        if (Time.time < _nextSendTime)
        {
            return;
        }

        _nextSendTime = Time.time + (1f / Mathf.Max(1f, sendRateHz));

        Dictionary<string, object> msg = new Dictionary<string, object>
        {
            ["type"] = "pose",
            ["root"] = BuildPosePart(VrRigReference.Singleton.root),
            ["head"] = BuildPosePart(VrRigReference.Singleton.head),
            ["left"] = BuildPosePart(VrRigReference.Singleton.leftHand),
            ["right"] = BuildPosePart(VrRigReference.Singleton.rightHand)
        };

        Send(msg);
    }

    private Dictionary<string, object> BuildPosePart(Transform t)
    {
        if (t == null)
        {
            return new Dictionary<string, object>
            {
                ["p"] = new[] { 0f, 0f, 0f },
                ["r"] = new[] { 0f, 0f, 0f, 1f }
            };
        }

        Vector3 p = t.position;
        Quaternion r = t.rotation;
        return new Dictionary<string, object>
        {
            ["p"] = new[] { p.x, p.y, p.z },
            ["r"] = new[] { r.x, r.y, r.z, r.w }
        };
    }

    private void Send(Dictionary<string, object> payload)
    {
        string json = MiniJSON.Json.Serialize(payload);
        byte[] data = Encoding.UTF8.GetBytes(json + "\n");
        lock (_sendLock)
        {
            try
            {
                _stream.Write(data, 0, data.Length);
            }
            catch
            {
                Disconnect();
            }
        }
    }

    private bool TryReadPose(Dictionary<string, object> msg, out PoseData pose)
    {
        pose = new PoseData();
        if (!TryGetPosePart(msg, "root", out pose.rootPos, out pose.rootRot))
        {
            return false;
        }
        if (!TryGetPosePart(msg, "head", out pose.headPos, out pose.headRot))
        {
            return false;
        }
        if (!TryGetPosePart(msg, "left", out pose.leftPos, out pose.leftRot))
        {
            return false;
        }
        if (!TryGetPosePart(msg, "right", out pose.rightPos, out pose.rightRot))
        {
            return false;
        }
        return true;
    }

    private bool TryGetPosePart(Dictionary<string, object> msg, string key, out Vector3 pos, out Quaternion rot)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;
        if (!msg.TryGetValue(key, out object partObj))
        {
            return false;
        }
        if (!(partObj is Dictionary<string, object> part))
        {
            return false;
        }
        if (!TryReadVector3(part, "p", out pos))
        {
            return false;
        }
        if (!TryReadQuaternion(part, "r", out rot))
        {
            return false;
        }
        return true;
    }

    private bool TryReadVector3(Dictionary<string, object> obj, string key, out Vector3 value)
    {
        value = Vector3.zero;
        if (!obj.TryGetValue(key, out object arrObj))
        {
            return false;
        }
        if (!(arrObj is List<object> arr) || arr.Count != 3)
        {
            return false;
        }
        value = new Vector3(ToFloat(arr[0]), ToFloat(arr[1]), ToFloat(arr[2]));
        return true;
    }

    private bool TryReadQuaternion(Dictionary<string, object> obj, string key, out Quaternion value)
    {
        value = Quaternion.identity;
        if (!obj.TryGetValue(key, out object arrObj))
        {
            return false;
        }
        if (!(arrObj is List<object> arr) || arr.Count != 4)
        {
            return false;
        }
        value = new Quaternion(ToFloat(arr[0]), ToFloat(arr[1]), ToFloat(arr[2]), ToFloat(arr[3]));
        return true;
    }

    private bool TryGetInt(Dictionary<string, object> obj, string key, out int value)
    {
        value = 0;
        if (!obj.TryGetValue(key, out object raw))
        {
            return false;
        }
        if (raw is long l)
        {
            value = (int)l;
            return true;
        }
        if (raw is int i)
        {
            value = i;
            return true;
        }
        if (raw is double d)
        {
            value = (int)d;
            return true;
        }
        return int.TryParse(raw.ToString(), out value);
    }

    private float ToFloat(object raw)
    {
        if (raw is float f)
        {
            return f;
        }
        if (raw is double d)
        {
            return (float)d;
        }
        if (raw is long l)
        {
            return l;
        }
        if (raw is int i)
        {
            return i;
        }
        float.TryParse(raw.ToString(), out float val);
        return val;
    }
}
