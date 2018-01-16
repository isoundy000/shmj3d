
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameAnim : MonoBehaviour {
	public string prefix = null;
	public float rate = 0.08f;
	public bool loop = false;
	public bool PlayOnLoad = false;
	public int count = 10;

	int index = 0;
	float nextFire = 0;
	UISprite mSprite = null;
	bool playing = false;

	Action onFinish = null;

	void Awake() {
		mSprite = transform.GetComponent<UISprite>();

		if (PlayOnLoad)
			playing = true;
	}

	public void reset() {
		if (prefix == null) {
			Debug.Log ("prefix null: " + transform.name);
		}

		if (mSprite == null)
			Debug.Log ("mSprite null: " + transform.name);
		
		mSprite.spriteName = prefix + 0;
		playing = false;
		index = 0;
		nextFire = 0;
	}

	public void run(Action cb) {
		playing = true;
		index = 0;
		nextFire = 0;
		onFinish = cb;
	}

	void Update() {
		if (!playing)
			return;
		
		if (Time.time < nextFire)
			return;

		nextFire = Time.time + rate;

		mSprite.spriteName = prefix + index;

		if (index < count - 1 || loop)
			index = (index + 1) % count;
		else {
			playing = false;
			if (onFinish != null)
				onFinish();
		}
	}
}
