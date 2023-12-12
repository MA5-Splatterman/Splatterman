using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Febucci.UI;
using Unity.Netcode;

public class InterfaceController : MonoBehaviour
{
	[SerializeField] private TMP_Text _interfaceJoinCode;
	[SerializeField] private GameObject _interfaceJoinCodeGroup;
	[SerializeField] private TMP_Text _interfaceTeamsValue;
	[SerializeField] private TMP_Text _roundTimer;
	[SerializeField] private TextAnimator_TMP _roundTimerAnims;
	[SerializeField] private TMP_Text _roundEndText;
	[SerializeField] private GameObject _roundEndObject;

	[SerializeField] private GameManager manager;


	private void OnEnable()
	{
		if (manager == null)
		{
			manager = FindObjectOfType<GameManager>();
		}
		SetJoinCode();
		manager.curRedPlayers.OnValueChanged += PlayerTeamNumberChanged;
		manager.curBluePlayers.OnValueChanged += PlayerTeamNumberChanged;
		manager.OnGameEnd += OnGameEnd;
		manager.curTimeInSeconds.OnValueChanged += UpdateRoundTimer;
		SetPlayersPerTeam(manager.curRedPlayers.Value, manager.curBluePlayers.Value);
	}

	private void OnDisable()
	{
		manager.curRedPlayers.OnValueChanged -= PlayerTeamNumberChanged;
		manager.curBluePlayers.OnValueChanged -= PlayerTeamNumberChanged;
		manager.OnGameEnd -= OnGameEnd;
		manager.curTimeInSeconds.OnValueChanged -= UpdateRoundTimer;
	}

	public void SetJoinCode()
	{
		_interfaceJoinCode.text = RelayManager.JoinCode;
		_interfaceJoinCodeGroup.SetActive(true);
	}
	[ClientRpc]
	private void OnGameEndClientRpc(TeamColor team)
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
	private void OnGameEnd(TeamColor team)
	{
		OnGameEndClientRpc(team);
	}

	public void SetPlayersPerTeam(int red = 0, int blue = 0)
	{
		_interfaceTeamsValue.SetText($"<color=red>{red}</color> | <color=blue>{blue}</color>");
	}

	private void PlayerTeamNumberChanged(int oldValue, int newValue)
	{
		SetPlayersPerTeam(manager.curRedPlayers.Value, manager.curBluePlayers.Value);
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
