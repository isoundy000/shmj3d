
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ListClubHistory {
	public int errcode;
	public string errmsg;
	public List<RoomHistory> data;
}

public class ClubHistory : ListBase {
	List<RoomHistory> mHistory;

	int mClubID = 0;
	int mUserID = 0;

	public void enter(int cid, int uid = 0) {
		mClubID = cid;
		mUserID = uid;
		refresh();
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		JsonObject ob = new JsonObject();
		ob ["club_id"] = mClubID;

		if (mUserID != 0)
			ob ["user_id"] = mUserID;

		nm.request_apis ("list_club_history", ob, data => {
			ListClubHistory ret = JsonUtility.FromJson<ListClubHistory>(data.ToString ());
			if (ret.errcode != 0)
				return;

			mHistory = ret.data;
			showHistories();
		});
	}

	void showHistories() {
		for (int i = 0; i < mHistory.Count; i++) {
			Transform item = getItem(i);
			RoomHistory room = mHistory[i];
			RoomHistoryInfo info = room.info;

			setText(item, "desc", info.huafen + "/" + info.huafen + (info.maima ? "带苍蝇" : "不带苍蝇") + info.maxGames + "局");
			setText(item, "time", Utils.formatTime (room.create_time, "yyyy/MM/dd HH:mm:ss"));
			setText(item, "roomid", "房间号: " + room.room_id);
			setText(item, "gamenum", "局数: " + info.game_num);

			setBtnEvent(item, "btn_detail", () => {
				enterDetail(room);
			});

			Transform seats = item.Find ("seats");
			int index = 0;
			for (int j = 0; j < seats.childCount && j < info.seats.Count; j++, index++) {
				Transform seat = seats.GetChild(j);

				seat.gameObject.SetActive(true);

				setText(seat, "name", info.seats [j].name);
				setText(seat, "score", "" + info.seats [j].score);
				setIcon(seat, "bghead/icon", info.seats [j].uid);
			}

			for (int j = index; j < seats.childCount; j++) {
				Transform seat = seats.GetChild (j);
				seat.gameObject.SetActive (false);
			}
		}

		updateItems(mHistory.Count);
	}

	void enterDetail(RoomHistory room) {
		GameObject ob = GameObject.Find ("PDetailHistory");
		ob.GetComponent<DetailHistory>().enter(room);
	}
}
