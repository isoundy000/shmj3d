
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using System.IO;

public class EditRoom : ListBase {
	UISlider uHuaFen = null;
	UILabel uFlowers = null;
	int flowers = 1;
	List<UIToggle> uGameNum = new List<UIToggle>();
	List<UIToggle> uLimits = new List<UIToggle>();
	UIToggle uMaima = null;
	UIToggle uAllPairs = null;
	UIToggle uIP = null;
	UIToggle uLocation = null;

	ClubRoomInfo mRoom = null;

	void Awake() {
		base.Awake();

		Transform btnEdit = transform.FindChild ("Bottom/BtnEdit");
		btnEdit.GetComponent<UIButton> ().onClick.Add (new EventDelegate(this, "onBtnEdit"));

		uHuaFen = transform.FindChild ("Body/huafen/score").GetComponent<UISlider>();
		uHuaFen.onChange.Add(new EventDelegate(this, "onScoreChanged"));
		uFlowers = transform.FindChild ("Body/huafen/value").GetComponent<UILabel>();

		uGameNum.Add(transform.FindChild ("Body/gamenum/gn4").GetComponent<UIToggle>());
		uGameNum.Add(transform.FindChild ("Body/gamenum/gn8").GetComponent<UIToggle>());
		uGameNum.Add(transform.FindChild ("Body/gamenum/gn16").GetComponent<UIToggle>());
		uLimits.Add(transform.FindChild("Body/maxfan/limit2").GetComponent<UIToggle>());
		uLimits.Add(transform.FindChild("Body/maxfan/limit3").GetComponent<UIToggle>());
		uLimits.Add(transform.FindChild("Body/maxfan/limit4").GetComponent<UIToggle>());
		uLimits.Add(transform.FindChild("Body/maxfan/limitno").GetComponent<UIToggle>());
		uMaima = transform.FindChild ("Body/wanfa/horse").GetComponent<UIToggle> ();
		uAllPairs = transform.FindChild ("Body/wanfa/allpairs").GetComponent<UIToggle> ();
		uIP = transform.FindChild ("Body/limit/ip").GetComponent<UIToggle> ();
		uLocation = transform.FindChild ("Body/limit/location").GetComponent<UIToggle> ();
	}

	void onScoreChanged() {
		int[] range = { 1, 5, 10, 20, 30, 50, 100, 200, 300 };
		int id = (int)(uHuaFen.value * (range.Length - 1) + 0.5);

		uFlowers.text = range[id].ToString();
		flowers = range[id];
	}

	void setScore(int score) {
		int[] range = { 1, 5, 10, 20, 30, 50, 100, 200, 300 };

		for (int i = 0; i < range.Length; i++) {
			if (range [i] == score) {
				uHuaFen.value = (float)i / (range.Length - 1);
				break;
			}
		}
	}

	void onBtnEdit() {
		int gamenum = 0;
		int[] gamenums = { 4, 8, 16 };

		for (int i = 0; i < uGameNum.Count; i++) {
			if (uGameNum [i].value) {
				gamenum = gamenums [i];
				break;
			}
		}

		int maxfan = 0;
		int[] maxfans = { 2, 3, 4, 100 };
		for (int i = 0; i < uLimits.Count; i++) {
			if (uLimits [i].value) {
				maxfan = maxfans[i];
				break;
			}
		}

		bool maima = uMaima.value;
		bool allpairs = uAllPairs.value;

		JsonObject conf = new JsonObject ();
		conf.Add ("type", "shmj");
		conf.Add ("gamenum", gamenum);
		conf.Add ("maxfan", maxfan);
		conf.Add ("huafen", flowers);
		conf.Add ("playernum", 4);
		conf.Add ("maima", maima);
		conf.Add ("qidui", allpairs);

		JsonObject ob = new JsonObject ();
		ob["roomid"] = mRoom.room_tag;
		ob["club_id"] = mRoom.club_id;
		ob["conf"] = conf;

		NetMgr nm = NetMgr.GetInstance();

		nm.request_connector ("edit_room", ob, (data) => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
			if (ret.errcode != 0) {
				Debug.Log("edit room fail");
				return;
			}

			back();
		});
	}

	void refresh() {
		ClubRoomBaseInfo info = mRoom.base_info;

		int[] gamenums = { 4, 8, 16 };
		for (int i = 0; i < uGameNum.Count; i++)
			uGameNum [i].value = gamenums[i] == info.maxGames;

		int[] maxfans = { 2, 3, 4, 100 };
		for (int i = 0; i < uLimits.Count; i++)
			uLimits [i].value = maxfans [i] == info.maxFan;

		uMaima.value = info.maima;
		uAllPairs.value = info.qidui;

		setScore (info.huafen);
	}

	public void enter(ClubRoomInfo room) {
		mRoom = room;
		refresh();
		show();
	}
}
