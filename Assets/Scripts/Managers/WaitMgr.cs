
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitMgr : MonoBehaviour {

	static WaitMgr mInstance = null;
	public static WaitMgr GetInstance() { return mInstance; }

	float mCount = 0;

	UILabel lblCount;

	void Awake() {
		mInstance = this;

		lblCount = transform.Find ("count").GetComponent<UILabel> ();

		hide ();
	}

	void show(float count) {
		mCount = Time.time + count;

		lblCount.text = "" + (int)count;

		gameObject.SetActive (true);
	}

	void hide() {
		mCount = 0;
		gameObject.SetActive (false);
	}

	public static void Show(float count = 60.0f) {
		mInstance.show (count);
	}

	public static void Hide() {
		mInstance.hide ();
	}

	void Update() {
		if (mCount == 0)
			return;

		float now = Time.time;

		if (mCount > now) {
			lblCount.text = "" + (int)(mCount - now);
		} else {
			hide ();
		}
	}
}


