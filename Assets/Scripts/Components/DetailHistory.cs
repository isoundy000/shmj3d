
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class HistoryGame {
	public int id;
	public int game_index;
	public int create_time;
	public List<int> result;
}

[Serializable]
public class GetGamesOfRoom {
	public int errcode;
	public string errmsg;
	public List<HistoryGame> data;
}

public class DetailHistory : ListBase {
	RoomHistory mRoom = null;
	List<HistoryGame> mGames = null;

	public void enter(RoomHistory room) {
		mRoom = room;
		refresh();
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		if (mRoom == null)
			return;

		JsonObject ob = new JsonObject();

		ob["room_id"] = mRoom.room_id;
		ob["club_id"] = mRoom.club_id;
		ob["create_time"] = mRoom.create_time;

		nm.request_apis ("get_games_of_room", ob, data => {
			GetGamesOfRoom ret = JsonUtility.FromJson<GetGamesOfRoom> (data.ToString ());
			if (ret.errcode != 0)
				return;

			mGames = ret.data;
			showGames();
		});
	}

	void showGames() {
		if (mRoom == null || mGames == null)
			return;

		setText(transform, "title/roomid", "房间号:" + mRoom.room_id);
		setText(transform, "title/desc", mRoom.info.huafen + "/" + mRoom.info.huafen + (mRoom.info.maima ? "带苍蝇" : "不带苍蝇") + mRoom.info.maxGames + "局");

		for (int i = 0; i < mGames.Count; i++) {
			HistoryGame game = mGames[i];
			Transform item = getItem(i);

			setText(item, "id", "" + (game.game_index + 1));
			setText(item, "time", Utils.formatTime (game.create_time, "yyyy/MM/dd HH:mm:ss"));

			Transform seats = item.Find("seats");
			List<HistorySeats> ss = mRoom.info.seats;

			for (int j = 0; j < seats.childCount && j < ss.Count; j++) {
				Transform seat = seats.GetChild(j);
				HistorySeats s = ss[j];

				setText(seat, "name", s.name);
				setText(seat, "score", "" + game.result [j]);
				setIcon(seat, "bghead/icon", s.uid);
			}

			setBtnEvent(item, "btn_share", () => {
				
			});

			setBtnEvent(item, "btn_replay", () => {

			});
		}

		updateItems(mGames.Count);
	}
}
