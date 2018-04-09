
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeShow : MonoBehaviour {

	DateTime startTime;
	UILabel label;

	void Start () {
		label = GetComponent<UILabel>();

		startTime = TimeZone.CurrentTimeZone.ToLocalTime (new DateTime (1970, 1, 1));
	}

	void updateText() {
		string format = "HH:mm";
		int secs = (int)((DateTime.Now.Ticks - DateTime.Parse("1970-01-01").Ticks) / 10000000);
		DateTime dt = startTime.AddSeconds(secs);

		label.text = dt.ToString(format);
	}

	void Update () {
		updateText ();
	}
}
