using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InterfaceController : MonoBehaviour {
	[SerializeField] private TMP_Text _interfaceJoinCode;
	[SerializeField] private GameObject _interfaceJoinCodeGroup;

	public void SetJoinCode ( string joinCode ) {
		_interfaceJoinCode.text = joinCode;
		_interfaceJoinCodeGroup.SetActive(true);
	}
}
