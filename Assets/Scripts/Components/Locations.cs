
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locations : MonoBehaviour {

	void Awake() {

		var gm = GameMgr.GetInstance ();

		gm.AddHandler ("location_warning", data => {
			if (this != null && gameObject.activeInHierarchy)
				refresh();
		});

		gm.AddHandler ("user_state_changed", data => {
			if (this != null && gameObject.activeInHierarchy)
				refresh();
		});

		PUtils.setBtnEvent (transform, "btnClose", onBtnClose);
	}

	public void onBtnClose() {
		gameObject.SetActive(false);
	}

	void OnEnable() {
		refresh();
	}

	void refresh() {
		var tm = transform;
		var seats = tm.Find ("seats");
		var info = tm.Find ("info");
		var rm = RoomMgr.GetInstance ();
		var players = rm.players;

		for (int i = 0; i < seats.childCount; i++)
			PUtils.setActive (seats.GetChild (i), null, false);

		for (int i = 0; i < info.childCount; i++)
			PUtils.setActive (info.GetChild (i), null, false);

		for (int i = 0; i < players.Count; i++) {
			var p = players [i];
			var local = rm.getLocalIndex (i);
			var seat = seats.GetChild (local);
			var valid = p.gps != null && p.gps.valid;
			var onSeat = p.userid > 0;

			PUtils.setActive (seat, null, onSeat);
			PUtils.setIcon (seat, "bghead/icon", p.userid);
			PUtils.setActive(seat, "status", onSeat && !valid);

			if (!onSeat || !valid)
				continue;

			for (int j = i + 1; j < players.Count; j++) {
				var p2 = players [j];
				var local2 = rm.getLocalIndex (j);
				var seat2 = seats.GetChild (local2);
				var valid2 = p2.gps != null && p2.gps.valid;
				var name = local < local2 ? local + "-" + local2 : local2 + "-" + local;
				var line = info.Find (name);
				var onSeat2 = p2.userid > 0;

				if (!onSeat2)
					continue;

				bool show = valid && valid2;
				PUtils.setActive (line, null, show);

				if (show) {
					var dis = getDistance (p.gps.lat, p.gps.lon, p2.gps.lat, p2.gps.lon);
					var text = dis > 1000 ? Math.Round(dis / 1000, 1) + "千米" : Math.Floor (dis) + "米";

					PUtils.setText (line, "text", text);
				}
			}
		}
	}

	float rad(float d) {
		return d * Mathf.PI / 180.0f;
	}

	float getDistance(float lat1, float lng1, float lat2, float lng2) {
		float a, b, R;

		R = 6378137;

		lat1 = lat1 * Mathf.PI / 180.0f;
		lat2 = lat2 * Mathf.PI / 180.0f;


		a = (lat1 - lat2);
		b = (lng1 - lng2) * Mathf.PI / 180.0f;

		float d;
		float sa2, sb2;

		sa2 = Mathf.Sin(a / 2.0f);
		sb2 = Mathf.Sin(b / 2.0f);

		d = 2 * R * Mathf.Asin(Mathf.Sqrt(sa2 * sa2 + Mathf.Cos(lat1) * Mathf.Cos(lat2) * sb2 * sb2));

		return d;
	}
}

