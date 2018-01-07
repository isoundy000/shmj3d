
//#define UNIT_TEST

using UnityEngine;
using System.Collections.Generic;

public class GameOver : MonoBehaviour {

	public GameObject btn_share;
	public GameObject btn_start;
	public GameObject btn_result;

	public GameObject game_result;

	public GameObject mahjong2d;

	public void onBtnShareClicked() {
		// TODO
	}

	public void onBtnStartClicked() {
		NetMgr nm = NetMgr.GetInstance ();
		nm.send ("ready");
		gameObject.SetActive (false);
	}

	public void onBtnResultClicked() {
		gameObject.SetActive (false);
		game_result.SetActive (true);
		game_result.GetComponent<GameResult>().doGameResult();
	}

	public void doGameOver() {
#if UNIT_TEST
		unittest();
#else
		GameOverInfo info = RoomMgr.GetInstance ().overinfo;
		List<GameOverPlayerInfo> results = info.results;
		List<GameEndInfo> endinfo = info.endinfo;

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

	void showResults(List<GameOverPlayerInfo> results) {
		Transform seats = transform.FindChild ("seats");

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

		int id = 0;
		if (huSeats.Count == 0) {
			id = 2;
		} else if (huSeats.FindIndex (x=> x == RoomMgr.GetInstance ().seatindex) >= 0) {
			id = 0;
		} else {
			id = 1;
		}

		SpriteMgr title = transform.FindChild ("title").GetComponent<SpriteMgr> ();
		title.setIndex (id);
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
		info.name = "小叶子小叶子小叶子";

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
		mj.setDepth (4);
		mj.setScale (0.5f);
		mj.setID (id);

		return mj;
	}

	void initPengGangs(Transform seat, int id, string type, int offset) {
		int y = -10;

		id = id % 100;

		for (int i = 0; i < 3; i++) {
			Mahjong2D mj = initMahjong (seat, type == "angang" ? 0 : id, new Vector2 (offset + i * 55, y), 4);
		}

		if (type != "peng")
			initMahjong (seat, id, new Vector2 (offset + 55, y + 16), 5);
	}

	void initChis(Transform seat, int id, int offset) {
		List<int> arr = RoomMgr.getChiArr (id);
		int y = -10;

		for (int i = 0; i < 3; i++)
			initMahjong (seat, arr[i], new Vector2 (offset + i * 55, y), 4);
	}

	void initSeat(Transform seat, GameOverPlayerInfo info) {
		int x = 100;
		int y = -10;

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
		UILabel tips = seat.FindChild ("tips").GetComponent<UILabel> ();
		tips.text = detail == null ? "" : detail.tips;
		tips.transform.localPosition = new Vector3 (x - 27, 50, 0);

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
			x += 10;
			initMahjong (seat, hu.pai, new Vector2 (x, y), 4);
			x += 55;
		}

		int ma = info.ma;
		if (ma > 0) {
			x += hued ? 30 : 95;
			initMahjong (seat, ma, new Vector2 (x, y), 4);
		}

		SpriteMgr huinfo = seat.FindChild("huinfo").GetComponent<SpriteMgr>();

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
		huinfo.transform.localPosition = new Vector2 (1142, 0);

		string score = info.score >= 0 ? "+" + info.score : "" + info.score;
		seat.FindChild("score").GetComponent<UILabel>().text = score;
#if !UNIT_TEST
		seat.FindChild ("bghead/icon").GetComponent<IconLoader>().setUserID (info.userid);
#endif
		seat.FindChild("name").GetComponent<UILabel>().text = info.name;
	}
}

