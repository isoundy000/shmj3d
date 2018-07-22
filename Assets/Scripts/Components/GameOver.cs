
//#define UNIT_TEST

using UnityEngine;
using System.Collections.Generic;

public class GameOver : MonoBehaviour {

	public GameObject btn_share;
	public GameObject btn_start;
	public GameObject btn_result;

	GameObject game_result;
	GameObject mahjong2d;

	public GameObject btn_next = null;
	public GameObject btn_prev = null;

	public UILabel progress = null;

	int index = 0;

	bool inited = false;

	void Awake() {
		mahjong2d = Resources.Load("Prefab/majiang/mahjong2d") as GameObject;
		game_result = transform.parent.Find("GameResult").gameObject;
	}

	void Start() {
		#if UNIT_TEST
		doGameOver ();
		#endif

		if (!inited) {
			showHistories ();
			inited = true;
		}
	}

	void OnEnable() {
		if (inited)
			showHistories ();
	}

	void showHistories() {
		if (btn_next != null) {
			RoomMgr rm = RoomMgr.GetInstance ();
			int cnt = rm.histories.Count;
			index = cnt > 0 ? cnt - 1 : 0;

			GameOverInfo info = rm.histories [index];
			showResults (info.results);

			updateBtns ();
		}
	}

	public void onBtnShareClicked() {
		UIButton btn = btn_share.GetComponent<UIButton>();
		btn.isEnabled = false;

		AnysdkMgr.GetInstance().shareImg(false, ()=>{
			btn.isEnabled = true;
		});
	}

	public void onBtnStartClicked() {
		NetMgr nm = NetMgr.GetInstance ();
		nm.send ("ready");
		gameObject.SetActive (false);
	}

	public void onBtnResultClicked() {
		gameObject.SetActive (false);
		game_result.SetActive (true);
		//game_result.GetComponent<GameResult>().doGameResult();
	}

	public void doGameOver() {
#if UNIT_TEST
		unittest();
#else
		RoomMgr rm = RoomMgr.GetInstance();
		GameOverInfo info = rm.overinfo;
		List<GameOverPlayerInfo> results = info.results;
		List<GameEndInfo> endinfo = info.endinfo;

		int cnt = rm.histories.Count;
		index =  cnt > 0 ? cnt - 1 : 0;

		if (btn_next != null)
			btn_next.SetActive(false);

		if (btn_prev != null)
			btn_prev.SetActive(false);

		if (results == null || results.Count == 0) {
			gameObject.SetActive (false);
			game_result.SetActive (true);
			Debug.Log ("result []");
			return;
		}

		bool over = endinfo != null && endinfo.Count > 0;

		btn_start.SetActive (!over);
		btn_result.SetActive (over);

		showResults (results);
#endif
	}

	void updateBtns() {
		RoomMgr rm = RoomMgr.GetInstance();
		int cnt = rm.histories.Count;

		btn_next.SetActive (index < cnt - 1);
		btn_prev.SetActive (index > 0);

		progress.text = (index + 1) + " / " + rm.conf.maxGames;
	}

	public void onBtnNext() {
		RoomMgr rm = RoomMgr.GetInstance();
		int cnt = rm.histories.Count;

		if (index < cnt - 1)
			index++;

		if (index >= 0 && index < cnt) {
			GameOverInfo info = rm.histories [index];
			showResults (info.results);
		}

		updateBtns ();
	}

	public void onBtnPrev() {
		RoomMgr rm = RoomMgr.GetInstance();
		int cnt = rm.histories.Count;

		if (index > 0)
			index--;

		if (index >= 0 && index < cnt) {
			GameOverInfo info = rm.histories [index];
			showResults (info.results);
		}

		updateBtns ();
	}

	public void onBtnClose() {
		gameObject.SetActive (false);
	}

	void showResults(List<GameOverPlayerInfo> results) {
		Transform seats = transform.Find ("seats");

		int index = 0;
		for (int i = 0; i < results.Count; i++, index++) {
			Transform s = seats.GetChild (i);
			s.gameObject.SetActive (true);
			initSeat (s, results[i]);
		}

		for (int i = index; i < seats.childCount; i++) {
			Transform s = seats.GetChild (i);
			s.gameObject.SetActive (false);
		}

		List<int> huSeats = new List<int> ();

		for (int i = 0; i < results.Count; i++) {
			HuInfo _hu = results[i].hu;
			if (_hu != null && _hu.hued)
				huSeats.Add(i);
		}
/*
		int id = 0;
		if (huSeats.Count == 0) {
			id = 2;
		} else if (huSeats.FindIndex (x=> x == RoomMgr.GetInstance ().seatindex) >= 0) {
			id = 0;
		} else {
			id = 1;
		}

		SpriteMgr title = transform.Find ("title").GetComponent<SpriteMgr> ();
		title.setIndex (id);
*/
		int si = RoomMgr.GetInstance().seatindex;
		bool win = huSeats.Contains(si);

		GameObject winOb = transform.Find("win").gameObject;
		GameObject loseOb = transform.Find("lose").gameObject;

		winOb.SetActive(win);
		loseOb.SetActive(!win);

		if (btn_next == null) {
			string audio = win ? "win" : "loss";
			AudioManager.GetInstance ().PlayEffectAudio (audio);
		}
	}

	void unittest() {
		GameOverPlayerInfo info = new GameOverPlayerInfo ();

		info.angangs = new List<int> ();
		info.angangs.Add (11);

		info.diangangs = new List<int> ();
		info.wangangs = new List<int> ();
		info.chis = new List<int> ();
		info.chis.Add (133);
		info.pengs = new List<int> ();

		info.button = 0;
		info.holds = new List<int> ();
		info.holds.Add (21);
		info.holds.Add (22);
		info.holds.Add (23);
		info.holds.Add (24);
		info.holds.Add (25);
		info.holds.Add (26);
		info.holds.Add (27);
		info.ma = 31;
		info.userid = 2;

		HuInfo hu = new HuInfo ();
		hu.fangpao = false;
		hu.zimo = true;
		hu.hued = true;
		hu.pai = 39;

		info.hu = hu;
		info.score = -9000;
		info.name = "小叶子";

		ResultDetail detail = new ResultDetail ();
		detail.tips = "底1花300 抢杠胡";
		info.detail = detail;


		List<GameOverPlayerInfo> results = new List<GameOverPlayerInfo> ();
		results.Add (info);
		results.Add (info);
		results.Add (info);
		results.Add (info);

		showResults (results);
	}

	Mahjong2D initMahjong(Transform seat, int id, Vector2 pos, int depth) {
		GameObject ob = Instantiate (mahjong2d);

		ob.transform.SetParent (seat);
		ob.transform.localPosition = pos;

		Mahjong2D mj = ob.GetComponent<Mahjong2D> ();
		mj.setDepth (depth);
		mj.setScale (0.5f);
		mj.setID (id);

		return mj;
	}

	void initPengGangs(Transform seat, int id, string type, int offset) {
		int y = -20;

		id = id % 100;

		for (int i = 0; i < 3; i++) {
			Mahjong2D mj = initMahjong (seat, type == "angang" ? 0 : id, new Vector2 (offset + i * 55, y), 4);
		}

		if (type != "peng")
			initMahjong (seat, id, new Vector2 (offset + 55, y + 16), 6);
	}

	void initChis(Transform seat, int id, int offset) {
		List<int> arr = RoomMgr.getChiArr (id);
		int y = -20;

		for (int i = 0; i < 3; i++)
			initMahjong (seat, arr[i], new Vector2 (offset + i * 55, y), 4);
	}

	void initSeat(Transform seat, GameOverPlayerInfo info) {
		int x = 160;
		int y = -20;

		Debug.Log ("initSeat");

		Mahjong2D[] mjs = seat.GetComponentsInChildren<Mahjong2D>();
		foreach (Mahjong2D mj in mjs)
			Destroy(mj.gameObject);

		foreach (int i in info.angangs) {
			initPengGangs (seat, i, "angang", x);
			x += 55 * 3 + 10;
		}

		foreach (int i in info.wangangs) {
			initPengGangs (seat, i, "wangang", x);
			x += 55 * 3 + 10;
		}

		foreach (int i in info.diangangs) {
			initPengGangs (seat, i, "diangang", x);
			x += 55 * 3 + 10;
		}

		foreach (int i in info.pengs) {
			initPengGangs (seat, i, "peng", x);
			x += 55 * 3 + 10;
		}

		foreach (int i in info.chis) {
			initChis (seat, i, x);
			x += 55 * 3 + 10;
		}

		ResultDetail detail = info.detail;
		UILabel tips = seat.Find ("tips").GetComponent<UILabel> ();
		tips.text = detail == null ? "" : detail.tips;
		//tips.transform.localPosition = new Vector3 (x - 27, 50, 0);

		List<int> holds = info.holds;
		holds.Sort ();

		for (int i = 0; i < holds.Count; i++) {
			initMahjong (seat, holds [i], new Vector2 (x, y), 4);

			x += 55;
		}

		HuInfo hu = info.hu;
		bool hued = false;
		if (hu != null && hu.hued) {
			hued = true;
			x += 80;
			initMahjong (seat, hu.pai, new Vector2 (x, y), 4);
			x += 55;
		}

		int ma = info.ma;
		seat.Find ("lbl").gameObject.SetActive(ma > 0);
		seat.Find ("ma").gameObject.SetActive(ma > 0);
		if (ma > 0) {
			UILabel lbl = seat.Find ("ma").GetComponent<UILabel> ();
			lbl.text = "+" + ma;
		}

		int chicken = info.chicken;
		if (chicken > 0) {
			x += 160;
			initMahjong (seat, chicken, new Vector2 (x, y), 4);
			x += 55;
		}

		SpriteMgr huinfo = seat.Find("huinfo").GetComponent<SpriteMgr>();

		int huid = -1;
		if (hu != null) {
			if (hu.zimo)
				huid = 1; // TODO
			else if (hu.hued)
				huid = 1;
			else if (hu.fangpao)
				huid = 0;
		}

		Debug.Log ("huid=" + huid);
		huinfo.setIndex (huid);
		//huinfo.transform.localPosition = new Vector2 (1142, 0);

		string score = info.score >= 0 ? "+" + info.score : "" + info.score;
		seat.Find("score").GetComponent<UILabel>().text = score;
#if !UNIT_TEST
		seat.Find ("bghead/icon").GetComponent<IconLoader>().setUserID (info.userid);
#endif
		seat.Find("name").GetComponent<UILabel>().text = info.name;
	}
}

