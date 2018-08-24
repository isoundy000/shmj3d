
using System;
using UnityEngine;
using System.Collections.Generic;

public class Dissolve : MonoBehaviour {

	GameObject mDissolve = null;
	UILabel mInfo = null;

	List<Transform> mSeats = new List<Transform>();

	GameObject mBtnAgree = null;
	GameObject mBtnReject = null;
	GameObject mBtnDissolve = null;

	int mEndTime = 0;
	int _lastSecs = 0;

	void Awake() {
		Transform dv = transform.Find("Dissolve");

		mDissolve = dv.gameObject;
		mInfo = dv.Find("info").GetComponent<UILabel>();

		Transform seats = dv.Find("seats");

		for (int i = 0; i < seats.childCount; i++)
			mSeats.Add(seats.GetChild(i));

		mBtnAgree = dv.Find("btn_agree").gameObject;
		mBtnReject = dv.Find("btn_reject").gameObject;
		mBtnDissolve = dv.Find("btn_dissolve").gameObject;

		mBtnAgree.GetComponent<UIButton>().onClick.Add(new EventDelegate(this, "onBtnAgree"));
		mBtnReject.GetComponent<UIButton>().onClick.Add(new EventDelegate(this, "onBtnReject"));
		mBtnDissolve.GetComponent<UIButton>().onClick.Add(new EventDelegate(this, "onBtnDissolve"));

		GameMgr gm = GameMgr.GetInstance ();

		gm.AddHandler ("game_reset", data => {
			if (this != null) {
				mEndTime = 0;
				mDissolve.SetActive(false);
			}
		});

		gm.AddHandler ("dissolve_notice", data => {
			if (this != null)
				showDissolveNotice((DissolveInfo)data);
		});

		gm.AddHandler ("dissolve_done", data => {
			if (this != null)
				mDissolve.SetActive (false);
		});

		gm.AddHandler ("dissolve_cancel", data => {
			DissolveCancel dc = (DissolveCancel)data;

			if (this != null)
				mDissolve.SetActive (false);

			int uid = dc.reject;
			if (uid > 0 && uid != gm.userMgr.userid)
				GameAlert.GetInstance().show("玩家" + uid + "已拒绝解散请求");
		});
	}

	void Start() {
		RoomMgr rm = RoomMgr.GetInstance();

		if (rm.dissolve != null) {
			showDissolveNotice(rm.dissolve);
			rm.dissolve = null;
		}
	}

	void onBtnAgree() {
		NetMgr.GetInstance().send("dissolve_agree");
	}

	void onBtnReject() {
		NetMgr.GetInstance().send("dissolve_reject");
	}

	void onBtnDissolve() {
		NetMgr.GetInstance().send("dissolve_request");
	}

	void showDissolveNotice(DissolveInfo dv) {
		RoomMgr rm = RoomMgr.GetInstance();

		mDissolve.SetActive (true);

		int now = (int)((DateTime.Now.Ticks - DateTime.Parse("1970-01-01").Ticks) / 10000000);

		mEndTime = dv.time + now;

		UIGrid grid = mSeats[0].GetComponentInParent<UIGrid>();

		int index = 0;
		for (int i = 0; i < rm.players.Count && i < mSeats.Count; i++, index++) {
			Transform s = mSeats[i];
			PlayerInfo p = rm.players[i];

			s.gameObject.SetActive(true);
			s.Find ("bghead/icon").GetComponent<IconLoader>().setUserID(p.userid);
			s.Find("name").GetComponent<UILabel>().text = PUtils.subString(p.name, 5);
		}

		for (int i = index; i < mSeats.Count; i++)
			mSeats[i].gameObject.SetActive(false);

		string[] descs = new string[]{ "等待中", "已拒绝", "已同意", "离线", "申请解散" };

		for (int i = 0; i < rm.players.Count && i < mSeats.Count; i++) {
			Transform status = mSeats [i].Find ("status");

			UILabel desc = status.GetComponentInChildren<UILabel>();
			SpriteMgr sm = status.Find("icon").GetComponent<SpriteMgr>();
			int state = dv.states[i];
			int id = 0;

			if (state > 2)
				id = 4;

			bool online = dv.online[i];
			if (state <= 2) {
				if (!online)
					id = 3;
				else
					id = state;
			}

			sm.setIndex(id);
			desc.text = descs[id];
		}

		grid.Reposition();

		int si = rm.seatindex;
		int st = dv.states[si];
		bool[] check = new bool[]{ false, false, false, false };

		if (dv.reason == "offline") {
			check [2] = true;
		} else {
			if (0 == st) {
				check [0] = true;
				check [1] = true;
			} else
				check [3] = true;
		}

		mBtnAgree.SetActive (check[0]);
		mBtnReject.SetActive (check[1]);
		mBtnDissolve.SetActive (check[2]);
	}

	void Update() {
		if (mEndTime <= 0)
			return;

		int now = (int)((DateTime.Now.Ticks - DateTime.Parse("1970-01-01").Ticks) / 10000000);
		if (now == _lastSecs)
			return;

		_lastSecs = now;

		int last = mEndTime - now;
		if (last < 0) {
			mEndTime = -1;
			return;
		}

		if (mInfo != null)
			mInfo.text = string.Format("{0:D2}:{1:D2}:{2:D2} 后将解散房间", last / 3600, (last % 3600) / 60, last % 60);
	}
}

