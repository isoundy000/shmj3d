using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceAnimation : MonoBehaviour {

	public bool ActivateWait = true;

	float fireRate = 0.08f;
	int index = 0;
	float nextFire = 0;

	UISprite mSprite = null;

	void Awake() {
		mSprite = GetComponent<UISprite>();
		mSprite.enabled = ActivateWait;
	}

	void Update() {
		if (!ActivateWait) {
			mSprite.enabled = false;
			return;
		}

		mSprite.enabled = true;

		if (Time.time < nextFire)
			return;

		nextFire = Time.time + fireRate;

		mSprite.spriteName = "dice" + string.Format("{0:D4}", index);

		index = (index + 1) % 24;
	}
}
