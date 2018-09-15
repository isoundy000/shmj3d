
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;

public class InteractMgr : MonoBehaviour {
	public static InteractMgr mInstance = null;

	public Transform options;
	public Transform gangOpt;
	public Transform chiOpt;
	public Transform tingOpt;

	public GameObject dqItem;
	public GameObject qiaoItem;
	public GameObject chicken;

	public UILabel warning;

	float lastGpsCheck = 0;

	GameAction _options = null;
	int _gangState = -1;
	int _tingState = -1;

	HandCardItem selected = null;
	Vector3 selPos = Vector3.zero;

	bool shot = false;

	void Awake() {
		mInstance = this;
	}

	void Start() {
		InitEventHandlers ();
	}

	public static InteractMgr GetInstance() {
		return mInstance;
	}

	void Destroy() {
		mInstance = null;
	}

	public void reset() {
		hidePrompt();
		hideOptions();
		showQiaoHelp(false);
		refreshDQ(false);
		_options = null;
		shot = false;
	}

	void InitEventHandlers() {
		RoomMgr rm = RoomMgr.GetInstance ();
		GameMgr gm = GameMgr.GetInstance ();

/*
		gm.AddHandler ("game_action", data => {
			GameAction action = rm.action;
			_options = action;

			showAction(action);
		});
*/

		gm.AddHandler ("guo_result", data => {
			if (this != null)
				hideOptions();
		});

		gm.AddHandler ("game_begin", data => {
			if (this != null)
				reset();
		});

		gm.AddHandler ("game_over", data => {
			if (this != null)
				reset();
		});
/*
		gm.AddHandler ("game_playing", data => {
			if (this != null)
				checkChuPai(rm.isMyTurn());
		});
*/
		gm.AddHandler ("game_sync", data => {
			if (this == null)
				return;

			showPrompt();
			shot = false;

			if (rm.state.state == "dingque")
				refreshDQ();
		});

		gm.AddHandler ("game_chupai_notify", data => {
			ActionInfo info = (ActionInfo)data;

			if (this == null)
				return;

			if (info.seatindex == rm.seatindex) {
				Highlight(info.pai, false);
				checkChuPai(false);
			}
		});

		gm.AddHandler ("game_turn_change", data => {
			if (this == null)
				return;

			if (rm.isMyTurn())
				checkChuPai(true);

			shot = false;
		});

		gm.AddHandler ("hupai", data => {
			if (this != null)
				lockHandCards();
		});

		gm.AddHandler ("location_warning", data => {
			LocationWarning lw = (LocationWarning)data;

			if (this != null)
				warning.text = lw.text;
		});

		gm.AddHandler ("game_dingque", data => {
			if (this != null)
				refreshDQ ();
		});

		var ques = dqItem.transform.Find ("ques");

		PUtils.setBtnEvent (ques, "que_wan", () => dingque (1));
		PUtils.setBtnEvent (ques, "que_tiao", () => dingque (2));
		PUtils.setBtnEvent (ques, "que_tong", () => dingque (3));
	}

	void refreshDQ(bool show = true) {
		RoomMgr rm = RoomMgr.GetInstance();
		var st = rm.getSelfSeat();

		dqItem.SetActive (show && st.que == 0);

		checkChuPai(false);
	}

	void dingque(int que) {
		NetMgr nm = NetMgr.GetInstance ();

		nm.send ("dingque", "que", que);
	}

	void updateGPS() {
		var lm = LocationMgr.GetInstance ();
		var loc = lm.Get();
		JsonObject gps = new JsonObject ();

		bool valid = loc != null && loc.valid ();
		if (valid) {
			gps.Add ("lat", loc.latitude);
			gps.Add ("lon", loc.longitude);
		}

		gps.Add ("valid", valid);
		NetMgr.GetInstance().send("update_location", gps);
	}

	void addOption(string op, int pai = 0) {
		if (op != "btn_ting")
			op = "grid/" + op;
		
		Transform tm = options.Find(op);

		if (tm == null)
			return;

		Mahjong2D mj = tm.GetComponentInChildren<Mahjong2D>();
		
		tm.gameObject.SetActive (true);

		if (mj != null)
			mj.gameObject.SetActive(pai != 0);

		if (pai == 0)
			return;

		if (mj != null) {
			mj.setScale(0.8f);
			mj.setID (pai);
		}
	}

	void hideOptions() {
		options.gameObject.SetActive (false);

		Transform grid = options.Find("grid");

		for (int i = 0; i < grid.childCount; i++) {
			Transform op = grid.GetChild (i);

			op.gameObject.SetActive (false);
		}

		options.Find ("btn_ting").gameObject.SetActive (false);

		hideChiOptions();
	}

	bool hasOptions() {
		return options.gameObject.activeSelf;
	}

	public void ShowAction() {
		RoomMgr rm = RoomMgr.GetInstance ();
		GameAction action = rm.action;

		_options = action;
		showAction(action);

		checkChuPai (rm.isMyTurn());
	}

	void showAction(GameAction act) {
		hideOptions ();

		if (act == null || !act.hasAction ())
			return;

		options.gameObject.SetActive (true);

		if (act.hu || act.gang || act.peng || act.chi || act.ting)
			addOption ("btn_guo");

		int pai = act.pai % 100;

		if (act.ting)
			addOption ("btn_ting");

		if (act.hu)
			addOption ("btn_hu", pai);

		if (act.gang) {
			for (int i = 0; i < act.gangpai.Count; i++) {
				int gp = act.gangpai[i];

				addOption ("btn_gang" + i, gp);
			}
		}

		if (act.peng)
			addOption ("btn_peng", pai);

		if (act.chi)
			addOption ("btn_chi", pai);

		options.GetComponentInChildren<UIGrid> ().Reposition ();
	}

	void Highlight(int id, bool enable) {
		foreach (DHM_CardManager cm in PlayerManager.GetInstance ().getCardManagers())
			cm.HighlightRecycle (id, enable);
	}

	public void onMJClicked(HandCardItem item) {
		if (item == null || !item.valid())
            return;

		RoomMgr rm = RoomMgr.GetInstance();

		if (_gangState == 0) {
			onMJChoosed (item);
			return;
		}

		if (!rm.isMyTurn() || shot)
			return;

		int method = PlayerPrefs.HasKey ("chupai_method") ? PlayerPrefs.GetInt ("chupai_method") : 0;

		if (0 == method) {
			shoot (item);
			shot = true;
			hidePrompt();
			return;
		}

		HandCardItem old = selected;
		GameObject ob = item.getObj();
		if (old != null && item.checkObj(old)) {
			if (_tingState != 0)
				old.choosed (false);

			Highlight(old.getId(), false);

			ob.transform.position = selPos;
			selected = null;
			selPos = Vector3.zero;

			shoot (item);
			shot = true;
			hidePrompt();
			return;
		}

		if (old != null && old.valid ()) {
			ob = old.getObj();

			int id = old.getId ();

			// NOTE: old maybe in recycle
			if (old.getLayer () == "Self") {
				ob.transform.position = selPos;

				if (_tingState != 0 && !_options.tingouts.Contains(id))
					old.choosed (false);

				Highlight(old.getId(), false);
			}

			selected = null;
		}

		ob = item.getObj();
		selPos = ob.transform.position;
		selected = item;

		ob.transform.Translate (0, 0.01f, 0);

		onMJChoosed(item);
	}

	void onMJChoosed(HandCardItem item) {
		int id = item.getId ();

		GameAction ac = _options;

		if (ac == null)
			return;

		int cnt = ac.help.Count;
		if (cnt > 0) {
			List<HuPai> hus = null;
			for (int i = 0; i < cnt; i++) {
				if (ac.help [i].pai == id) {
					hus = ac.help [i].hus;
					break;
				}
			}

			showPrompt (hus);
		}

		if (_tingState != 0)
			item.choosed();

		Highlight(id, true);
	}

	void shoot(HandCardItem item) {
		int mjid = item.getId();
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

	void onBtnGangClicked(int id) {
		var tm = options.Find("grid/btn_gang" + id);
		if (tm == null)
			return;

		var mj = tm.GetComponentInChildren<Mahjong2D>();
		if (mj == null)
			return;

		int pai = mj.getID();

		NetMgr.GetInstance ().send ("gang", "pai", pai);
		checkChuPai (false);
	}

	public void onBtnGang0Clicked() {
		onBtnGangClicked (0);
	}

	public void onBtnGang1Clicked() {
		onBtnGangClicked (1);
	}

	public void onBtnGang2Clicked() {
		onBtnGangClicked (2);
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

			PUtils.onClick(chi, ()=>{
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

		var nm = NetMgr.GetInstance ();
		GameAction ac = _options;

		if (ac.tingouts.Count == 0) {
			nm.send ("ting");
		} else {
			enterTingState (0);
		}
	}

	public void onBtnGuoClicked() {
		NetMgr.GetInstance ().send ("guo");
	}

	void showTingOpt(bool status) {
		//tingOpt.gameObject.SetActive (status);
		showQiaoHelp(status);
	}

	public void onTingCancel() {
		enterTingState (-1);

		showAction(_options);
	}

	public void updatePrompt(int pai) {
		var arr = RoomMgr.getChiArr(pai);
		var rm = RoomMgr.GetInstance();
		var seat = rm.getSelfSeat();

		if (!seat.tingpai) {
			hidePrompt();
			return;
		}

		bool found = false;
		foreach (HuPai hu in seat.hus) {
			if (hu.pai >= arr [0] && hu.pai <= arr [2]) {
				found = true;
				break;
			}
		}

		if (found) {
			rm.updateHus();
			showPrompt(seat.hus);
		}
	}

	void showPrompt(List<HuPai> hus) {
		Transform prompt = transform.Find ("prompt");
		Transform grid = prompt.Find ("grid");

		if (hus == null || hus.Count == 0) {
			prompt.gameObject.SetActive (false);
			return;
		}

		prompt.gameObject.SetActive(true);

		while (grid.childCount > 0)
			DestroyImmediate(grid.GetChild(0).gameObject);

		for (int i = 0; i < hus.Count; i++) {
			HuPai hu = hus[i];
			GameObject ob = Instantiate (Resources.Load ("Prefab/majiang/mj_prompt"), grid) as GameObject;
			Transform tm = ob.transform;

			tm.localScale = new Vector3 (0.6f, 0.6f, 1);
			tm.GetComponent<Mahjong2D> ().setID (hu.pai);
			tm.Find("score").GetComponent<UILabel>().text = hu.score >= 0 ? hu.score + "分": "";
			tm.Find("num").GetComponent<UILabel>().text = hu.num >= 0 ? hu.num + "张": "";
		}

		grid.GetComponent<UIGrid>().Reposition();
	}

	public void showPrompt() {
		RoomMgr rm = RoomMgr.GetInstance ();
		SeatInfo seat = rm.getSelfSeat ();

		if (!seat.tingpai) {
			hidePrompt ();
			return;
		}

		var hus = seat.hus;

		showPrompt(hus);
	}

	void hidePrompt() {
		Transform prompt = transform.Find("prompt");
		prompt.gameObject.SetActive(false);
	}

	void enterTingState(int state, int pai = 0) {
		NetMgr nm = NetMgr.GetInstance ();
		_tingState = state;

		switch (state) {
		case 0:
			showTingOpt (true);
			//checkTingPai ();
			checkChuPai(true);
			break;
		case 1:
			{
				GameAction ac = _options;

				List<int> tingouts = new List<int>();
				if (ac != null) {
					for (int i = 0; i < ac.help.Count; i++)
						tingouts.Add(ac.help[i].pai);
				}

				if (tingouts.Contains (pai)) {
					nm.send ("ting", "pai", pai);
				} else {
					nm.send ("guo");
					nm.send ("chupai", "pai", pai);
				}

				enterTingState (-1);
				break;
			}
		case -1:
			showTingOpt (false);
			checkChuPai (true);
			break;
		default:
			break;
		}
	}

	DHM_HandCardManager getHandCardManager() {
		return PlayerManager.GetInstance().getSelfCardManager().getHCM();
	}

	bool canTing() {
		return _options != null && _options.help.Count > 0;
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
			bool check = tingouts == null || tingouts.Contains(item.getId());
			item.setInteractable(check);
		}
	}

	void lockHandCards() {
		Debug.Log ("lock");
		DHM_HandCardManager hcm = getHandCardManager ();
		List<HandCardItem> list = new List<HandCardItem>(hcm._handCardList);

		if (hcm._MoHand != null)
			list.Add (hcm._MoHand);

		foreach (HandCardItem item in list) {
			item.setInteractable (false, false);
		}
	}

	public void checkChuPai(bool check) {
		Debug.Log ("checkChuPai: " + check.ToString());

		var rm = RoomMgr.GetInstance ();

		if (rm.state.state == "dingque")
			check = false;

		DHM_HandCardManager hcm = getHandCardManager ();
		List<HandCardItem> list = new List<HandCardItem>(hcm._handCardList);

		if (hcm._MoHand != null)
			list.Add (hcm._MoHand);

		SeatInfo seat = rm.getSelfSeat();
		bool show = check && !seat.tingpai;

		var ac = _options;
	
		List<int> tingouts = new List<int>();
		if (_tingState <= 0 && ac != null && ac.tingouts != null) {
			for (int i = 0; i < ac.tingouts.Count; i++)
				tingouts.Add(ac.tingouts[i]);
		}

		foreach (HandCardItem item in list) {
			int id = item.getId();
			bool active = show && !seat.limit.Contains(id);
			item.setInteractable(active);

			if (active && tingouts.Contains (id))
				item.getObj ().GetComponent<HandCard> ().choosed ();
		}
	}

	public void showQiaoHelp(bool status) {
		Transform Qiao = transform.Find("Qiao");

		if (!status) {
			Qiao.gameObject.SetActive (false);
			return;
		}

		Transform scroll = Qiao.Find("scroll");
		Transform grid = scroll.Find("grid");

		UIScrollView roll = scroll.GetComponent<UIScrollView>();
		UIGrid gd = grid.GetComponent<UIGrid>();

		while (grid.childCount > 0)
			DestroyImmediate(grid.GetChild(0).gameObject);

		Qiao.gameObject.SetActive(true);

		List<TingOut> help = _options.help;

		for (int i = 0; i < help.Count; i++) {
			TingOut to = help[i];

			GameObject ob = Instantiate (qiaoItem, grid) as GameObject;
			Transform chupai = ob.transform.Find ("chupai");
			chupai.localScale = new Vector3 (0.7f, 0.7f, 1);
			chupai.GetComponent<Mahjong2D> ().setID (to.pai);

			HuPai hu = to.hus [0];
			Transform hupai = ob.transform.Find ("hupai");
			hupai.localScale = new Vector3 (0.6f, 0.6f, 1);
			hupai.GetComponent<Mahjong2D> ().setID (hu.pai);
			hupai.Find("score").GetComponent<UILabel>().text = hu.score + "分";
			hupai.Find("num").GetComponent<UILabel>().text = hu.num + "张";

			for (int j = 1; j < to.hus.Count; j++) {
				hu = to.hus[j];
				GameObject pai = Instantiate (hupai.gameObject, ob.transform) as GameObject;
				Transform tm = pai.transform;
				tm.GetComponent<Mahjong2D> ().setID (hu.pai);
				tm.localScale = new Vector3 (0.6f, 0.6f, 1);
				tm.Translate (new Vector3 (0.2f * j, 0, 0));
				tm.Find("score").GetComponent<UILabel>().text = hu.score + "分";
				tm.Find("num").GetComponent<UILabel>().text = hu.num + "张";
			}
		}

		gd.Reposition();
		roll.ResetPosition();

		//Transform btn = Qiao.Find("btn_cancel");
		//btn.localPosition = new Vector3(0, 35 - 200 * (help.Count - 1), 0);
	}

	public void showChicken(int si, int key, Action cb) {
		var board = chicken.transform.Find ("board");
		var tile = board.Find ("tile");
		FrameAnim anim = board.GetComponent<FrameAnim>();

		chicken.SetActive (true);
		tile.gameObject.SetActive(false);
		anim.reset();

		anim.run (() => {
			tile.gameObject.SetActive(true);
			UISprite t = tile.GetComponent<UISprite>();

			t.spriteName = "" + key;
			UISpriteData sp = t.GetAtlasSprite();
			t.width = sp.width;
			t.height = sp.height;

			tile.localScale = new Vector3(1.8f, 1.8f, 1.0f);
			end(cb);
		});
	}

	void end(Action cb) {
		StartCoroutine(_end(cb));
	}

	IEnumerator _end(Action cb) {
		yield return new WaitForSeconds(1.0f);
		chicken.SetActive (false);
		if (cb != null)
			cb.Invoke();
	}

	void Update() {
		var now = Time.time;
		var rm = RoomMgr.GetInstance ();
		var replay = ReplayMgr.GetInstance ();

		if (!replay.isReplay() && rm.limitLocation() && now - lastGpsCheck > 60) {
			lastGpsCheck = now;
			updateGPS ();
		}
	}
}
