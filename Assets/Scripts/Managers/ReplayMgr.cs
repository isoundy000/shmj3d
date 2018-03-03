
using UnityEngine;
using System.Collections.Generic;
using Pomelo.DotNetClient;
using SimpleJson;

public class ReplayAction {
	public int si;
	public ACTION_TYPE type;
	public int pai;

	public ReplayAction(int seatindex, int act, int card) {
		si = seatindex;
		type = (ACTION_TYPE)act;
		pai = card;
	}
}

public enum ACTION_TYPE {
	CHUPAI = 1,
	MOPAI = 2,
	PENG = 3,
	GANG = 4,
	CHI = 5,
	HU = 6,
	ZIMO = 7,
	TING = 8
}

public class ReplayMgr {
	
	static ReplayMgr mInstance = null;
	bool inited = false;

	List<int> actionRecords;
	int current = 0;
	ReplayAction lastAction = null;

	RoomHistory mRoom = null;
	GameBaseInfo mBaseInfo = null;

	public static ReplayMgr GetInstance () {
		if (mInstance == null)
			mInstance = new ReplayMgr ();

		return mInstance;
	}

	public ReplayMgr () {
		actionRecords = new List<int>();
	}

	public void Init() {
		if (inited)
			return;

		inited = true;
	}

	public void clear() {
		lastAction = null;
		current = 0;
		actionRecords.Clear();
	}

	public void Setup(RoomHistory room, GameBaseInfo baseInfo, List<int> records) {
		actionRecords = records;
		mRoom = room;
		mBaseInfo = baseInfo;

		current = 0;
		lastAction = null;
	}

	public bool isReplay() {
		return actionRecords.Count > 0;
	}

	public void gotoAction(int id) {
		current = 0;
		lastAction = null;

		int total = actionRecords.Count / 3;

		if (id >= total) {
			Debug.Log ("replaymgr id > total");
			return;
		}

		int cnt = 0;
		while (cnt < id) {
			takeAction (true);
			cnt++;
		}
	}

	public void prev(int step = 1) {
		int id = current / 3;

		if (id >= step)
			id -= step;
		else
			id = 0;

		NetMgr net = NetMgr.GetInstance();
		PomeloClient pc = net.pc;
		pc.flush();

		RoomMgr rm = RoomMgr.GetInstance();
		rm.prepareReplay(mRoom, mBaseInfo);

		gotoAction(id);
	}

	public void forward(int step = 1) {
		int id = 0;
		while (id < step) {
			takeAction(true);
			id++;
		}
	}

	public int getProgress() {
		int cnt = actionRecords.Count;

		if (cnt == 0)
			return 0;

		return current * 100 / cnt;
	}

	ReplayAction getNext() {
		int total = actionRecords.Count;

		if (current >= total)
			return null;

		ReplayAction ret = new ReplayAction(actionRecords[current], actionRecords[current+1], actionRecords[current+2]);
		current += 3;

		return ret;
	}

	public void sync() {
		NetMgr net = NetMgr.GetInstance();
		PomeloClient pc = net.pc;

		JsonObject data = new JsonObject();
		pc.pseudo ("game_sync_push", data);
	}

	public float takeAction(bool background) {
		NetMgr net = NetMgr.GetInstance();
		RoomMgr rm = RoomMgr.GetInstance();
		PomeloClient pc = net.pc;

		ReplayAction action = getNext();

		if (lastAction != null && lastAction.type == ACTION_TYPE.CHUPAI) {
			if (action != null &&
			    action.type != ACTION_TYPE.PENG &&
			    action.type != ACTION_TYPE.GANG &&
			    action.type != ACTION_TYPE.CHI &&
			    action.type != ACTION_TYPE.HU)
			{
				int _uid = rm.getUIDBySeatIndex(lastAction.si);
				JsonObject guo = new JsonObject();
				guo.Add("userId", _uid);
				guo.Add("seatindex", lastAction.si);
				guo.Add("pai", lastAction.pai);
				if (background)
					guo.Add("bg", true);

				pc.pseudo("guo_notify_push", guo);
			}
		}

		lastAction = action;

		if (action == null)
			return 0;

		JsonObject turn = new JsonObject();
		turn.Add("turn", action.si);
		if (background)
			turn.Add("bg", true);

		float delay = 1.0f;

		int uid = rm.getUIDBySeatIndex(action.si);

		JsonObject data = new JsonObject();
		data.Add("userId", uid);
		data.Add("userid", uid);
		data.Add("pai", action.pai);
		data.Add("seatindex", action.si);
		if (background)
			data.Add("bg", true);

		switch (action.type) {
		case ACTION_TYPE.CHUPAI:
			pc.pseudo ("game_chupai_notify_push", data);
			return 1.0f;
		case ACTION_TYPE.MOPAI:
			if (action.pai >= 45) {
				pc.pseudo ("game_af_push", data);
				return 2.0f;
			} else {
				pc.pseudo ("game_mopai_push", data);
				pc.pseudo ("game_chupai_push", turn);
			}

			return 1.0f;
		case ACTION_TYPE.PENG:
			pc.pseudo ("peng_notify_push", data);
			pc.pseudo ("game_chupai_push", turn);
			return 1.0f;
		case ACTION_TYPE.GANG:
			if (!background)
				pc.pseudo ("hangang_notify_push", data);

			pc.pseudo ("gang_notify_push", data);
			pc.pseudo ("game_chupai_push", turn);
			return 1.0f;
		case ACTION_TYPE.CHI:
			pc.pseudo ("chi_notify_push", data);
			pc.pseudo ("game_chupai_push", turn);
			return 1.0f;
		case ACTION_TYPE.HU:
			data.Add ("hupai", action.pai);
			data.Add ("iszimo", false);
			pc.pseudo ("hu_push", data);
			return 1.5f;
		case ACTION_TYPE.ZIMO:
			data.Add ("hupai", action.pai);
			data.Add ("iszimo", true);
			pc.pseudo ("hu_push", data);
			return 1.5f;
		case ACTION_TYPE.TING:
			pc.pseudo ("ting_notify_push", data);
			return 1.0f;
		default:
			break;

		}

		return delay;
	}
}

