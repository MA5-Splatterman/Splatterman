using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InterfaceController : MonoBehaviour {
	[SerializeField] private TMP_Text _interfaceJoinCode;
	[SerializeField] private GameObject _interfaceJoinCodeGroup;
	[SerializeField] private TMP_Text _interfaceTeamsValue;


	public void SetJoinCode ( string joinCode ) {
		_interfaceJoinCode.text = joinCode;
		_interfaceJoinCodeGroup.SetActive(true);
	}

	public void SetPlayersPerTeam(int red = 0, int blue = 0 ) {
		_interfaceTeamsValue.SetText($"<color=Red>{red}</color> / <color=blue>{blue}</color>" );
	}
}
