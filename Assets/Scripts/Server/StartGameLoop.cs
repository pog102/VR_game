using System;
using System.Collections.Generic;
using System.Linq; // Important: This allows the .OrderByDescending method
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class StartGameLoop : NetworkBehaviour
{
    [Serializable]
    public struct Question
    {
        public string questionText;
        public string[] answers; // Always 4 answers (A, B, C, D)
        public int correctAnswerIndex;
    }

    public enum GameState
    {
        Lobby,
        Asking,
        Results,
        GameOver,
    }

    [Header("Quiz Content")]
    [SerializeField]
    private List<Question> quizData;

    [Header("References")]
    [SerializeField]
    private TextMeshProUGUI whiteboardText;

    [SerializeField]
    private TextMeshProUGUI timerText;

    [SerializeField]
    private Transform[] chairPositions;

    // Sync Variables
    private NetworkVariable<GameState> currentState = new NetworkVariable<GameState>(
        GameState.Lobby
    );
    private NetworkVariable<int> currentQuestionIndex = new NetworkVariable<int>(0);
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>(10f);

    // Server-side only score tracking
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    // public struct PlayerScoreData : INetworkSerializable
    // {
    //     public Unity.Collections.FixedString32Bytes playerName;
    //     public int score;
    //
    //     // Required for Netcode to send this data over the internet
    //     public void NetworkSerialize<T>(T serializer)
    //         where T : IReaderWriter
    //     {
    //         serializer.SerializeValue(ref playerName);
    //         serializer.SerializeValue(ref score);
    //     }
    // }

    #region Game Logic
    public void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClinetConnectedClientRpc;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedClientRpc;
    }

    [ClientRpc]
    public void OnClinetConnectedClientRpc(ulong connectionId)
    {
        SubmitPlayerDataServerRpc(Globals.playerName, Globals.gender);
        if (currentState.Value != GameState.Lobby)
            return;
        whiteboardText.text = $"Total:  {NetworkManager.Singleton.ConnectedClients.Count}";
    }

    [ClientRpc]
    public void OnClientDisconnectedClientRpc(ulong connectionId)
    {
        if (currentState.Value != GameState.Lobby)
            return;
        whiteboardText.text = $"Total:  {NetworkManager.Singleton.ConnectedClients.Count}";
    }

    // public override void OnNetworkSpawn()
    // {
    //     if (IsClient)
    //     {
    //         SubmitPlayerDataServerRpc(Globals.playerName, Globals.gender);
    //     }
    // }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitPlayerDataServerRpc(
        string playerName,
        string gender,
        ServerRpcParams rpcParams = default
    )
    {
        ulong ClientId = rpcParams.Receive.SenderClientId;
        playerNames.Add(ClientId, playerName);
        // gender.Add(ClientId,gender);
        playerScores.Add(ClientId, 0);
        Debug.Log($"ID:{ClientId}|Name:{playerNames[ClientId]}|Score:{playerScores[ClientId]}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (currentState.Value != GameState.Lobby)
            return;

        // TeleportPlayersToChairsClientRpc();
        currentState.Value = GameState.Results;
        StartNextQuestion();
    }

    // private void StartNextAnswer()
    // {
    //     if (currentQuestionIndex.Value >= quizData.Count)
    //     {
    //         currentState.Value = GameState.GameOver;
    //         // ShowLeaderboardClientRpc();
    //         EndGameAndShowScores();
    //         return;
    //     }
    //
    //     currentState.Value = GameState.Results;
    //     timeRemaining.Value = 6f;
    //
    //     UpdateWhiteboardClientRpc(currentQuestionIndex.Value);
    //     // ShowCorrectAnswer(currentQuestionIndex.Value);
    // }

    private void StartNextQuestion()
    {
        if (currentQuestionIndex.Value >= quizData.Count)
        {
            currentState.Value = GameState.GameOver;
            // ShowLeaderboardClientRpc();
            EndGameAndShowScores();
            return;
        }
        if (currentState.Value == GameState.Results)
        {
            timeRemaining.Value = 10f;
            currentState.Value = GameState.Asking;
            UpdateWhiteboardClientRpc(currentQuestionIndex.Value);
        }
        else
        {
            timeRemaining.Value = 6f;
            currentState.Value = GameState.Results;
            ShowCorrectAnswerClientRpc(currentQuestionIndex.Value);
            currentQuestionIndex.Value++;
        }
    }

    // [ClientRpc]
    // private void DisplayLeaderboardClientRpc(PlayerScoreData[] topScores)
    // {
    //     string leaderboardString = "--- TOP 5 SCORES ---\n";
    //
    //     for (int i = 0; i < topScores.Length; i++)
    //     {
    //         leaderboardString += $"{i + 1}. {topScores[i].playerName}: {topScores[i].score}\n";
    //     }
    //
    //     // Print to Unity Console
    //     Debug.Log(leaderboardString);
    //
    //     // Print to your VR Whiteboard UI
    //     if (whiteboardText != null)
    //     {
    //         whiteboardText.text = leaderboardString;
    //     }
    // }

    private void Update()
    {
        if (!IsServer)
            return;

        if (currentState.Value == GameState.Results || currentState.Value == GameState.Asking)
        {
            timeRemaining.Value -= Time.deltaTime;
            UpdateTimerUIClientRpc(Mathf.CeilToInt(timeRemaining.Value));

            if (timeRemaining.Value <= 0)
            {
                StartNextQuestion();
                // ShowCorrectAnswer(currentQuestionIndex.Value);
            }
        }

        // if (currentState.Value == GameState.Asking)
        // {
        //     timeRemaining.Value -= Time.deltaTime;
        //     UpdateTimerUIClientRpc(Mathf.CeilToInt(timeRemaining.Value));
        //
        //     if (timeRemaining.Value <= 0)
        //     {
        //         currentQuestionIndex.Value++;
        //         StartNextQuestion();
        //     }
        // }
    }

    #endregion

    #region RPCs (Remote Procedure Calls)

    [ClientRpc]
    private void TeleportPlayersToChairsClientRpc()
    {
        // On each client, find their local player object and move them
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayer != null)
        {
            // Simple teleport (use a specific index or logic to avoid stacking)
            int index = (int)NetworkManager.Singleton.LocalClientId % chairPositions.Length;
            localPlayer.transform.position = chairPositions[index].position;
            localPlayer.transform.rotation = chairPositions[index].rotation;
        }
    }

    [ClientRpc]
    private void ShowCorrectAnswerClientRpc(int questionIndex)
    {
        char ans;
        switch (quizData[questionIndex].correctAnswerIndex)
        {
            case 0:
                ans = 'A';
                break;
            case 1:
                ans = 'B';
                break;
            case 2:
                ans = 'C';
                break;
            case 3:
                ans = 'D';
                break;
            default:
                ans = 'E';
                break;
        }
        whiteboardText.text = "Correct Answer is: " + ans;
    }

    [ClientRpc]
    private void UpdateWhiteboardClientRpc(int questionIndex)
    {
        var q = quizData[questionIndex];
        whiteboardText.text =
            $"{q.questionText}\n\n"
            + $"A: {q.answers[0]} | B: {q.answers[1]}\n"
            + $"C: {q.answers[2]} | D: {q.answers[3]}";
    }

    [ClientRpc]
    private void UpdateTimerUIClientRpc(int seconds)
    {
        timerText.text = $"Time: {seconds}s";
    }

    public void SubmitAnswer(int answerIndex)
    {
        SubmitAnswerServerRpc(answerIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitAnswerServerRpc(int answerIndex, ServerRpcParams rpcParams = default)
    {
        if (currentState.Value != GameState.Asking)
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        var currentQ = quizData[currentQuestionIndex.Value];

        if (answerIndex == currentQ.correctAnswerIndex)
        {
            // Calculate score: Points = 100 + (seconds remaining * 10)
            int points = 100 + Mathf.RoundToInt(timeRemaining.Value * 10);
            playerScores[clientId] += points;
            Debug.Log($"Client {clientId} got it right! Total: {playerScores[clientId]}");
        }
    }

    // [ServerRpc(RequireOwnership = false)]
    public void EndGameAndShowScores()
    {
        // 1. The Server builds the string because the Server has the data
        var sortedScores = playerScores.OrderByDescending(entry => entry.Value).ToList();

        // 2. Build the string to send to clients
        string leaderboardText = "--- TOP 5 PLAYERS ---\n";

        for (int i = 0; i < sortedScores.Count; i++)
        {
            if (i >= 5)
                break; // Only take the top 5

            ulong id = sortedScores[i].Key;
            int score = sortedScores[i].Value;

            // Get the name from your names dictionary

            leaderboardText += $"{i + 1}. {playerNames[id]}: {score} pts\n";
        }
        // 2. Send that finished string to the clients
        ShowLeaderboardClientRpc(leaderboardText);
    }

    [ClientRpc]
    private void ShowLeaderboardClientRpc(string finalScores)
    {
        Debug.Log(finalScores);
        whiteboardText.text = "GAME OVER!\n" + finalScores;
        timerText.text = "0";
    }

    #endregion
}
