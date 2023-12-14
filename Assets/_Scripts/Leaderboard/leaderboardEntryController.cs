using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class leaderboardEntryController : MonoBehaviour
{
    [SerializeField] private TMP_Text leaderboardRank;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text points;

    public void SetEntryValues(string rank, string playerName, string points)
    {
        leaderboardRank.text = rank;
        this.playerName.text = playerName;
        this.points.text = points;
    }
}
