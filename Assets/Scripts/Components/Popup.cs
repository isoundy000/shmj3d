using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour {

	public GameObject mAudioSet = null;
	public GameObject mMenu = null;
	public SpriteMgr mExit = null;

	void Start() {
		bool replay = ReplayMgr.GetInstance ().isReplay ();

		gameObject.SetActive(!replay);
	}

	public void onBtnMenu() {
		mMenu.SetActive(true);
	}

	void hideMenu() {
		mMenu.SetActive(false);
	}

	public void onBtnMenuMask() {
		hideMenu ();
	}

	public void onBtnAudioSet() {
		Debug.Log ("onBtnAudioSet");
		hideMenu ();
		mAudioSet.SetActive(true);
	}

	public void onBtnExit() {
		Debug.Log ("onBtnExit");

		RoomMgr rm = RoomMgr.GetInstance ();
		NetMgr nm = NetMgr.GetInstance ();
		bool isIdle = rm.isIdle ();
		bool isOwner = rm.isOwner ();

		if (isIdle) {
			if (isOwner) {
				/*
				cc.vv.alert.show('牌局还未开始，房主解散房间，房卡退还', function() {
					net.send("dispress");
				}, true);
*/
				nm.send ("dispress");
			} else {
				nm.send("exit");
			}
		} else {
			nm.send("dissolve_request");
		}

		hideMenu();
	}
}
