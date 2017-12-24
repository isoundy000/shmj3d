﻿
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ActionInfo {
	public int seatindex;
	public int pai;
}

[Serializable]
public class GangInfo {
	public int seatindex;
	public string gangtype;
	public int pai;
}

[Serializable]
public class TingInfo {
	public int seatindex;
	public List<int> tings;
}

[Serializable]
public class PlayerInfo {
    public int userid;
    public string name;
    public int score;
	public string ip;
	public bool online;
	public bool ready;
	public int seatindex;

	public void reset() {
		userid = 0;
		name = string.Empty;
		score = 0;
		ip = string.Empty;
		online = false;
		ready = false;
	}
}

[Serializable]
public class RoomInfo {
	public string roomid;
	public int numofseats;
	public int numofgames;
}

[Serializable]
public class RoomConf {
	public string type;
	public int creator;
	public int baseScore;
	public int maxFan;
	public int maxGames;
	public int pay;
	public int cost;
	public int huafen;
	public bool maima;
	public bool qidui;
}

[Serializable]
public class GameState {
	// maima state
	public List<int> mas;
	public List<int> scores;
	public int seatindex;
	public int selected;

	// dices
	public int dice1;
	public int dice2;

	// others
	public string state;
	public int button;
	public int numofmj;
	public int turn;
	public int last;
	public int chupai;

	public GameState() {
		mas = new List<int> ();
		scores = new List<int> ();
		seatindex = -1;
		selected = -1;

		dice1 = -1;
		dice2 = -1;

		state = string.Empty;
		button = -1;
		turn = -1;
		last = -1;
		numofmj = 0;
	}
};

[Serializable]
public class SeatInfo {
	public List<int> holds;
	public List<int> folds;
	public List<int> pengs;
	public List<int> chis;
	public List<int> angangs;
	public List<int> diangangs;
	public List<int> wangangs;
	public List<int> tings;
	public List<int> flowers;
	public bool hastingpai;

	public SeatInfo() {
		reset ();
	}

	public void reset() {
		holds = new List<int>();
		folds = new List<int>();
		pengs = new List<int>();
		chis = new List<int>();
		angangs = new List<int>();
		diangangs = new List<int>();
		wangangs = new List<int>();
		tings = new List<int>();
		flowers = new List<int>();
		hastingpai = false;
	}
}

[Serializable]
public class GameAction {
	public int pai;
	public int si;
	public bool peng;
	public bool gang;
	public List<int> gangpai;
	public bool chi;
	public List<int> chitypes;
	public bool ting;
	public List<int> tingouts;
	public bool hu;

	public GameAction() {
		reset ();
	}

	public void reset() {
		pai = -1;
		si = -1;
		peng = false;
		gang = false;
		gangpai = new List<int> ();
		chi = false;
		chitypes = new List<int> ();
		ting = false;
		tingouts = new List<int> ();
		hu = false;
	}

	public bool hasAction() {
		return (peng || gang || chi || ting || hu);
	}
}

public class RoomMgr {
    public static RoomMgr mInstance = null;

	public RoomInfo info;
	public RoomConf conf;
	public List<PlayerInfo> players;
	public GameState state;
	public List<SeatInfo> seats;
	public GameAction action;

	public int seatindex;
	public int numOfHolds = 13;

    bool inited = false;

	public static RoomMgr GetInstance () {
		if (mInstance == null)
			mInstance = new RoomMgr ();

		return mInstance;
	}

	public RoomMgr () {
		reset ();
	}

	public void Init() {
		if (inited)
			return;

		inited = true;
	}

	public bool isIdle() {
		return info.numofgames == 0;
	}

	public bool isPlaying() {
		string st = state.state;
		return st == "begin" || st == "playing" || st == "maima";
	}

	public bool isMyTurn() {
		return seatindex == state.turn;
	}

	public PlayerInfo getSelfPlayer() {
		return players[seatindex];
	}

	public SeatInfo getSelfSeat() {
		return seats[seatindex];
	}

	public void updateRoom(JsonObject room) {
		JsonUtility.FromJsonOverwrite (room.ToString(), info);
		JsonUtility.FromJsonOverwrite (room ["conf"].ToString(), conf);

		JsonArray _seats = room ["seats"] as JsonArray;

		players.Clear ();
		seats.Clear ();

		int userid = GameMgr.GetInstance ().userMgr.userid;

		for (int i = 0; i < _seats.Count; i++) {
			JsonObject seat = (JsonObject)_seats[i];
			PlayerInfo player = JsonUtility.FromJson<PlayerInfo>(seat.ToString());

			players.Add (player);
			seats.Add (new SeatInfo());

			if (userid == player.userid)
				seatindex = i;
		}
	}

	public PlayerInfo findPlayer(int userid) {
		for (int i = 0; i < players.Count; i++) {
			PlayerInfo p = players [i];
			if (p.userid == userid)
				return p;
		}

		return null;
	}

	public SeatInfo findSeat(int userid) {
		PlayerInfo p = findPlayer (userid);

		if (p != null)
			return seats[p.seatindex];

		return null;
	}

	public void reset() {
		info = new RoomInfo();
		conf = new RoomConf();
		players = new List<PlayerInfo> ();
		state = new GameState ();
		seats = new List<SeatInfo> ();
		action = new GameAction ();
	}

	public void newRound() {
		state = new GameState ();

		foreach (SeatInfo seat in seats) {
			seat.reset ();
		}

		action = new GameAction ();
	}

	public int userExit(int userid) {
		PlayerInfo p = findPlayer (userid);

		if (p != null) {
			p.reset ();
			return p.seatindex;
		}

		return -1;
	}

	public int newUserCome(JsonObject data) {
		int id = Convert.ToInt32(data ["seatindex"]);
		JsonUtility.FromJsonOverwrite (data.ToString(), players[id]);
		return id;
	}

	public int updateUser(JsonObject data) {
		PlayerInfo p = findPlayer (Convert.ToInt32(data["userid"]));

		if (p != null) {
			JsonUtility.FromJsonOverwrite (data.ToString(), p);
			return p.seatindex;
		}

		return -1;
	}

	public void updateState(JsonObject data) {
		JsonUtility.FromJsonOverwrite (data.ToString(), state);
	}

	public int updateSeat(JsonObject data) {
		int id = Convert.ToInt32(data ["seatindex"]);
		JsonUtility.FromJsonOverwrite (data.ToString(), seats[id]);
		return id;
	}

	public void updateSeats(JsonArray data) {
		for (int i = 0; i < data.Count; i++)
			JsonUtility.FromJsonOverwrite (data[i].ToString(), seats[i]);
	}

	public void updateAction(JsonObject data) {
		if (data == null)
			action.reset ();
		else
			action = JsonUtility.FromJson<GameAction>(data.ToString());
	}

	public void updateRoomInfo(JsonObject data) {
		JsonUtility.FromJsonOverwrite (data.ToString (), info);
	}

	public ActionInfo doChupai(JsonObject data) {
		ActionInfo info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = info.pai;
		state.chupai = pai;
		SeatInfo seat = seats[info.seatindex];
		List<int> holds = seat.holds;

		if (holds.Count > 0)
			removeFromList (holds, pai);

		return info;
	}

	public ActionInfo doAddFlower(JsonObject data) {
		ActionInfo info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = info.pai;
		SeatInfo seat = seats[info.seatindex];
		List<int> flowers = seat.flowers;

		flowers.Add (pai);

		return info;
	}

	public ActionInfo doMopai(JsonObject data) {
		ActionInfo info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = info.pai;
		SeatInfo seat = seats[info.seatindex];
		List<int> holds = seat.holds;

		if (holds.Count > 0 && pai >= 0)
			holds.Add (pai);

		return info;
	}

	public ActionInfo doGuo(JsonObject data) {
		ActionInfo info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = info.pai;
		SeatInfo seat = seats[info.seatindex];
		List<int> folds = seat.folds;

		folds.Add (pai);

		return info;
	}

	public ActionInfo doPeng(JsonObject data) {
		ActionInfo info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = info.pai;
		SeatInfo seat = seats[info.seatindex];
		List<int> holds = seat.holds;
		List<int> pengs = seat.pengs;

		int c = pai % 100;

		if (holds.Count > 0)
			removeFromList (holds, c, 2);

		pengs.Add (pai);

		return info;
	}

	public List<int> getChiArr(int pai, bool ign) {
		int type = pai / 100;
		int c = pai % 100;

		int begin = c - type;

		List<int> arr = new List<int> ();
		for (int i = 0; i < 3; i++) {
			int k = begin + i;
			if (ign && k == c)
				continue;

			arr.Add (k);
		}

		return arr;
	}

	bool removeFromList(List<int> list, int pai, int cnt = 1) {
		int found = 0;
		for (int i = 0; i < list.Count; ) {
			if (list [i] == pai) {
				list.RemoveAt (i);
				found++;

				if (found == cnt)
					return true;

				continue;
			}

			i++;
		}

		return false;
	}

	public ActionInfo doChi(JsonObject data) {
		ActionInfo info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = info.pai;
		SeatInfo seat = seats[info.seatindex];
		List<int> holds = seat.holds;
		List<int> chis = seat.chis;

		if (holds.Count > 0) {
			List<int> mjs = getChiArr (pai, true);
			for (int i = 0; i < 2; i++)
				removeFromList (holds, mjs[i]);
		}

		chis.Add (pai);

		return info;
	}

	public GangInfo doGang(JsonObject data) {
		GangInfo info = JsonUtility.FromJson<GangInfo>(data.ToString());

		int pai = info.pai;
		string gangtype = info.gangtype;
		SeatInfo seat = seats[info.seatindex];
		List<int> holds = seat.holds;
		List<int> pengs = seat.pengs;

		int c = pai % 100;

		if ("wangang" == gangtype) { // wangang
			for (int i = 0; i < pengs.Count; i++) {
				if (pai == pengs[i]) {
					pengs.RemoveAt (i);
					break;
				}
			}

			removeFromList (holds, c);
			seat.wangangs.Add (pai);
		} else if ("diangang" == gangtype) {	// diangang
			removeFromList (holds, c, 3);
			seat.diangangs.Add (pai);
		} else if ("angang" == gangtype) { // angang
			removeFromList (holds, c, 4);
			seat.angangs.Add (pai);
		}

		return info;
	}

	public TingInfo doTing(JsonObject data) {
		TingInfo info = JsonUtility.FromJson<TingInfo>(data.ToString());

		SeatInfo seat = seats[info.seatindex];
		seat.tings = info.tings;
		seat.hastingpai = true;

		return info;
	}
}

