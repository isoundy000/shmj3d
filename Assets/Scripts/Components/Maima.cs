
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maima : MonoBehaviour {
	public Transform tmaima = null;
	public UILabel title = null;
	public UILabel score = null;

	void Awake() {
		Debug.Log ("Maima Awake");

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

			Debug.Log("game_sync, state=" + state.state);
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

		tmaima.gameObject.SetActive(true);

		title.text = act ? "请选择飞苍蝇" : "请等待飞苍蝇";
		score.text = "";

		for (int i = 0; i < mas.childCount; i++) {
			Transform board = mas.GetChild(i);
			Transform tile = board.Find("tile");
			FrameAnim anim = board.GetComponent<FrameAnim>();

			board.gameObject.SetActive(true);

			tile.gameObject.SetActive(false);
			anim.reset();

			Utils.onClick (board, () => {
				if (!act) return;

				nm.send("maima", "index", i);
			});
		}

		tmaima.gameObject.SetActive(true);
	}

	public void showResult(Action cb) {
		RoomMgr rm = RoomMgr.GetInstance();
		NetMgr nm = NetMgr.GetInstance();
		GameMaima maima = rm.overinfo.info.maima;

		bool act = false;
		Transform mas = tmaima.Find ("mas");

		tmaima.gameObject.SetActive(true);

		title.text = "请等待飞苍蝇";
		score.text = "";

		for (int i = 0; i < mas.childCount; i++) {
			Transform board = mas.GetChild(i);
			Transform tile = board.Find("tile");
			FrameAnim anim = board.GetComponent<FrameAnim>();

			board.gameObject.SetActive(true);

			tile.gameObject.SetActive(false);
			anim.reset();

			Utils.onClick (board, () => {});
		}

		int id = maima.selected;
		int mjid = maima.mas[id];
		int add = maima.scores[id];

		Transform _board = mas.GetChild(id);
		FrameAnim frame = _board.GetComponent<FrameAnim>();
		Transform _tile = _board.Find("tile");

		frame.run (() => {
			_tile.gameObject.SetActive(true);
			UISprite t = _tile.GetComponent<UISprite>();

			t.spriteName = "" + mjid;
			UISpriteData sp = t.GetAtlasSprite();
			t.width = sp.width;
			t.height = sp.height;

			score.text = "" + add;

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
