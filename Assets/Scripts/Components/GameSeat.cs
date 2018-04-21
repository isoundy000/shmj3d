
using System;
using UnityEngine;
using System.Collections.Generic;

public class GameSeat : MonoBehaviour {

	GameObject mAction = null;
	GameObject mCard = null;
	GameObject mChupai = null;
	int mEndTime = -1;
	int _lastSecs = -1;

	float mActionEnd = 0;
	float mChupaiEnd = 0;

	Transform mEffect = null;

	void Awake() {
		mAction = transform.Find ("action").gameObject;
		mCard = transform.Find ("action/mahjong2d").gameObject;
		mChupai = transform.Find ("chupai").gameObject;

		if (mAction == null)
			Debug.LogError ("action null");

		if (mCard == null)
			Debug.LogError ("card null");

		mEffect = transform.Find ("effect");
	}

	void Start() {
		mAction.SetActive(false);
		mCard.SetActive(false);
	}

	public void showAction(string act, int card = 0) {
		int id = -1;

		if (act == "chi" || act == "peng" || act == "gang" || act == "ting" || act == "hu" || act == "zimo" || act == "dianpao") {
			string path = "Prefab/anim/" + act;
			GameObject obj = Instantiate(Resources.Load(path), mEffect) as GameObject;

			return;
		}
			
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

		if (mAction == null || mCard == null)
			Debug.LogError ("action or card null");

		mAction.SetActive(true);	
		mAction.GetComponent<SpriteMgr>().setIndex(id);

		mCard.SetActive (card > 0);
		if (card > 0)
			mCard.GetComponent<Mahjong2D> ().setID (card);

		mActionEnd = Time.time + 2.0f;

		hideChupai ();
	}

	void hideAction() {
		mAction.SetActive (false);
		mActionEnd = 0;
	}

	public void showChupai(int card) {
		mChupai.SetActive (true);
		mChupai.GetComponentInChildren<Mahjong2D>().setID(card % 100);

		//mChupaiEnd = Time.time + 2.0f;

		hideAction ();
	}

	public void hideChupai() {
		mChupai.SetActive (false);
		mChupaiEnd = 0;
	}

	void Update () {
		float now = Time.time;

		if (mActionEnd > 0 && mActionEnd < now)
			hideAction ();

		if (mChupaiEnd > 0 && mChupaiEnd < now)
			hideChupai ();
	}
}
