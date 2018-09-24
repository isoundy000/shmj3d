
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

			PUtils.setText (item, "roomid", "房间号:" + room.room_tag);

			string club = room.club_id == 10001 ? "" + room.club_name : "俱乐部:" + room.club_name;

			PUtils.setText (item, "club", PUtils.subString(club, 12));
			PUtils.setText (item, "desc", info.getDesc());
			PUtils.setText (item, "date", PUtils.formatTime(room.create_time, "MM-dd"));
			PUtils.setText (item, "time", PUtils.formatTime(room.create_time, "HH:mm"));

			var seats = item.Find ("seats");
			UITable table = seats.GetComponent<UITable>();
			int index = 0;
			for (int j = 0; j < seats.childCount && j < info.seats.Count; j++, index++) {
				var seat = seats.GetChild(j);

				seat.gameObject.SetActive(true);

				PUtils.setText(seat, "name", PUtils.subString(info.seats[j].name, 5));
				PUtils.setText(seat, "score", "" + info.seats [j].score);
				//PUtils.setIcon(seat, "bghead/icon", info.seats [j].uid);
			}

			for (int j = index; j < seats.childCount; j++) {
				var seat = seats.GetChild (j);
				seat.gameObject.SetActive (false);
			}

			if (table)
				table.Reposition();

			PUtils.onClick (item, () => {
				enterDetail(room);
			});
		}

		shrinkContent(rooms.Count);

		var grid = mGrid.GetComponent<UIGrid> ();
		if (grid)
			grid.Reposition();

		var scroll = mGrid.GetComponentInParent<UIScrollView> ();
		if (scroll)
			scroll.ResetPosition ();
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

		PUtils.setText (stats, "balance", "总盈亏: " + stat.balance);
		PUtils.setText (stats, "game", "" + stat.game_num);
		PUtils.setText (stats, "zimo", "" + stat.zimo);
		PUtils.setText (stats, "gk", "" + stat.gk);
		PUtils.setText (stats, "dp", "" + stat.dp);
	}
}
