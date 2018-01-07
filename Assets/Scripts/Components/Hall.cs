
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
	public ClubInfo mClub = null;
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
			refresh ();
		});

		gm.AddHandler ("club_room_removed", data => {
			refresh ();
		});
	}

	public void onBtnCard() {
		if (mShare == null)
			return;

		mShare.SetActive(true);
		mShare.GetComponent<Share>().club_id = mClub.id;
	}

	public void onBtnHistory() {
		GameObject ob = GameObject.Find ("PClubHistory");
		ob.GetComponent<ClubHistory>().enter(mClub.id);
	}

	void onBack() {
		leaveClubChannel (mClub.id);
		mClub = null;
	}

	void joinClubChannel(int club_id) {
		NetMgr nm = NetMgr.GetInstance();
		nm.request_apis ("join_club_channel", "club_id", club_id, data => {});
	}

	void leaveClubChannel(int club_id) {
		NetMgr nm = NetMgr.GetInstance();
		nm.request_apis ("leave_club_channel", "club_id", club_id, data => {});
	}

	public void enter(ClubInfo club) {
		mClub = club;
		refresh ();
		joinClubChannel (club.id);
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("list_club_rooms", "club_id", mClub.id, data => {
			ListClubRoom ret = JsonUtility.FromJson<ListClubRoom> (data.ToString ());
			if (ret.errcode != 0)
				return;

			mRooms = ret.data;
			showRooms();
		});
	}

	void showRooms() {
		int cnt = mRooms.Count;

		int uid = GameMgr.getUserMgr ().userid;

		for (int i = 0; i < mRooms.Count; i++) {
			ClubRoomInfo room = mRooms [i];
			Transform item = getItem(i);
			Transform table = item.FindChild ("table");
			bool found = false;

			Debug.Log ("room " + i);
			Debug.Log ("room_tag: " + room.room_tag);

			for (int j = 0; j < room.players.Count; j++) {
				ClubRoomPlayer p = room.players [j];
				Transform s = table.GetChild(j);
				GameObject name = s.FindChild ("name").gameObject;
				GameObject ready = s.FindChild ("ready").gameObject;
				GameObject icon = s.FindChild ("icon").gameObject;
				bool empty = p.id == 0;

				name.SetActive(!empty);
				ready.SetActive(!empty && p.ready);
				icon.SetActive (!empty);

				UIButton btn = s.GetComponent<UIButton>();
				btn.enabled = empty;
				if (empty) {
					btn.onClick.Clear();
					btn.onClick.Add(new EventDelegate(()=>{
						Debug.Log("btn clicked: " + room.room_tag);
						GameMgr.GetInstance().enterRoom(room.room_tag, code=>{
							Debug.Log("club enterRoom code=" + code);
						});
					}));
						
					continue;
				}
			
				name.GetComponent<UILabel>().text = p.name;
				icon.GetComponent<IconLoader>().setUserID(p.id);

				if (p.id == uid) {
					mRoomID = room.id;
					found = true;
				}
			}

			ClubRoomBaseInfo info = room.base_info;
			item.FindChild("desc").GetComponent<UILabel>().text = info.huafen + "/" + info.huafen + (info.maima ? "带苍蝇" : "不带苍蝇") + info.maxGames + "局";
			item.FindChild("progress").GetComponent<UILabel>().text = room.num_of_turns + " / " + info.maxGames;

			item.FindChild("btn_leave").gameObject.SetActive(found);

			if (found) {
				UIButton btnLeave = item.FindChild("btn_leave").GetComponent<UIButton>();
				btnLeave.onClick.Clear();
				btnLeave.onClick.Add(new EventDelegate(()=>{
					leaveRoom(room.id, room.room_tag);
				}));
			}
		}

		updateItems(mRooms.Count);
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
