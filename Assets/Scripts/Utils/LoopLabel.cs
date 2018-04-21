
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopLabel : MonoBehaviour
{
	public string[] contents;
	public float interval;

	UILabel lbl;
	int index = -1;

	float nextUpdate = 0;

	void Start () {
		lbl = GetComponent<UILabel>();

		if (interval > 0 && contents.Length > 0) {
			next();
			nextUpdate = Time.time + interval;
		}
	}

	void next() {
		int cnt = contents.Length;

		if (cnt == 0)
			return;

		index = (index + 1) % cnt;
		lbl.text = contents[index];
	}

	void Update () {
		float now = Time.time;

		if (nextUpdate > 0 && nextUpdate < now) {
			next ();

			nextUpdate = now + interval;
		}
	}
}
