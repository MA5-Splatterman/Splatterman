using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InterfaceController : MonoBehaviour {
	[SerializeField] private TMP_Text _interfaceJoinCode;
	[SerializeField] private GameObject _interfaceJoinCodeGroup;
	[SerializeField] private TMP_Text _interfaceTeamsValue;
	[SerializeField] private TMP_Text _roundTimer;

    private void OnEnable()
    {
        
    }

    public void SetJoinCode ( string joinCode ) {
		_interfaceJoinCode.text = joinCode;
		_interfaceJoinCodeGroup.SetActive(true);
	}

	public void SetPlayersPerTeam(int red = 0, int blue = 0 ) {
		_interfaceTeamsValue.SetText($"<color=red>{red}</color> | <color=blue>{blue}</color>" );
	}

	public void UpdateRoundTimer(int time)
    {
		_roundTimer.text = time.ToString();
		if (time <= 30)
        {
			_roundTimer.text = "<pend>" + _roundTimer.text + "</pend>";
        }
    }


}
