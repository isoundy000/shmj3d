﻿
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class HistoryStat {
	public int balance;
	public int zimo;
	public int dp;
	public int gk;
	public int game_num;
}

[Serializable]
public class UserHistory {
	public HistoryStat statw;
	public HistoryStat statd;
	public HistoryStat statm;
	public List<RoomHistory> rooms;
}

[Serializable]
public class HistorySeats {
	public int uid;
	public string name;
	public int score;
}

[Serializable]
public class RoomHistoryInfo {
	public string type;
	public int maxGames;

	public int game_num;
	public int huafen;
	public bool maima;

	public bool jyw;
	public bool j7w;
	public bool ryj;

	public List<HistorySeats> seats;

	public string getDesc() {
		List<string> tips = new List<string> ();
		if (type == "shmj") {
			tips.Add ("上海敲麻");
			tips.Add (huafen + "/" + huafen);
			tips.Add (maima ? "带苍蝇" : "不带苍蝇");
		} else if (type == "gzmj") {
			tips.Add ("酒都麻将");
			if (jyw) tips.Add ("金银乌");
			if (j7w) tips.Add ("见7挖");
			if (ryj) tips.Add ("软硬鸡");
		}

		tips.Add (maxGames + "局");

		return string.Join (" ", tips.ToArray ());
	}
}

[Serializable]
public class RoomHistory {
	public int id;
	public int room_id;
	public int club_id;
	public string room_tag;
	public int create_time;
	public int score;
	public string club_name;
	public string club_logo;
	public RoomHistoryInfo info;
}

[Serializable]
public class ListUserHistory {
	public int errcode;
	public string errmsg;
	public UserHistory data;
}

public class History : MonoBehaviour {

	Transform mGrid = null;
	Transform mTemp = null;

	UserHistory mHistory = null;

	void Awake() {
		mGrid = transform.Find ("items/grid");

		mTemp = mGrid.GetChild(0);
		mTemp.parent = null;
	}

	void Start() {
		refresh ();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("list_user_history", null, data => {
			ListUserHistory ret = JsonUtility.FromJson<ListUserHistory> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("list_user_history msg=" + ret.errmsg);
				return;
			}

			if (this != null) {
				mHistory = ret.data;
				showHistories();
			}
		});
	}

	void showHistories() {
		List<RoomHistory> rooms = mHistory.rooms;

		onButtonSel(mHistory.statd);

		for (int i = 0; i < rooms.Count; i++) {
			Transform item = getItem(i);
			RoomHistory room = rooms[i];
			RoomHistoryInfo info = room.info;

			item.Find("roomid").GetComponent<UILabel>().text = "房间号:" + room.room_tag;
			item.Find("club").GetComponent<UILabel>().text = room.club_id == 1 ? room.club_name : "俱乐部:" + room.club_name;
			item.Find("desc").GetComponent<UILabel> ().text = info.getDesc();
			item.Find("btn/score").GetComponent<UILabel>().text = "" + room.score;
			item.Find("date").GetComponent<UILabel>().text = PUtils.formatTime(room.create_time, "MM-dd");
			item.Find("time").GetComponent<UILabel>().text = PUtils.formatTime(room.create_time, "HH:mm");

			PUtils.onClick (item.Find ("btn"), () => {
				enterDetail(room);
			});

			PUtils.onClick (item, () => {
				enterDetail(room);
			});
		}

		shrinkContent(rooms.Count);
		mGrid.GetComponent<UIGrid> ().Reposition ();
		mGrid.GetComponentInParent<UIScrollView> ().ResetPosition ();
	}

	void enterDetail(RoomHistory room) {
		var ob = ListBase.getPage<DetailHistory>("PDetailHistory");
		if (ob != null)
			ob.enter(room);
	}

	Transform getItem(int id) {
		if (mGrid.childCount > id)
			return mGrid.GetChild(id);

		GameObject ob = Instantiate(mTemp.gameObject, mGrid) as GameObject;
		return ob.transform;
	}

	void shrinkContent(int num) {
		while (mGrid.childCount > num)
			DestroyImmediate(mGrid.GetChild(mGrid.childCount - 1).gameObject);
	}

	public void onDaySel() {
		if (mHistory == null)
			return;

		if (UIToggle.current.value)
			onButtonSel (mHistory.statd);
	}

	public void onWeekSel() {
		if (mHistory == null)
			return;

		if (UIToggle.current.value)
			onButtonSel(mHistory.statw);
	}

	public void onMonthSel() {
		if (mHistory == null)
			return;

		if (UIToggle.current.value)
			onButtonSel(mHistory.statm);
	}

	void onButtonSel(HistoryStat stat) {
		Transform stats = transform.Find("stats");

		stats.Find("balance").GetComponent<UILabel>().text = "总盈亏: " + stat.balance;
		stats.Find("game").GetComponent<UILabel>().text = "" + stat.game_num;
		stats.Find("zimo").GetComponent<UILabel>().text = "" + stat.zimo;
		stats.Find("gk").GetComponent<UILabel>().text = "" + stat.gk;
		stats.Find("dp").GetComponent<UILabel>().text = "" + stat.dp;
	}
}
