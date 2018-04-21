
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeShow : MonoBehaviour {
	UILabel label;

	void Start () {
		label = GetComponent<UILabel>();
	}

	void updateText() {
		string format = "HH:mm";
		DateTime dd = DateTime.Now;

		label.text = dd.ToString(format);
	}

	void Update () {
		updateText ();
	}
}
