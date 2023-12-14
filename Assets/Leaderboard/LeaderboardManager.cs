using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using System;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private string leaderboardID = "Splatterman_Leaderboard";
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private TMP_InputField nameInput, scoreInput;
    [SerializeField] private int scoreToAdd;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void AddScore(double score)
    {
        var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, score);
    }

    public async void GetScores()
    {
        var scoreResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID);
        var childCount = leaderboardContainer.childCount;
        for (int i = 0;  i < childCount; i++)
        {
            Destroy(leaderboardContainer.GetChild(i).gameObject);
        }

        foreach(var leaderboardEntry in scoreResponse.Results)
        {
            var GO = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            GO.GetComponent<leaderboardEntryController>().SetEntryValues(leaderboardEntry.Rank.ToString(), leaderboardEntry.PlayerName, leaderboardEntry.Score.ToString());
        }
    }

    public void DisplayLeaderboard()
    {
        GetScores();
    }

    public void AddScoreToLeaderboard()
    {
        scoreToAdd = int.Parse(scoreInput.text);
        AddScore(scoreToAdd);
        GetScores();
    }
}
