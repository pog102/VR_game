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
    private AudioSource SoundCorrect;

    [SerializeField]
    private AudioSource SoundBad;

    [SerializeField]
    private TextMeshProUGUI timerText;

    [SerializeField]
    private Transform[] chairPositions;
    private HashSet<ulong> playersWhoAnswered = new HashSet<ulong>();

    // Sync Variables
    private NetworkVariable<GameState> currentState = new NetworkVariable<GameState>(
        GameState.Lobby
    );
    private NetworkVariable<int> currentQuestionIndex = new NetworkVariable<int>(0);

    // private NetworkVariable<bool> isSumbmited = new NetworkVariable<bool>(false);
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>(30f);

    // Server-side only score tracking
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    #region Game Logic
    public void Start()
    // public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClinetConnectedClientRpc;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedClientRpc;
    }

    [ClientRpc]
    public void OnClinetConnectedClientRpc(ulong connectionId)
    {
        if (currentState.Value != GameState.Lobby)
        {
            return;
        }
        SubmitPlayerDataServerRpc(Globals.playerName, Globals.gender);
        // whiteboardText.text = $"Total:  {NetworkManager.Singleton.ConnectedClients.Count}";
        timerText.text = $"{NetworkManager.Singleton.ConnectedClients.Count}";
    }

    [ClientRpc]
    public void OnClientDisconnectedClientRpc(ulong connectionId)
    {
        if (currentState.Value != GameState.Lobby)
            return;
        whiteboardText.text = $"Total:  {NetworkManager.Singleton.ConnectedClients.Count}";
    }

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
        // currentQuestionIndex.Value = 0;
        currentState.Value = GameState.Results;
        StartNextQuestion();
    }

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
            playersWhoAnswered.Clear();
            timeRemaining.Value = 30f;
            currentState.Value = GameState.Asking;
            UpdateWhiteboardClientRpc(currentQuestionIndex.Value);
        }
        else
        {
            timeRemaining.Value = 5f;
            currentState.Value = GameState.Results;
            ShowCorrectAnswerClientRpc(currentQuestionIndex.Value);
            currentQuestionIndex.Value++;
            // Resets the emision of the button
        }
    }

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
            // int index = (int)NetworkManager.Singleton.LocalClientId % chairPositions.Length;
            int index = (int)NetworkManager.Singleton.LocalClientId;
            localPlayer.transform.position = chairPositions[index].position;
            localPlayer.transform.rotation = chairPositions[index].rotation;
        }
    }

    [ClientRpc]
    private void ShowCorrectAnswerClientRpc(int questionIndex)
    {
        // ButtonVr.mat.DisableKeyword("_EMISSION");
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
        if (ButtonVr.mat != null)
        {
            ButtonVr.mat.DisableKeyword("_EMISSION");
        }
    }

    [ClientRpc]
    private void UpdateTimerUIClientRpc(int seconds)
    {
        timerText.text = $"{seconds}";
    }

    public void SubmitAnswer(int answerIndex)
    {
        SubmitAnswerServerRpc(answerIndex);
    }

    private void CheckAllPlayersAnswered()
    {
        // Compare count of answers to count of connected clients
        if (playersWhoAnswered.Count >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            // Force the timer to 0 so the Update loop or logic triggers the next state
            timeRemaining.Value = 0;
            StartNextQuestion();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitAnswerServerRpc(int answerIndex, ServerRpcParams rpcParams = default)
    {
        if (currentState.Value != GameState.Asking)
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        // 1. Prevent double-voting for the same question
        if (playersWhoAnswered.Contains(clientId))
            return;

        // 2. Register the answer
        playersWhoAnswered.Add(clientId);
        var currentQ = quizData[currentQuestionIndex.Value];

        if (answerIndex == currentQ.correctAnswerIndex)
        {
            // Calculate score: Points = 100 + (seconds remaining * 10)
            PlaySoundClientRpc(true);
            int points = 100 + Mathf.RoundToInt(timeRemaining.Value * 10);
            playerScores[clientId] += points;
            Debug.Log($"Client {clientId} got it right! Total: {playerScores[clientId]}");
        }
        else
        {
            PlaySoundClientRpc(false);
        }
        CheckAllPlayersAnswered();
    }

    [ClientRpc]
    private void PlaySoundClientRpc(bool isCorrect)
    {
        if (isCorrect)
        {
            SoundCorrect.Play();
        }
        else
        {
            SoundBad.Play();
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
