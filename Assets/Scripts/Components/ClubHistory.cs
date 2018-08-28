
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ClubHistories {
	public int count;
	public List<RoomHistory> rooms;
};

[Serializable]
public class ListClubHistories {
	public int errcode;
	public string errmsg;
	public ClubHistories data;
}

public class ClubHistory : ListBase {

	int mClubID = 0;
	int mUserID = 0;

	public void enter(int cid, int uid = 0) {
		mClubID = cid;
		mUserID = uid;

		resetNavigator();

		refresh();
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		JsonObject ob = new JsonObject();
		ob ["club_id"] = mClubID;

		if (mUserID != 0)
			ob ["user_id"] = mUserID;

		ob ["limit"] = mNumsPerPage;
		ob ["offset"] = mPage * mNumsPerPage;

		nm.request_apis ("list_club_histories", ob, data => {
			var ret = JsonUtility.FromJson<ListClubHistories>(data.ToString ());
			if (ret.errcode != 0)
				return;

			if (this != null) {
				showHistories(ret.data.rooms);
				mTotal = ret.data.count;
				updateNavigator(refresh);
			}
		});
	}

	void showHistories(List<RoomHistory> rooms) {

		for (int i = 0; i < rooms.Count; i++) {
			Transform item = getItem(i);
			RoomHistory room = rooms[i];
			RoomHistoryInfo info = room.info;

			setText(item, "desc", info.getDesc());
			setText(item, "time", PUtils.formatTime (room.create_time, "yyyy/MM/dd HH:mm:ss"));
			setText(item, "roomid", "房间号: " + room.room_id);
			setText(item, "gamenum", "局数: " + info.game_num);

			setBtnEvent(item, null, () => {
				enterDetail(room);
			});

			Transform seats = item.Find ("seats");
			UITable table = seats.GetComponent<UITable>();
			int index = 0;
			for (int j = 0; j < seats.childCount && j < info.seats.Count; j++, index++) {
				Transform seat = seats.GetChild(j);

				seat.gameObject.SetActive(true);

				setText(seat, "name", PUtils.subString(info.seats[j].name, 5));
				setText(seat, "score", "" + info.seats [j].score);
				setIcon(seat, "bghead/icon", info.seats [j].uid);
			}

			for (int j = index; j < seats.childCount; j++) {
				Transform seat = seats.GetChild (j);
				seat.gameObject.SetActive (false);
			}

			table.Reposition();
		}

		updateItems(rooms.Count);
	}

	void enterDetail(RoomHistory room) {
		var ob = getPage<DetailHistory>("PDetailHistory");
		if (ob != null)
			ob.enter(room);
	}
}
