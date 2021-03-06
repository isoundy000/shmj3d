﻿
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;

public class Voice : MonoBehaviour {

	public GameObject voice = null;
	public UISprite bar = null;
	public UILabel notice = null;

	long lastTouchTime = 0;
	int oldWidth = 0;
	long MAXTIME = 15000;

	void Awake() {
		oldWidth = bar.width;
	}

	void Update() {
		if (lastTouchTime > 0) {
			long time = PUtils.getMilliSeconds () - lastTouchTime;
			if (time >= MAXTIME) {
				onVoiceOK ();
			} else {
				bar.width = (int)(((float)time / MAXTIME) * oldWidth);
			}
		}
	}

	void onVoiceOK() {
		VoiceMgr vm = VoiceMgr.GetInstance();

		if (lastTouchTime > 0) {
			vm.release();

			long time = PUtils.getMilliSeconds () - lastTouchTime;
			string msg = vm.getVoiceData("record.amr");

			if (msg != null && msg.Length > 0) {
				JsonObject ob = new JsonObject ();
				ob ["msg"] = msg;
				ob ["time"] = time;

				NetMgr.GetInstance ().send ("voice_msg", ob);
			}
		}

		voice.SetActive (false);
	}

	public void onPress() {
		bar.width = 0;
		notice.text = "请按住说话";
		voice.SetActive (true);
		lastTouchTime = PUtils.getMilliSeconds();

		Debug.Log ("onPress");

		VoiceMgr.GetInstance().prepare("record.amr");
	}

	public void onRelease() {
		if (lastTouchTime == 0)
			return;

		if (PUtils.getMilliSeconds () - lastTouchTime < 1000) {
			VoiceMgr.GetInstance().cancel();
			voice.SetActive(true);
			bar.width = 0;
			notice.text = "录制时间太短";
			voice.SetActive(false);
		} else {
			onVoiceOK ();
		}

		lastTouchTime = 0;
	}
}
