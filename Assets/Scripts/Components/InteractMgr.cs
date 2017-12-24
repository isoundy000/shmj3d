
using UnityEngine;
using System.Collections.Generic;

public class InteractMgr : MonoBehaviour {
	public static InteractMgr mInstance = null;

	public Transform options;
	public Transform gangOpt;
	public Transform chiOpt;
	public Transform tingOpt;

	GameAction _options = null;
	int _gangState = -1;

	int _tingState = -1;

	void Awake() {
		mInstance = this;

		InitEventHandlers ();
	}

	public static InteractMgr GetInstance() {
		return mInstance;
	}

	void Destroy() {
		mInstance = null;
	}

	void InitEventHandlers() {
		RoomMgr rm = RoomMgr.GetInstance ();
		GameMgr gm = GameMgr.GetInstance ();

		gm.AddHandler ("game_action", data => {
			GameAction action = rm.action;

			if (action == null)
				return;
			
			showAction(action);
		});

		gm.AddHandler ("guo_result", data => {
			hideOptions();
		});

	}

	void addOption(string op) {
		Transform tm = options.FindChild (op);

		if (tm != null)
			tm.gameObject.SetActive (true);
	}

	void hideOptions() {
		options.gameObject.SetActive (false);

		for (int i = 0; i < options.childCount; i++) {
			Transform op = options.GetChild (i);

			op.gameObject.SetActive (false);
		}
	}

	bool hasOptions() {
		return options.gameObject.activeSelf;
	}

	void showAction(GameAction act) {
		hideOptions ();

		if (!act.hasAction ())
			return;

		options.gameObject.SetActive (true);

		addOption ("btn_guo");

		if (act.ting) {
			addOption ("btn_ting");
			// showTings(true);
		}

		if (act.hu)
			addOption ("btn_hu");

		if (act.gang)
			addOption ("btn_gang");

		if (act.peng)
			addOption ("btn_peng");

		if (act.chi)
			addOption ("btn_chi");
	}

	public void onBtnPengClicked() {
		NetMgr.GetInstance ().send ("peng");
	}

	public void onBtnGangClicked() {
		enterGangState (0);
	}

	public void onBtnHuClicked() {
		NetMgr.GetInstance ().send ("hu");
	}

	public void onBtnChiClicked() {
		List<int> types = _options.chitypes;
		int pai = _options.pai;

		if (types.Count > 1) {
			hideOptions ();
			// showChiOptions(pai, types);  TODO
			NetMgr.GetInstance ().send ("chi", "type", types[0]);
		} else {
			NetMgr.GetInstance ().send ("chi", "type", types[0]);
		}
	}

	public void onBtnTingClicked() {
		hideOptions ();
		enterTingState (0);
	}

	public void onBtnGuoClicked() {
		// hideChiOptions();
		NetMgr.GetInstance ().send ("guo");
	}

	void showGangOpt(bool status) {
		gangOpt.gameObject.SetActive (status);
	}

	void enterGangState(int state, int pai = 0) {
		_gangState = state;

		List<int> gp = _options.gangpai;

		switch (state) {
		case 0:
			if (gp.Count == 1) {
				enterGangState (1, gp [0]);
			} else {
				showGangOpt (true);
				// checkGangPai();
			}

			break;
		case 1:
			NetMgr.GetInstance ().send ("gang", "pai", pai);
			enterGangState (-1);
			break;
		case -1:
			showGangOpt (false);
			//checkChuPai (false);
			break;
		default:
			break;
		}
	}

	void showTingOpt(bool status) {
		tingOpt.gameObject.SetActive (status);
	}

	public void onTingCancel() {
		enterTingState (-1);
		//showTings (true);
		NetMgr.GetInstance().send("guo");
	}

	void enterTingState(int state, int pai = 0) {
		_tingState = state;

		switch (state) {
		case 0:
			showTingOpt (true);
			//checkTingPai ();
			break;
		case 1:
			showTingOpt (false);
			//showTingPrompts ();

			NetMgr.GetInstance ().send ("ting", "pai", pai);
			enterTingState (-1);
			break;
		case -1:
			showTingOpt (false);
			//showTings (false);
			//checkChuPai (true);
			//showTingPrompts ();
			break;
		default:
			break;
		}
	}
}
