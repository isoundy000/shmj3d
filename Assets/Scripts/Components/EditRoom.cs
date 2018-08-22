
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using System.IO;

public class EditRoom : ListBase {
	List<UIToggle> uGameNum = new List<UIToggle>();
	List<UIToggle> uPlayerNum = new List<UIToggle>();
/*
	List<UIToggle> uLimits = new List<UIToggle>();

 	UISlider uHuaFen = null;
	UILabel uFlowers = null;
	int flowers = 1;
	UIToggle uMaima = null;
	UIToggle uAllPairs = null;
*/
	public UIToggle uJYW = null;
	public UIToggle uJ7W = null;
	public UIToggle uRYJ = null;

	UIToggle uIP = null;
	UIToggle uLocation = null;

	ClubRoomInfo mRoom = null;

	void Awake() {
		base.Awake();

		setBtnEvent (transform, "Bottom/BtnEdit", onBtnEdit);

		Transform grid = transform.Find ("items_gzmj/grid");
/*
		uHuaFen = grid.Find ("huafen/score").GetComponent<UISlider>();
		uHuaFen.onChange.Add(new EventDelegate(this, "onScoreChanged"));
		uFlowers = grid.Find ("huafen/value").GetComponent<UILabel>();
*/
		uGameNum.Add(grid.Find ("gamenum/gn4").GetComponent<UIToggle>());
		uGameNum.Add(grid.Find ("gamenum/gn8").GetComponent<UIToggle>());
		uGameNum.Add(grid.Find ("gamenum/gn16").GetComponent<UIToggle>());

		uPlayerNum.Add(grid.Find ("playernum/pn4").GetComponent<UIToggle>());
		uPlayerNum.Add(grid.Find ("playernum/pn3").GetComponent<UIToggle>());
		uPlayerNum.Add(grid.Find ("playernum/pn2").GetComponent<UIToggle>());
/*
		uLimits.Add(grid.Find("maxfan/limit2").GetComponent<UIToggle>());
		uLimits.Add(grid.Find("maxfan/limit3").GetComponent<UIToggle>());
		uLimits.Add(grid.Find("maxfan/limit4").GetComponent<UIToggle>());
		uLimits.Add(grid.Find("maxfan/limitno").GetComponent<UIToggle>());

		uMaima = grid.Find ("wanfa/horse").GetComponent<UIToggle> ();
		uAllPairs = grid.Find ("wanfa/allpairs").GetComponent<UIToggle> ();
*/
		uJYW = grid.Find ("wanfa/jyw").GetComponent<UIToggle> ();
		uJ7W = grid.Find ("wanfa/j7w").GetComponent<UIToggle> ();
		uRYJ = grid.Find ("wanfa/ryj").GetComponent<UIToggle> ();

		uIP = grid.Find ("limit/ip").GetComponent<UIToggle> ();
		uLocation = grid.Find ("limit/location").GetComponent<UIToggle> ();

		PUtils.setToggleEvent (grid, "playernum/pn4", null);
		PUtils.setToggleEvent (grid, "playernum/pn3", null);
		PUtils.setToggleEvent (grid, "playernum/pn2", null);
		PUtils.setToggleEvent (grid, "wanfa/jyw", null);
		PUtils.setToggleEvent (grid, "wanfa/j7w", null);
		PUtils.setToggleEvent (grid, "wanfa/ryj", null);
		PUtils.setToggleEvent (grid, "gamenum/gn4", null);
		PUtils.setToggleEvent (grid, "gamenum/gn8", null);
		PUtils.setToggleEvent (grid, "gamenum/gn16", null);
		PUtils.setToggleEvent (grid, "limit/ip", null);
		PUtils.setToggleEvent (grid, "limit/location", null);
	}
/*
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
*/
	void onBtnEdit() {
		int gamenum = 0;
		int[] gamenums = { 4, 8, 16 };

		for (int i = 0; i < uGameNum.Count; i++) {
			if (uGameNum [i].value) {
				gamenum = gamenums [i];
				break;
			}
		}

		int playernum = 4;
		int[] playernums = { 4, 3, 2 };

		for (int i = 0; i < uPlayerNum.Count; i++) {
			if (uPlayerNum [i].value) {
				playernum = playernums [i];
				break;
			}
		}
/*
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
*/
		bool jyw = uJYW.value;
		bool j7w = uJ7W.value;
		bool ryj = uRYJ.value;

		bool limit_ip = uIP.value;
		bool limit_gps = uLocation.value;

		JsonObject conf = new JsonObject ();
		conf.Add ("type", "gzmj");
		conf.Add ("gamenum", gamenum);
		conf.Add ("playernum", playernum);

/*		conf.Add ("maxfan", maxfan);
		conf.Add ("huafen", flowers);
		conf.Add ("maima", maima);
		conf.Add ("qidui", allpairs);
*/
		conf.Add("jyw", jyw);
		conf.Add("j7w", j7w);
		conf.Add("ryj", ryj);

		conf.Add ("limit_ip", limit_ip);
		conf.Add ("limit_gps", limit_gps);

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

			if (this != null)
				back();
		});
	}

	void refresh() {
		ClubRoomBaseInfo info = mRoom.base_info;

		int[] gamenums = { 4, 8, 16 };
		for (int i = 0; i < uGameNum.Count; i++)
			uGameNum [i].value = gamenums[i] == info.maxGames;

		int[] playernums = { 4, 3, 2 };
		for (int i = 0; i < uPlayerNum.Count; i++)
			uPlayerNum [i].value = playernums [i] == info.numOfSeats;
/*
		int[] maxfans = { 2, 3, 4, 100 };
		for (int i = 0; i < uLimits.Count; i++)
			uLimits [i].value = maxfans [i] == info.maxFan;

		uMaima.value = info.maima;
		uAllPairs.value = info.qidui;
*/
		uJYW.value = info.jyw;
		uJ7W.value = info.j7w;
		uRYJ.value = info.ryj;

		uIP.value = info.limit_ip;
		uLocation.value = info.limit_gps;

		//setScore (info.huafen);
	}

	public void enter(ClubRoomInfo room) {
		mRoom = room;
		refresh();
		show();

		Transform grid = transform.Find("items_gzmj/grid");

		grid.GetComponent<UIGrid> ().Reposition ();
		grid.GetComponentInParent<UIScrollView> ().ResetPosition ();
	}
}
