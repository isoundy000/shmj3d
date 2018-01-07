
using System;
using UnityEngine;
using System.Collections.Generic;

public class GameSeat : MonoBehaviour {

	GameObject mAction = null;
	GameObject mCard = null;
	int mEndTime = -1;
	int _lastSecs = -1;

	void Awake() {
		mAction = transform.FindChild ("action").gameObject;
		mCard = transform.FindChild ("action/mahjong2d").gameObject;
	}

	public void showAction(string act, int card = 0) {
		int id = -1;

		if (act == "chi")
			id = 0;
		else if (act == "peng")
			id = 1;
		else if (act == "gang")
			id = 2;
		else if (act == "ting")
			id = 3;
		else if (act == "add_flower")
			id = 4;

		mAction.SetActive(true);
		mAction.GetComponent<SpriteMgr>().setIndex(id);

		mCard.gameObject.SetActive (card > 0);
		if (card > 0)
			mCard.GetComponent<Mahjong2D> ().setID (card);

		int now = (int)((DateTime.Now.Ticks - DateTime.Parse("1970-01-01").Ticks) / 10000000);
		mEndTime = now + 2;
	}

	void Update () {
		if (mEndTime <= 0)
			return;

		int now = (int)((DateTime.Now.Ticks - DateTime.Parse("1970-01-01").Ticks) / 10000000);
		if (now == _lastSecs)
			return;

		_lastSecs = now;

		int last = mEndTime - now;
		if (last < 0) {
			mEndTime = -1;
			mAction.SetActive(false);
			return;
		}
	}
}
