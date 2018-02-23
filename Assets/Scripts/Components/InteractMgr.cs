
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

	HandCardItem selected = null;
	Vector3 selPos = Vector3.zero;

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
			_options = action;

			if (action == null)
				return;
			
			showAction(action);
		});

		gm.AddHandler ("guo_result", data => {
			hideOptions();
		});
	}

	void addOption(string op, int pai = 0) {
		Transform tm = options.Find (op);

		if (tm == null)
			return;

		Mahjong2D mj = tm.GetComponentInChildren<Mahjong2D>();
		
		tm.gameObject.SetActive (true);

		if (mj != null)
			mj.gameObject.SetActive(pai != 0);

		if (pai == 0)
			return;

		if (mj != null)
			mj.setID(pai);
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

		int pai = act.pai % 100;

		if (act.ting) {
			addOption ("btn_ting");
			// showTings(true);
		}

		if (act.hu)
			addOption ("btn_hu", pai);

		if (act.gang) {
			int gang = act.gangpai.Count > 1 ? 0 : pai;
			addOption ("btn_gang", gang);
		}

		if (act.peng)
			addOption ("btn_peng", pai);

		if (act.chi)
			addOption ("btn_chi", pai);

		options.GetComponent<UIGrid> ().Reposition ();
	}

	public void onMJClicked(HandCardItem item) {
        if (item == null || item._obj == null)
            return;

		if (_gangState == 0) {
			onMJChoosed (item);
			return;
		}

		HandCardItem old = selected;
		if (old != null && item.Equals(old)) {
			shoot (item);

			old._obj.transform.position = selPos;
			selected = null;
			selPos = Vector3.zero;
			//showTingPrompts ();
			return;
		}

		if (old != null && old._obj != null)
			old._obj.transform.position = selPos;

		selPos = item._obj.transform.position;
		selected = item;

		item._obj.transform.Translate (0, 0.01f, 0);

		onMJChoosed (item);
	}

	void onMJChoosed(HandCardItem item) {
		if (_tingState == 0) {
			// TODO
		} else if (_gangState == 0) {
			enterGangState (1, item._id);
		} else {
			// TODO
		}
	}

	void shoot(HandCardItem item) {
		int mjid = item._id;
		NetMgr nm = NetMgr.GetInstance ();

		if (_tingState == 0) {
			enterTingState (1, mjid);
		} else {
			if (hasOptions ())
				nm.send ("guo");

			//showTings(false);

			nm.send ("chupai", "pai", mjid);
		}
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
			showChiOptions(pai, types);
		} else {
			NetMgr.GetInstance ().send ("chi", "type", types[0]);
		}
	}

	void hideChiOptions() {
		chiOpt.gameObject.SetActive (false);
	}

	void showChiOptions(int pai, List<int> types) {
		chiOpt.gameObject.SetActive (true);

		types.Sort ((a, b) => { return b - a; });

		NetMgr nm = NetMgr.GetInstance ();
		Transform chis = chiOpt.Find ("chis");

		int i = 0;
		for (i = 0; i < types.Count; i++) {
			int type = types [i];
			Transform chi = chis.GetChild (i);
			List<int> arr = RoomMgr.getChiArr(type * 100 + pai, true);

			chi.gameObject.SetActive(true);

			for (int j = 0; j < arr.Count; j++)
				chi.GetChild(j).GetComponent<Mahjong2D>().setID(arr[j]);

			Utils.onClick(chi, ()=>{
				chiOpt.gameObject.SetActive (false);
				nm.send("chi", "type", type);
			});
		}

		for (int j = i; j < 3; j++)
			chis.GetChild(j).gameObject.SetActive (false);

		chis.GetComponent<UIGrid> ().Reposition ();
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
			checkChuPai (false);
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
			checkTingPai ();
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
			checkChuPai (true);
			//showTingPrompts ();
			break;
		default:
			break;
		}
	}

	DHM_HandCardManager getHandCardManager() {
		return GameManager.GetInstance ().getSelfCardManager ().getHCM ();
	}

	public void checkTingPai() {
		Debug.Log ("checkTingPai");

		List<int> tingouts = _options.tingouts;
		DHM_HandCardManager hcm = getHandCardManager ();
		List<HandCardItem> list = new List<HandCardItem>(hcm._handCardList);

		if (hcm._MoHand != null)
			list.Add (hcm._MoHand);

		for (int i = 0; i < list.Count; i++) {
			HandCardItem item = list [i];
			bool check = tingouts == null || tingouts.FindIndex(x=> x == item._id) >= 0;
			HandCard hc = item._obj.GetComponent<HandCard> ();

			hc.setInteractable (check);
		}
	}

	public void checkChuPai(bool check) {
		Debug.Log ("checkChuPai: " + check.ToString());

		DHM_HandCardManager hcm = getHandCardManager ();
		List<HandCardItem> list = new List<HandCardItem>(hcm._handCardList);

		if (hcm._MoHand != null)
			list.Add (hcm._MoHand);

		bool hastingpai = RoomMgr.GetInstance ().getSelfSeat ().hastingpai;
		bool show = check && !hastingpai;

		foreach (HandCardItem item in list) {
			HandCard hc = item._obj.GetComponent<HandCard> ();
			hc.setInteractable (show);
		}
	}
}
