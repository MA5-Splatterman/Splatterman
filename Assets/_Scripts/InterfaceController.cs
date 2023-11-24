using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Febucci.UI;

public class InterfaceController : MonoBehaviour {
	[SerializeField] private TMP_Text _interfaceJoinCode;
	[SerializeField] private GameObject _interfaceJoinCodeGroup;
	[SerializeField] private TMP_Text _interfaceTeamsValue;
	[SerializeField] private TMP_Text _roundTimer;
	[SerializeField] private TextAnimator_TMP _roundTimerAnims;
	[SerializeField] private TMP_Text _roundEndText;
	[SerializeField] private GameObject _roundEndObject;

	[SerializeField] private GameManager manager;

	private int redTeamNumber;
	private int blueTeamNumber;

    private void OnEnable()
    {
		if (manager == null)
        {
			manager = FindObjectOfType<GameManager>();
        }
		manager.curRedPlayers.OnValueChanged += RedPlayerTeamNumberChanged;
		manager.curBluePlayers.OnValueChanged += BluePlayerTeamNumberChanged;
		manager.OnGameEnd += OnGameEnd;
		manager.curTimeInSeconds.OnValueChanged += UpdateRoundTimer;
    }

    private void OnDisable()
    {
		manager.curRedPlayers.OnValueChanged -= RedPlayerTeamNumberChanged;
		manager.curBluePlayers.OnValueChanged -= BluePlayerTeamNumberChanged;
		manager.OnGameEnd -= OnGameEnd;
		manager.curTimeInSeconds.OnValueChanged -= UpdateRoundTimer;
	}

    public void SetJoinCode ( string joinCode ) {
		_interfaceJoinCode.text = joinCode;
		_interfaceJoinCodeGroup.SetActive(true);
	}

	private void OnGameEnd(TeamColor team)
    {
		_roundEndObject.SetActive(true);
        switch (team)
        {
			case TeamColor.RED:
				_roundEndText.text = "$<color=red>Red Team Wins";
				break;

			case TeamColor.BLUE:
				_roundEndText.text = "$<color=blue>Blue Team Wins";
				break;

			case TeamColor.NONE:
				_roundEndText.text = "<shake>" + "??No Team Wins??" + "</shake>";
				break;
        }
    }

	public void SetPlayersPerTeam(int red = 0, int blue = 0 ) {
		_interfaceTeamsValue.SetText($"<color=red>{red}</color> | <color=blue>{blue}</color>" );
	}

	private void RedPlayerTeamNumberChanged(int oldValue, int newValue)
    {
		UpdateCurPlayerCount(TeamColor.RED, newValue-oldValue);
    }
	
	private void BluePlayerTeamNumberChanged(int oldValue, int newValue)
    {
		UpdateCurPlayerCount(TeamColor.BLUE, newValue-oldValue);
    }

	public void UpdateCurPlayerCount(TeamColor team, int numberToChangeBy)
    {
        switch (team)
        {
			case TeamColor.RED:
				redTeamNumber += numberToChangeBy;
				break;

			case TeamColor.BLUE:
				blueTeamNumber += numberToChangeBy;
				break;
        }

		SetPlayersPerTeam(redTeamNumber, blueTeamNumber);

	}



	public void UpdateRoundTimer(int oldValue, int newValue)
    {
		_roundTimer.text = newValue.ToString();
		if (newValue <= 30)
		{
			_roundTimerAnims.SetText($"<pend><incr><flash>{newValue}</flash></incr></pend>");
		}
    }


}
