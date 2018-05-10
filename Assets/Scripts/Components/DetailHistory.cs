
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

[Serializable]
public class DetailOfGame {
	public string base_info;
	public List<int> action_records;
}

[Serializable]
public class GetDetailOfGame {
	public int errcode;
	public string errmsg;
	public DetailOfGame data;
}

[Serializable]
public class GameSeatInfo {
	public List<int> holds;
	public List<int> flowers;
}

[Serializable]
public class GameBaseInfo {
	public string type;
	public int button;
	public int index;
	public List<int> mahjongs;
	public List<GameSeatInfo> game_seats;
	public RoomConf conf;
}

public class DetailHistory : ListBase {
	RoomHistory mRoom = null;
	List<HistoryGame> mGames = null;

	public void enter(RoomHistory room) {
		mRoom = room;
		shrinkContent(0);
		show(() => {
			refresh();
		});
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
			setText(item, "time", PUtils.formatTime (game.create_time, "yyyy/MM/dd HH:mm:ss"));

			Transform seats = item.Find("seats");
			UITable table = seats.GetComponent<UITable>();
			List<HistorySeats> ss = mRoom.info.seats;

			int j = 0;

			for (; j < seats.childCount && j < ss.Count; j++) {
				Transform seat = seats.GetChild(j);
				HistorySeats s = ss[j];

				seat.gameObject.SetActive(true);
				setText(seat, "name", s.name);
				setText(seat, "score", "" + game.result [j]);
				setIcon(seat, "bghead/icon", s.uid);
			}

			for (int k = j; k < seats.childCount; k++) {
				Transform seat = seats.GetChild(k);
				seat.gameObject.SetActive(false);
			}

			table.Reposition();

			setBtnEvent(item, "btn_share", () => {
				
			});

			setBtnEvent(item, "btn_replay", () => {
				onBtnReplay(game.id);
			});
		}

		updateItems(mGames.Count);
	}

	void onBtnReplay(int id) {
		NetMgr nm = NetMgr.GetInstance();
		RoomMgr rm = RoomMgr.GetInstance();

		nm.request_apis ("get_detail_of_game", "id", id, data => {
			GetDetailOfGame ret = JsonUtility.FromJson<GetDetailOfGame> (data.ToString ());
			if (ret.errcode != 0)
				return;

			Debug.Log("base_info: ");
			Debug.Log(ret.data.base_info);	

			GameBaseInfo baseInfo = JsonUtility.FromJson<GameBaseInfo> (ret.data.base_info);
			List<int> actionRecords = ret.data.action_records;

			rm.prepareReplay(mRoom, baseInfo);
			ReplayMgr.GetInstance().Setup(mRoom, baseInfo, actionRecords);

			LoadingScene.LoadNewScene("04.table3d");
		});
	}
}
