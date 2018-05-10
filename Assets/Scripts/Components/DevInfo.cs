using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevInfo : MonoBehaviour {

	Transform network;
	Transform power;

	float lastcheck = 0;

	void Awake () {
		network = transform.Find("network");
		power = transform.Find("power");
	}

	void Start() {
		lastcheck = Time.time;
	
		updateBattery();
		updateNetwork();
	}

	void updateBattery() {
		Transform progress = power.Find("progress");
		SpriteMgr sm = progress.GetComponent<SpriteMgr>();
		UISprite sp = progress.GetComponent<UISprite>();
		BatteryInfo info = AnysdkMgr.GetBatteryInfo();

		sm.setIndex(info.state == "charging" ? 1 : 0);
		sp.fillAmount = (float)info.power / 100;
	}

	void updateNetwork() {
		Transform state = network.Find("state");
		Transform wifi = network.Find("wifi");
		SpriteMgr sm = wifi.GetComponent<SpriteMgr>();
		UILabel desc = state.GetComponent<UILabel>();
		NetworkInfo info = AnysdkMgr.GetNetworkInfo();

		bool isWifi = info.type == "wifi";

		wifi.gameObject.SetActive(isWifi);
		state.gameObject.SetActive(!isWifi);

		if (isWifi)
			sm.setIndex(info.strength - 1);
		else
			desc.text = info.type;
	}

	void Update () {
		if (Time.time - lastcheck >= 2.0f) {
			lastcheck = Time.time;

			updateBattery();
			updateNetwork();
		}
	}
}
