﻿
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ClubRoomPlayer {
	public int id;
	public string name;
	public string icon;
	public int seatindex;
	public bool ready;
}

[Serializable]
public class ClubRoomBaseInfo {
	public int huafen;
	public bool maima;
	public int maxGames;
	public int maxFan;
	public bool qidui;
	public int numOfSeats;
	public bool limit_ip;
	public bool limit_gps;
}

[Serializable]
public class ClubRoomInfo {
	public int id;
	public string room_tag;
	public string status;
	public ClubRoomBaseInfo base_info;
	public int num_of_turns;
	public int club_id;
	public List<ClubRoomPlayer> players;
}

[Serializable]
public class ListClubRoom {
	public int errcode;
	public string errmsg;
	public List<ClubRoomInfo> data;
}

public class Hall : ListBase {
	public int mClubID = 0;
	int mRoomID = 0;
	List<ClubRoomInfo> mRooms = null;
	public GameObject mShare = null;

	void Awake() {
		base.Awake();

		InitEventHandler ();
	}

	void InitEventHandler() {
		GameMgr gm = GameMgr.GetInstance ();

		gm.AddHandler ("club_room_updated", data => {
			if (!mShow) return;
			refresh ();
		});

		gm.AddHandler ("club_room_removed", data => {
			if (!mShow) return;
			refresh ();
		});
	}

	public void onBtnCard() {
		if (mShare == null)
			return;

		mShare.SetActive(true);
		mShare.GetComponent<Share>().club_id = mClubID;
	}

	public void onBtnHistory() {
		GameObject ob = GameObject.Find ("PClubHistory");
		ob.GetComponent<ClubHistory>().enter(mClubID);
	}

	void onBack() {
		leaveClubChannel (mClubID);
		mClubID = 0;
	}

	void joinClubChannel(int club_id) {
		NetMgr nm = NetMgr.GetInstance();
		nm.request_apis ("join_club_channel", "club_id", club_id, data => {
			GameMgr.GetInstance().club_channel = club_id;
		});
	}

	void leaveClubChannel(int club_id) {
		NetMgr nm = NetMgr.GetInstance();
		nm.request_apis ("leave_club_channel", "club_id", club_id, data => {
			GameMgr.GetInstance().club_channel = 0;
		});
	}

	public void enter(int clubid) {
		mClubID = clubid;
		refresh ();
		joinClubChannel (clubid);
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("list_club_rooms", "club_id", mClubID, data => {
			ListClubRoom ret = JsonUtility.FromJson<ListClubRoom> (data.ToString ());
			if (ret.errcode != 0)
				return;

			mRooms = ret.data;
			showRooms();
		});
	}

	public void onBtnDetail() {
		GameObject ob = GameObject.Find ("PClubDetail");
		ob.GetComponent<ClubDetail>().enter(mClubID, false);
	}

	void showRooms() {
		int cnt = mRooms.Count;
		GameMgr gm = GameMgr.GetInstance();
		int uid = GameMgr.getUserMgr().userid;

		bool show = false;

		for (int i = 0; i < mRooms.Count; i++) {
			ClubRoomInfo room = mRooms [i];
			Transform item = getItem(i);
			Transform table = item.Find ("table");
			bool found = false;

			int j = 0;

			for (; j < room.players.Count && j < table.childCount; j++) {
				ClubRoomPlayer p = room.players [j];
				Transform s = table.GetChild(j);
				bool empty = p.id == 0;

				s.gameObject.SetActive(true);

				setActive(s, "name", !empty);
				setActive(s, "ready", !empty && p.ready);
				setActive(s, "icon", !empty);

				UIButton btn = s.GetComponent<UIButton>();
				btn.enabled = empty;
				if (empty) {
					setBtnEvent(s, null, () => {
						showDetail(room);
					});

					continue;
				}
			
				setText(s, "name", p.name);
				setIcon(s, "icon", p.id);

				if (p.id == uid)
					found = true;
			}

			for (int k = j; k < table.childCount; k++) {
				Transform s = table.GetChild(k);
				s.gameObject.SetActive(false);
			}

			ClubRoomBaseInfo info = room.base_info;
			setText(item, "desc", info.huafen + "/" + info.huafen + (info.maima ? "带苍蝇" : "不带苍蝇") + info.maxGames + "局");
			setText(item, "progress", room.num_of_turns + " / " + info.maxGames);
			setActive(item, "btn_leave", found);

			if (found) {
				setBtnEvent(item, "btn_leave", () => {
					leaveRoom(room.id, room.room_tag);
				});
			}

			if (mRoomID == room.id) {
				show = true;
				showDetail (room);
			}
		}

		updateItems(mRooms.Count);

		if (!show) {
			mRoomID = 0;
			setActive (transform, "detail", false);
		}
	}

	void showDetail(ClubRoomInfo room) {
		mRoomID = room.id;

		Transform detail = transform.Find("detail");
		setActive(detail, null, true);

		Transform seats = detail.Find("seats");
		UIGrid grid = seats.GetComponent<UIGrid>();

		int nseats = room.players.Count;
		int empties = 0;

		for (int i = 0; i < seats.childCount && i < nseats; i++) {
			ClubRoomPlayer p = room.players [i];
			Transform s = seats.GetChild(i);
			bool empty = p.id == 0;

			setActive(s, null, true);
			setActive(s, "name", !empty);
			setActive(s, "bghead/icon", !empty);

			setIcon(s, "bghead/icon", p.id);
			setText(s, "name", p.name);
			if (empty)
				empties++;
		}

		for (int i = nseats; i < seats.childCount; i++) {
			Transform s = seats.GetChild (i);
			setActive(s, null, false);
		}

		grid.Reposition();

		ClubRoomBaseInfo info = room.base_info;
		Transform rules = detail.Find("rules");

		setText(rules, "rule", "上海敲麻");
		setText(rules, "huafen", "" + info.huafen);
		setText (rules, "playernum", "" + info.numOfSeats);
		setText (rules, "gamenum", "" + info.maxGames);
		setText (rules, "maxfan", "" + info.maxFan);
		setText (rules, "maima", info.maima ? "是" : "否");
		setText (rules, "qidui", info.qidui ? "是" : "否");
		setText (rules, "limit_ip", "否");
		setText (rules, "limit_gps", "否");

		setBtnEvent(detail, "btn_join", () => {
			if (empties == 0) {
				GameAlert.Show("房间已满，请重新选择");
				return;
			}

			mShow = false;
			GameMgr.GetInstance().enterRoom(room.room_tag, code=>{
				Debug.Log("club enterRoom code=" + code);
				if (0 != code) {
					mShow = true;

					string content = "房间不存在";

					if (code == 2224)
						content = "房间已满！";
					else if (code == 2222)
						content = "钻石不足";
					else if (code == 2231)
						content = "您的IP和其他玩家相同";
					else if (code == 2232)
						content = "您的位置和其他玩家太近";
					else if (code == 2233)
						content = "您的定位信息无效，请检查是否开启定位";
					else if (code == 2251)
						content = "您不是俱乐部普通成员，无法加入俱乐部房间";

					GameAlert.Show(content);
				}
			});
		});

		setBtnEvent(detail, "btn_back", () => {
			mRoomID = 0;
			setActive(detail, null, false);
		});
	}

	void leaveRoom(int roomid, string room_tag) {
		JsonObject ob = new JsonObject ();
		ob ["roomid"] = roomid;
		ob ["room_tag"] = room_tag;

		NetMgr.GetInstance ().request_apis ("leave_club_room", ob, data => {
			refresh();
		});
	}
}
