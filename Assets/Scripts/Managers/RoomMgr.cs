
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class HandCardsInfo {
	public int seatindex;
	public List<int> holds;
	public List<int> lens;
}

[Serializable]
public class ActionInfo {
	public int seatindex;
	public int pai;
	public bool bg;
}

[Serializable]
public class GangInfo {
	public int seatindex;
	public string gangtype;
	public int pai;
	public bool bg;
}

[Serializable]
public class TingInfo {
	public int seatindex;
	public List<int> tings;
	public List<HuPai> hus;
	public bool bg;
}

[Serializable]
public class DissolveInfo {
	public int time;
	public string reason;
	public List<int> states;
	public List<bool> online;
}

[Serializable]
public class DissolveCancel {
	public int reject;
}

[Serializable]
public class Flowers {
	public List<int> flowers;
}

[Serializable]
public class HandFlowers {
	public List<Flowers> hf;
}

[Serializable]
public class PlayerInfo {
    public int userid;
    public string name;
	public int sex;
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
	public int numOfSeats;
	public int pay;
	public int cost;
	public int huafen;
	public bool maima;
	public bool qidui;

	public bool jyw;
	public bool j7w;
	public bool ryj;

	public bool limit_ip;
	public bool limit_gps;

	public string getDesc() {
		var tips = new List<string> ();
		if (type == "shmj") {
			tips.Add ("上海敲麻");
			tips.Add ("花分" + huafen);

			if (maima)
				tips.Add ("飞苍蝇");

			if (maxFan > 10)
				tips.Add ("不封顶");
			else
				tips.Add ("封顶" + maxFan + "番");

			if (qidui)
				tips.Add ("七对");
		} else if (type == "gzmj") {
			tips.Add ("酒都麻将");
			if (jyw) tips.Add ("金银乌");
			if (j7w) tips.Add ("见7挖");
			if (ryj) tips.Add ("软硬鸡");
		}

		tips.Add (numOfSeats + "人");
		tips.Add (maxGames + "局");

		if (limit_gps)
			tips.Add ("限制距离");

		if (limit_ip)
			tips.Add ("限制IP");

		return string.Join (" ", tips.ToArray ());
	}
}

[Serializable]
public class GameState {
	public GameMaima maima;

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
	public List<HuPai> hus;
	public List<int> flowers;
	public List<int> limit;
	public int que;
	public int len;
	public bool tingpai;
	public bool hued;

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
		hus = new List<HuPai>();
		flowers = new List<int>();
		limit = new List<int>();
		tingpai = false;
		hued = false;
		que = 0;
		len = 13;
	}

    public int getCPGCnt() {
        return pengs.Count + chis.Count + angangs.Count + diangangs.Count + wangangs.Count;
    }

	public bool isHoldsValid() {
		return holds.Count > 0;
	}

	public int getHoldsLen() {
		int cnt = holds.Count;

		return cnt > 0 ? cnt : len;
	}

	public void update() {
		len = 13 - getCPGCnt() * 3;
	}
}

[Serializable]
public class HuPai {
	public int score;
	public int num;
	public int pai;
}

[Serializable]
public class TingOut {
	public int pai;
	public List<HuPai> hus;
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

	public List<TingOut> help;

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
		help = new List<TingOut>();
	}

	public bool hasAction() {
		return (peng || gang || chi || ting || hu);
	}
}

[Serializable]
public class HuInfo {
	public bool hued;
	public bool zimo;
	public bool fangpao;
	public int pai;
}

[Serializable]
public class ResultDetail {
	public string tips;
}

[Serializable]
public class GameOverPlayerInfo {
	public int userid;
	public string name;
	public List<int> pengs;
	public List<int> chis;
	public List<int> wangangs;
	public List<int> diangangs;
	public List<int> angangs;
	public List<int> holds;
	public int ma;
	public int chicken;
	public int score;
	public int totalscore;
	public int button;
	public HuInfo hu;
	public ResultDetail detail;
}

[Serializable]
public class GameEndInfo {
	public int numzimo;
	public int numjiepao;
	public int numdianpao;
	public int numangang;
	public int numminggang;
}

[Serializable]
public class GameEndFlags {
	public bool dissolve;
	public bool end;
	public bool huangzhuang;
	public GameMaima maima;
}

[Serializable]
public class GameMaima {
	public List<int> mas;
	public List<int> scores;
	public int seatindex;
	public int selected;

	public GameMaima() {
		mas = new List<int>();
		scores = new List<int>();
		seatindex = -1;
		selected = -1;
	}
}

[Serializable]
public class GameOverInfo {
	public List<GameOverPlayerInfo> results;
	public List<GameEndInfo> endinfo;
	public GameEndFlags info;
}

[Serializable]
public class DingQueInfo {
	public List<int> ques;
}

public class RoomMgr {
    public static RoomMgr mInstance = null;

	public RoomInfo info;
	public RoomConf conf;
	public List<PlayerInfo> players;
	public GameState state;
	public List<SeatInfo> seats;
	public GameAction action;
	public GameOverInfo overinfo;

	public bool dingqueDone = false;

	public List<GameOverInfo> histories;

	public DissolveInfo dissolve;

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

	public bool isClubRoom() {
		return info.roomid != null && info.roomid.StartsWith ("c");
	}

	public bool isOwner() {
		return !isClubRoom() && seatindex == 0;
	}

	public bool isButton() {
		return seatindex == state.button;
	}

	public bool isPlaying() {
		string st = state.state;
		return st == "begin" || st == "playing" || st == "maima"  || st == "dingque";
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

	public int getSeatIndexByID(int uid) {
		PlayerInfo p = findPlayer (uid);
		if (p != null)
			return p.seatindex;

		return -1;
	}

	public int getUIDBySeatIndex(int si) {
		PlayerInfo p = players[si];
		return p.userid;
	}

	public string getWanfa() {
		if (conf == null)
			return "";

		return conf.getDesc ();
	}

	public void updateRoom(JsonObject room) {
		JsonUtility.FromJsonOverwrite (room.ToString(), info);
		JsonUtility.FromJsonOverwrite (room ["conf"].ToString(), conf);

		JsonArray _seats = room ["seats"] as JsonArray;
		JsonArray _histories = room["histories"] as JsonArray;

		players.Clear ();
		seats.Clear ();
		histories.Clear ();

		int userid = GameMgr.GetInstance ().userMgr.userid;

		for (int i = 0; i < _seats.Count; i++) {
			JsonObject seat = (JsonObject)_seats[i];
			PlayerInfo player = JsonUtility.FromJson<PlayerInfo>(seat.ToString());

			players.Add (player);
			seats.Add (new SeatInfo());

			if (userid == player.userid)
				seatindex = i;
		}

		for (int i = 0; i < _histories.Count; i++) {
			JsonObject history = (JsonObject)_histories[i];
			GameOverInfo info = JsonUtility.FromJson<GameOverInfo>(history.ToString());

			histories.Add (info);
		}

		Debug.Log ("_histories count: " + _histories.Count);
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
		overinfo = new GameOverInfo();
		histories = new List<GameOverInfo>();

		dissolve = null;
		dingqueDone = false;
	}

	public int numOfSeats() {
		return info.numofseats;
	}

	public void prepareReplay(RoomHistory room, GameBaseInfo baseInfo) {
		reset();

		info.roomid = room.room_tag;
		info.numofseats = room.info.seats.Count;
		info.numofgames = baseInfo.index + 1;

		int nSeats = room.info.seats.Count;

		state.state = "playing";

		for (int i = 0; i < nSeats; i++) {
			PlayerInfo p = new PlayerInfo();
			p.reset();

			HistorySeats hs = room.info.seats[i];
			p.name = hs.name;
			p.score = hs.score;
			p.userid = hs.uid;
			p.seatindex = i;
			p.online = true;
			p.ip = "127.0.0.1";
			p.ready = true;

			players.Add(p);

			SeatInfo s = new SeatInfo();
			s.reset();

			s.holds = new List<int>(baseInfo.game_seats[i].holds);
			s.flowers = new List<int>(baseInfo.game_seats[i].flowers);
			seats.Add(s);

			if (p.userid == GameMgr.GetInstance ().userMgr.userid)
				seatindex = i;
		}

		if (seatindex < 0)
			seatindex = 0;

		conf = baseInfo.conf;

		int button = baseInfo.button;

		state.button = button;
		state.turn = button;

		int count = baseInfo.mahjongs.Count;

		foreach (GameSeatInfo seat in baseInfo.game_seats)
			count -= seat.holds.Count + seat.flowers.Count;

		state.numofmj = count;
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

	public void updateMaima(JsonObject data) {
		state.maima = JsonUtility.FromJson<GameMaima> (data.ToString());
	}

	public void updateDingque(JsonObject data) {
		var dingque = JsonUtility.FromJson<DingQueInfo> (data.ToString());

		bool done = true;

		for (int i = 0; i < seats.Count; i++) {
			var seat = seats[i];
			var q = dingque.ques[i];

			seat.que = q;
			if (q == 0)
				done = false;
		}

		dingqueDone = done;
	}

	public int updateSeat(JsonObject data) {
		int id = Convert.ToInt32(data ["seatindex"]);
		JsonUtility.FromJsonOverwrite (data.ToString(), seats[id]);
		return id;
	}

	public void updateSeats(JsonArray data) {
		bool done = true;

		for (int i = 0; i < data.Count; i++) {
			JsonUtility.FromJsonOverwrite (data[i].ToString(), seats[i]);
			if (seats [i].que == 0)
				done = false;
		}

		dingqueDone = done;
	}

	public void updateHandCards(JsonObject data) {
		HandCardsInfo info = JsonUtility.FromJson<HandCardsInfo> (data.ToString ());

		for (int i = 0; i < seats.Count; i++) {
			var st = seats[i];
			if (i == seatindex)
				st.holds = new List<int> (info.holds);
			else
				st.len = info.lens[i];
		}
	}

	public void updateAction(JsonObject data) {
		if (data == null)
			action.reset ();
		else {
			Debug.Log ("action:" + data.ToString());
			action = JsonUtility.FromJson<GameAction> (data.ToString ());
		}
	}

	public void updateRoomInfo(JsonObject data) {
		JsonUtility.FromJsonOverwrite (data.ToString (), info);
	}

	public ActionInfo doChupai(JsonObject data) {
		ActionInfo _info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = _info.pai %  100;
		state.chupai = pai;
		SeatInfo seat = seats[_info.seatindex];
		List<int> holds = seat.holds;

		if (holds.Count > 0)
			removeFromList (holds, pai);

		seat.limit.Clear();

		return _info;
	}

	public ActionInfo doAddFlower(JsonObject data) {
		ActionInfo _info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = _info.pai;
		SeatInfo seat = seats[_info.seatindex];
		List<int> flowers = seat.flowers;

		flowers.Add (pai);

		return _info;
	}

	public ActionInfo doMopai(JsonObject data) {
		ActionInfo _info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = _info.pai;
		SeatInfo seat = seats[_info.seatindex];
		List<int> holds = seat.holds;

		if (holds.Count > 0 && pai >= 0)
			holds.Add (pai);

		return _info;
	}

	public ActionInfo doGuo(JsonObject data) {
		ActionInfo _info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = _info.pai;
		SeatInfo seat = seats[_info.seatindex];
		List<int> folds = seat.folds;

		folds.Add (pai);

		return _info;
	}

	public ActionInfo doPeng(JsonObject data) {
		ActionInfo _info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = _info.pai;
		SeatInfo seat = seats[_info.seatindex];
		List<int> holds = seat.holds;
		List<int> pengs = seat.pengs;

		int c = pai % 100;

		if (holds.Count > 0)
			removeFromList (holds, c, 2);

		pengs.Add (pai);

		return _info;
	}

	public static List<int> getChiArr(int pai, bool ign = false) {
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
		ActionInfo _info = JsonUtility.FromJson<ActionInfo>(data.ToString());

		int pai = _info.pai;
		SeatInfo seat = seats[_info.seatindex];
		List<int> holds = seat.holds;
		List<int> chis = seat.chis;

		if (holds.Count > 0) {
			List<int> mjs = getChiArr (pai, true);
			for (int i = 0; i < 2; i++)
				removeFromList (holds, mjs[i]);
		}

		chis.Add (pai);

		return _info;
	}

	string getGangType(SeatInfo seat, int pai) {
		List<int> pengs = seat.pengs;
		for (int i = 0; i < pengs.Count; i++) {
			int c = pengs [i] % 100;

			if (c == pai)
				return "wangang";
		}

		int cnt = 0;
		foreach (int i in seat.holds) {
			if (i == pai)
				cnt++;
		}

		if (cnt == 3)
			return "diangang";
		else if (cnt == 4)
			return "angang";
		else {
			Debug.LogError("unknown gangtype: cnt=" + cnt);
			return "unknown";
		}
	}

	public GangInfo doGang(JsonObject data) {
		GangInfo _info = JsonUtility.FromJson<GangInfo>(data.ToString());

		int pai = _info.pai;
		string gangtype = _info.gangtype;
		SeatInfo seat = seats[_info.seatindex];
		List<int> holds = seat.holds;
		List<int> pengs = seat.pengs;

		int c = pai % 100;

		if (gangtype == null || gangtype == "") {
			gangtype = getGangType(seat, c);
			_info.gangtype = gangtype;
		}

		Debug.Log ("gangtype=" + gangtype);

		if ("wangang" == gangtype) { // wangang
			for (int i = 0; i < pengs.Count; i++) {
				if (c == pengs[i] % 100) {
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

		return _info;
	}

	public TingInfo doTing(JsonObject data) {
		TingInfo _info = JsonUtility.FromJson<TingInfo>(data.ToString());

		SeatInfo seat = seats[_info.seatindex];
		seat.tings = _info.tings;
		seat.hus = _info.hus;
		seat.tingpai = true;

		return _info;
	}

	public GameOverInfo updateOverInfo(JsonObject data) {
		GameOverInfo _info = JsonUtility.FromJson<GameOverInfo> (data.ToString ());

		overinfo = _info;
		histories.Add(_info);

		bool dissolve = _info.results.Count == 0;

		if (!dissolve) {
			for (int i = 0; i < players.Count; i++)
				players [i].score = _info.results [i].totalscore;
		}

		return _info;
	}

	public void updateFlowers(JsonObject data) {
		HandFlowers flowers = JsonUtility.FromJson<HandFlowers> (data.ToString ());

		if (seats == null)
			Debug.LogError("seats null");

		Debug.Log(data.ToString());

		for (int i = 0; i < seats.Count; i++)
			seats[i].flowers = flowers.hf[i].flowers;
	}

	public HuPushInfo updateHu(JsonObject data) {
		HuPushInfo info = JsonUtility.FromJson<HuPushInfo>(data.ToString());

		seats[info.seatindex].hued = true;
		return info;
	}

	public void updateLimit(int si, List<int> limit) {
		seats[si].limit = limit;
	}

	public int[] getValidLocalIDs() {
		switch (info.numofseats) {
		case 2:
			return new int[] { 0, 2 };
		case 3:
			return new int[] { 0, 1, 3 };
		case 4:
		default:
			return new int[] { 0, 1, 2, 3 };
		}
	}

	public int getLocalIndex(int si) {
		int nSeats = info.numofseats;

		if (si >= nSeats)
			Debug.LogError("si out of range");

		int[] ids = getValidLocalIDs ();
		int id = (si - seatindex + nSeats) % nSeats;

		return ids [id];
	}

	public int getLocalIndexByID(int uid) {
		int sid = getSeatIndexByID (uid);

		return getLocalIndex(sid);
	}

	public int getSeatIndexByLocal(int local) {
		int[] ids = getValidLocalIDs ();

		int id = -1;
		int cnt = ids.Length;

		for (int i = 0; i < cnt; i++) {
			if (ids [i] == local) {
				id = i;
				break;
			}
		}

		if (id < 0)
			return id;

		return (id + seatindex) % cnt;
	}

	public bool isHoldsValid(int si) {
		SeatInfo st = seats[si];

		return st.isHoldsValid();
	}

	public int getHoldsLen(int si) {
		SeatInfo st = seats[si];

		return st.getHoldsLen();
	}

	int getLeftCount(int pai) {
		int count = 4;

		for (int i = 0; i < seats.Count; i++) {
			SeatInfo seat = seats[i];

			if (i == seatindex) {
				foreach (int c in seat.holds) {
					if (c == pai)
						count--;
				}
			}

			foreach (int c in seat.folds) {
				if (c % 100 == pai)
					count--;
			}

			foreach (int c in seat.pengs) {
				if (c % 100 == pai)
					count -= 3;
			}

			foreach (int c in seat.chis) {
				var mjs = getChiArr (c, false);
				for (int j = 0; j < 3; j++) {
					if (mjs [j] == pai)
						count--;
				}
			}

			foreach (int c in seat.angangs) {
				if (c % 100 == pai)
					count -= 4;
			}

			foreach (int c in seat.wangangs) {
				if (c % 100 == pai)
					count -= 4;
			}

			foreach (int c in seat.diangangs) {
				if (c % 100 == pai)
					count -= 4;
			}
		}

		return count;
	}

	public void updateHus() {
		SeatInfo seat = getSelfSeat();
		var hus = seat.hus;


		foreach (HuPai hu in hus) {
			hu.num = getLeftCount(hu.pai);
		}
	}

	public bool limitLocation() {
		return conf.limit_gps;
	}
}


