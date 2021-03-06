﻿
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class RecommendRoom {
	public int id;
	public string room_tag;
	public ClubRoomBaseInfo base_info;
	public int club_id;
	public string club_name;
	public string club_logo;
	public int cnt;
	public List<ClubRoomPlayer> players;
}

[Serializable]
public class ListRecommendRooms {
	public int errcode;
	public string errmsg;
	public List<RecommendRoom> data;
}

public class Discover : ListBase {

	public GameObject msg_num = null;

	bool shown = false;
	int mRoomID = 0;

	void Awake() {
		listPath = "Recommend/items/grid";
		base.Awake();

		InitEventHandler();
	}

	void InitEventHandler() {
		GameMgr gm = GameMgr.GetInstance();

		gm.AddHandler("recommend_room_updated", data=>{
			if (!shown) return;

			refresh();
		});

		gm.AddHandler("sys_message_updated", data => {
			updateMessageCnt();
		});
	}

	public void onBtnCreate() {
		CreateRoom jr = getPage<CreateRoom>("PCreateRoom");
		jr.enter();
	}

	public void onBtnJoin() {
		JoinRoom jr = getPage<JoinRoom>("PJoinRoom");
		jr.enter();
	}

	void OnEnable() {
		refresh();
		updateMessageCnt();

		shown = true;
	}

	void OnDisable() {
		shown = false;
	}

	void refresh() {
		NetMgr.GetInstance ().request_apis ("list_recommend_rooms", null, data => {
			ListRecommendRooms ret = JsonUtility.FromJson<ListRecommendRooms> (data.ToString());
			if (ret.errcode != 0) {
				Debug.Log("list_recommend_rooms fail");
				return;
			}

			showItems(ret.data);
		});
	}

	void showItems(List<RecommendRoom> rooms) {
		bool show = false;

		for (int i = 0; i < rooms.Count; i++) {
			Transform item = getItem(i);
			RecommendRoom room = rooms[i];
			ClubRoomBaseInfo info = room.base_info;

			setText(item, "club", room.club_name + "俱乐部");
			setText(item, "desc", info.huafen + "/" + info.huafen + (info.maima ? "带苍蝇" : "不带苍蝇") + info.maxGames + "局");
			setText(item, "room", "房间号" + room.room_tag);
			setText(item, "hc", room.cnt + " / " + info.numOfSeats);
			setIcon(item, "bghead/icon", room.club_logo);

			setBtnEvent (item, null, () => {
				showDetail(room);
			});

			if (mRoomID == room.id) {
				show = true;
				showDetail (room);
			}
		}

		if (!show) {
			mRoomID = 0;
			setActive (transform, "detail", false);
		}

		updateItems(rooms.Count);
	}

	void setCount(int cnt) {
		msg_num.SetActive(cnt > 0);
		msg_num.transform.Find("tile").GetComponent<UILabel>().text = "" + cnt;
	}

	void updateMessageCnt() {
		NetMgr.GetInstance ().request_apis ("get_my_message_cnt", null, data => {
			GetClubMessageCnt ret = JsonUtility.FromJson<GetClubMessageCnt> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_my_message_cnt fail");
				return;
			}

			if (ret.data != null)
				setCount(ret.data.cnt);
			else
				Debug.LogError("get_my_message_cnt failed");
		});
	}

	public void onBtnMessage() {
		Message msg = getPage<Message>("PMessage");
		msg.UpdateEvents += refresh;
		msg.enter();
	}

	void showDetail(RecommendRoom room) {
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
}



