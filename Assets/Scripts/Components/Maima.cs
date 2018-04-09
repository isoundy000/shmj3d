
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maima : MonoBehaviour {
	public Transform tmaima = null;
	public UILabel title = null;
	public UILabel score = null;

	void Awake() {

		GameMgr gm = GameMgr.GetInstance();
		RoomMgr rm = RoomMgr.GetInstance();

		gm.AddHandler ("game_wait_maima", data => {
			showWait();
		});

		gm.AddHandler ("game_maima", data => {
			//showResult();
		});

		gm.AddHandler ("game_sync", data => {
			GameState state = rm.state;

			if (state.state != "maima")
				return;

			showWait();
		});
	}

	void showWait() {
		RoomMgr rm = RoomMgr.GetInstance();
		NetMgr nm = NetMgr.GetInstance();
		GameMaima maima = rm.state.maima;

		bool act = maima.seatindex == rm.seatindex;
		Transform mas = tmaima.Find ("mas");
		UIGrid grid = mas.GetComponent<UIGrid>();

		tmaima.gameObject.SetActive(true);

		title.text = act ? "请选择飞苍蝇" : "请等待飞苍蝇";
		score.text = "";

		int count = maima.mas.Count;
		int i = 0;

		for (i = 0; i < mas.childCount && i < count; i++) {
			Transform board = mas.GetChild(i);
			Transform tile = board.Find("tile");
			FrameAnim anim = board.GetComponent<FrameAnim>();

			board.gameObject.SetActive(true);

			tile.gameObject.SetActive(false);
			anim.reset();

			GameObject _fire = board.Find ("fire").gameObject;
			_fire.SetActive (false);

			int j = i;

			Utils.onClick (board, () => {
				if (!act) return;

				nm.send("maima", "index", j);
			});
		}

		for (int j = i; j < mas.childCount; j++) {
			Transform board = mas.GetChild(j);
			board.gameObject.SetActive(false);
		}

		grid.Reposition();
	}

	void Start() {
		#if false
		RoomMgr rm = RoomMgr.GetInstance ();

		rm.overinfo = new GameOverInfo ();
		rm.overinfo.info = new GameEndFlags ();
		GameMaima maima = new GameMaima ();

		rm.overinfo.info.maima = maima;

		maima.mas = new List<int> () { 11, 23, 45, 36 };
		maima.scores = new List<int> () { 1, 3, 5, 6 };
		maima.seatindex = 1;
		maima.selected = 2;

		showResult(null);
		#endif
	}

	public void showResult(Action cb) {
		RoomMgr rm = RoomMgr.GetInstance();
		NetMgr nm = NetMgr.GetInstance();
		GameMaima maima = rm.overinfo.info.maima;

		bool act = false;
		Transform mas = tmaima.Find ("mas");
		UIGrid grid = mas.GetComponent<UIGrid>();

		tmaima.gameObject.SetActive(true);

		title.text = "请等待飞苍蝇";
		score.text = "";

		int id = maima.selected;
		int i = 0;
		int count = maima.mas.Count;

		for (i = 0; i < mas.childCount && i < count; i++) {
			Transform board = mas.GetChild(i);
			Transform tile = board.Find("tile");
			FrameAnim anim = board.GetComponent<FrameAnim>();

			board.gameObject.SetActive(true);
			tile.gameObject.SetActive(false);
			anim.reset();

			Utils.onClick (board, () => {});

			if (i == id)
				continue;

			int _mjid = maima.mas[i];
			GameObject _fire = board.Find ("fire").gameObject;
			_fire.SetActive (false);

			anim.run (() => {
				tile.gameObject.SetActive(true);
				UISprite t = tile.GetComponent<UISprite>();
				t.spriteName = "" + _mjid;

				UISpriteData sp = t.GetAtlasSprite();
				t.width = sp.width;
				t.height = sp.height;
			});
		}

		for (int j = i; j < mas.childCount; j++) {
			Transform board = mas.GetChild(j);
			board.gameObject.SetActive(false);
		}

		grid.Reposition();

		int mjid = maima.mas[id];
		int add = maima.scores[id];

		Transform _board = mas.GetChild(id);
		FrameAnim frame = _board.GetComponent<FrameAnim>();
		Transform _tile = _board.Find("tile");
		GameObject fire = _board.Find ("fire").gameObject;

		fire.SetActive (true);

		frame.run (() => {
			_tile.gameObject.SetActive(true);
			UISprite t = _tile.GetComponent<UISprite>();

			t.spriteName = "" + mjid;
			UISpriteData sp = t.GetAtlasSprite();
			t.width = sp.width;
			t.height = sp.height;

			_tile.localScale = new Vector3(1.8f, 1.8f, 1.0f);

			score.text = add > 0 ? "+" + add : "" + add;

			end(cb);
		});
	}

	void end(Action cb) {
		StartCoroutine(_end(cb));
	}

	IEnumerator _end(Action cb) {
		yield return new WaitForSeconds(3.0f);
		tmaima.gameObject.SetActive(false);
		if (cb != null)
			cb.Invoke();
	}
}
