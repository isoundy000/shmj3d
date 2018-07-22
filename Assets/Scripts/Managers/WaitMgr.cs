
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

		hide ();
	}

	void show(string text, float count) {
		mCount = Time.time + count;

		var ob = GameObject.Find("Waiting");
		if (ob == null)
			return;
		
		var tm = ob.transform;

		tm.Find ("notice").GetComponent<UILabel>().text = text;
		lblCount = tm.Find ("count").GetComponent<UILabel> ();
		lblCount.text = "" + (int)count;

		PUtils.activeChildren(tm);

		Debug.Log ("WaitMgr show");
	}

	void hide() {
		mCount = 0;

		var ob = GameObject.Find("Waiting");
		if (ob == null)
			return;

		PUtils.activeChildren (ob, false);

		lblCount = null;

		Debug.Log ("WaitMgr hide");
	}

	public static void Show(string text, float count = 60.0f) {
		Loom.QueueOnMainThread (() => {
			mInstance.show (text, count);
		});
	}

	public static void Hide() {
		Loom.QueueOnMainThread (() => {
			mInstance.hide ();
		});
	}

	void Update() {
		if (mCount == 0)
			return;

		float now = Time.time;

		if (mCount > now) {
			if (lblCount != null)
				lblCount.text = "" + (int)(mCount - now);
		} else {
			hide ();
		}
	}
}


