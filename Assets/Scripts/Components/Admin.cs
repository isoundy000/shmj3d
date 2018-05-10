
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

public class Admin : ListBase {
	public int mClubID = 0;
	int mRoomID = 0;
	List<ClubRoomInfo> mRooms = null;

	void Awake() {
		base.Awake();

		setBtnEvent(transform, "entries/btn_member", onBtnMember);
		setBtnEvent(transform, "entries/btn_history", onBtnHistory);
		setBtnEvent(transform, "entries/btn_message", onBtnMessage);
		setBtnEvent(transform, "top/btn_detail", onBtnDetail);
		setBtnEvent(transform, "bottom/btn_create", onBtnCreate);

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

		gm.AddHandler ("club_message_notify", data => {
			if (!mShow) return;

			ClubMessageNotify notify = (ClubMessageNotify)data;

			Debug.Log("club message: " + notify.club_id);

			if (notify.club_id == mClubID)
				setCount(notify.cnt);
		});
	}

	void setCount(int cnt) {
		setActive(transform, "entries/btn_message/msg_num", cnt > 0);
		if (cnt > 0)
			setText(transform, "entries/btn_message/msg_num/tile", "" + cnt);
	}

	void updateMessageCnt() {
		GameMgr.get_club_message_cnt (mClubID, cnt => {
			setCount(cnt);
		});
	}

	void onBtnSetClub() {
		GameObject ob = GameObject.Find ("PSetClub");
		ob.GetComponent<SetClub>().enter(mClubID);
	}

	void onBtnDetail() {
		GameObject ob = GameObject.Find ("PClubDetail");
		ob.GetComponent<ClubDetail>().enter(mClubID, true);
	}

	void onBtnMember() {
		GameObject ob = GameObject.Find ("PSetMember");
		ob.GetComponent<SetMember>().enter(mClubID);
	}

	void onBtnMessage() {
		GameObject ob = GameObject.Find ("PClubMessage");
		ob.GetComponent<ClubMessage>().enter(mClubID);
	}

	void onBtnHistory() {
		GameObject ob = GameObject.Find ("PClubHistory");
		Debug.Log ("onBtnHistory");
		ob.GetComponent<ClubHistory>().enter(mClubID);
	}

	void onBtnCreate() {
		GameObject ob = GameObject.Find ("PCreateRoom");
		Debug.Log ("onBtnCreate");
		ob.GetComponent<CreateRoom>().enter(mClubID);
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
		updateMessageCnt();
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		if (mClubID == 0) {
			updateItems(0);
			return;
		}

		nm.request_apis ("list_club_rooms", "club_id", mClubID, data => {
			ListClubRoom ret = JsonUtility.FromJson<ListClubRoom> (data.ToString ());
			if (ret.errcode != 0)
				return;

			mRooms = ret.data;
			showRooms();
		});
	}

	void showRooms() {
		int cnt = mRooms.Count;
		NetMgr nm = NetMgr.GetInstance ();
		int uid = GameMgr.getUserMgr ().userid;

		for (int i = 0; i < mRooms.Count; i++) {
			ClubRoomInfo room = mRooms [i];
			Transform item = getItem(i);
			Transform seats = item.Find ("seats");
			bool found = false;

			int readys = 0;
			int nplayers = 0;

			bool idle = room.status == "idle";

			int j = 0;

			for (; j < room.players.Count && j < seats.childCount; j++) {
				ClubRoomPlayer p = room.players [j];
				Transform s = seats.GetChild(j);
				GameObject name = s.Find ("name").gameObject;
				GameObject ready = s.Find ("ready").gameObject;
				GameObject id = s.Find ("id").gameObject;
				GameObject icon = s.Find ("icon").gameObject;
				GameObject btn_kick = s.Find("btn_kick").gameObject;

				bool empty = p.id == 0;

				s.gameObject.SetActive(true);

				setActive(s, "icon", !empty);
				setActive(s, "name", !empty);
				setActive(s, "id", !empty);
				setActive(s, "ready", !empty && p.ready);
				setActive(s, "btn_kick", !empty && idle);

				if (!empty && idle) {
					setBtnEvent(s, "btn_kick", () => {
						onBtnKick (p.id, room.id, room.room_tag);
					});
				}

				if (empty)
					continue;

				nplayers += 1;
				if (p.ready)
					readys += 1;

				setText(s, "name", p.name);
				setText(s, "id", "" + p.id);
				setIcon(s, "icon", p.id);

				if (p.id == uid)
					mRoomID = room.id;
			}

			for (int k = j; k < seats.childCount; k++) {
				Transform s = seats.GetChild(k);
				s.gameObject.SetActive(false);
			}

			ClubRoomBaseInfo info = room.base_info;
			setText(item, "desc", info.huafen + "/" + info.huafen + (info.maima ? "带苍蝇" : "不带苍蝇") + info.maxGames + "局");
			setText(item, "progress", room.num_of_turns + " / " + info.maxGames);
			setText(item, "roomid", "ID:" + room.id);
			setText(item, "status", idle ? "开始" : "游戏中");

			Transform btn_play = item.Find ("btn_play");
			btn_play.GetComponent<SpriteMgr> ().setIndex (idle ? 0 : 1);
			PUtils.onClick (btn_play, () => {
				if (room.status == "idle") {
					if (readys != info.numOfSeats) {
						GameAlert.Show("玩家没有全部准备");
						return;
					}

					nm.request_connector ("start_room", "room_tag", room.room_tag, data => {
						NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
						if (ret.errcode != 0) {
							Debug.Log("start room fail");
							return;
						}

						refresh();
					});
				} else {
					nm.request_connector ("stop_room", "room_tag", room.room_tag, data => {
						NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
						if (ret.errcode != 0) {
							Debug.Log("stop room fail");
							return;
						}

						refresh();
					});
				}
			});

			setActive(item, "btn_edit", idle && nplayers == 0);
			setBtnEvent(item, "btn_edit", () => {
				EditRoom er = getPage<EditRoom>("PEditRoom");
				
				er.UpdateEvents += refresh;
				er.enter(room);
			});

			setActive(item, "btn_destroy", idle && nplayers == 0);
			setBtnEvent(item, "btn_destroy", () => {
				GameAlert.Show("确定解散房间吗?", () => {
					JsonObject ob = new JsonObject();
					ob["roomid"] = room.id;
					ob["room_tag"] = room.room_tag;
					ob["club_id"] = room.club_id;

					nm.request_apis("destroy_club_room", ob, data=>{
						NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
						if (ret.errcode != 0) {
							Debug.Log("destroy club room fail");
							return;
						}

						refresh();
					});
				}, true);
			});
		}

		updateItems(mRooms.Count);
	}

	void onBtnKick(int uid, int roomid, string room_tag) {
		NetMgr nm = NetMgr.GetInstance ();

		JsonObject ob = new JsonObject(); 
		ob["uid"] = uid;
		ob["roomid"] = roomid;
		ob["room_tag"] = room_tag;

		nm.request_connector ("kick", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("kick fail");
				return;
			}

			refresh();
		});
	}
}

