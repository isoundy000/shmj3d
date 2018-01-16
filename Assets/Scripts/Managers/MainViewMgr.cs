
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ScoreModel {};
public class RoomModel {};

[Serializable]
public class VoiceMsg {
	public int time;
	public string msg;
}

[Serializable]
public class VoiceMsgPush {
	public int sender;
	public VoiceMsg content;
}

public class MainViewMgr : MonoBehaviour {
    public static MainViewMgr m_Instance = null;

	public int mSeatIndex;

	public Transform prepare;

	public UILabel roomid;
	public UILabel gamenum;
	public UILabel mjcnt;

	public GameObject game_over;
	public GameObject game_result;

	List<Seat> seats = new List<Seat>();
	List<GameSeat> gseats = new List<GameSeat>();

    void Awake() {
        m_Instance = this;

		Debug.Log ("load dissolve");
		gameObject.AddComponent<Dissolve>();

		InitView ();
		InitEventHandlers ();
    }

	public static MainViewMgr GetInstance() {
		return m_Instance;
	}

	void Start() {
		RoomMgr rm = RoomMgr.GetInstance ();
		NetMgr nm = NetMgr.GetInstance ();
		bool isIdle = 0 == rm.info.numofgames;

		if (!isIdle)
			nm.send("ready");
    }

	void enablePrepare(bool status) {
		prepare.gameObject.SetActive (status);
	}

    void InitView() {
		RoomMgr rm = RoomMgr.GetInstance ();

		// 1. set roomid

		// 2. set seat index
		mSeatIndex = rm.seatindex;
		Debug.LogWarning("我的座位是：" + mSeatIndex);

		// 3. set player info

		// 4. set button event handler

		UIButton btnReady = prepare.Find ("actions/btnReady").GetComponent<UIButton> ();
		btnReady.onClick.Add (new EventDelegate(this, "onBtnReadyClicked"));

		UIButton btnInvite = prepare.Find ("actions/btnInvite").GetComponent<UIButton> ();
		btnInvite.onClick.Add (new EventDelegate(this, "onBtnInviteClicked"));

		//AudioManager.Instance.PlayEffectAudio("ui_click");

		UIButton btnExit = transform.Find ("Popup/btn_exit").GetComponent<UIButton> ();
		EventDelegate.Add (btnExit.onClick, this.onBtnExitClicked);

		roomid.text = rm.info.roomid;
		gamenum.text = "第" + rm.info.numofgames + "局(" + rm.conf.maxGames + ")";

		Transform Seats = transform.Find ("Seats");
		for (int i = 0; i < Seats.childCount; i++)
			seats.Add(Seats.GetChild(i).GetComponent<Seat>());

		Transform gs = transform.Find ("Game/seats");
		for (int i = 0; i < gs.childCount; i++)
			gseats.Add(gs.GetChild(i).GetComponent<GameSeat>());

		InitSeats ();
    }

	void onBtnInviteClicked() {
		
	}

	void onBtnReadyClicked() {
		Debug.Log ("onBtnReadyClicked");
		NetMgr.GetInstance ().send ("ready");
	}

	void onBtnExitClicked() {
		RoomMgr rm = RoomMgr.GetInstance ();
		NetMgr nm = NetMgr.GetInstance ();
		bool isIdle = rm.isIdle ();
		bool isOwner = rm.isOwner ();

		if (isIdle) {
			if (isOwner) {
/*
				cc.vv.alert.show('牌局还未开始，房主解散房间，房卡退还', function() {
					net.send("dispress");
				}, true);
*/
				nm.send ("dispress");
			} else {
				nm.send("exit");
			}
		} else {
			nm.send("dissolve_request");
		}
	}

	void refreshBtns() {
		RoomMgr rm = RoomMgr.GetInstance ();
		NetMgr nm = NetMgr.GetInstance ();
		bool isIdle = rm.isIdle ();
		PlayerInfo player = rm.getSelfPlayer ();

		Transform actions = prepare.Find ("actions");
		GameObject btnReady = actions.Find ("btnReady").gameObject;
		GameObject waiting = prepare.Find ("waiting").gameObject;

		waiting.SetActive (isIdle);
		actions.gameObject.SetActive (isIdle);
		btnReady.SetActive (!player.ready);
	}

	void InitEventHandlers() {
		GameMgr gm = GameMgr.GetInstance ();
		RoomMgr rm = RoomMgr.GetInstance ();

		gm.AddHandler ("mj_count", data => {
			mjcnt.text = rm.state.numofmj + "张";
		});

		gm.AddHandler ("game_num", data => {
			gamenum.text = "第" + rm.info.numofgames + "局(" + rm.conf.maxGames + ")";
		});

		gm.AddHandler("new_user", data=>{
			InitSingleSeat((int)data);
		});

		gm.AddHandler("user_state_changed", data=>{
			refreshBtns();
			InitSingleSeat((int)data);
		});

		gm.AddHandler("game_begin", data=>{
			enablePrepare(false);
			refreshBtns();
			InitSeats();
		});

		gm.AddHandler("game_sync", data=>{
			enablePrepare(false);
			refreshBtns();
			InitSeats();
		});

		gm.AddHandler ("game_over", data => {
			GameOverInfo overinfo = RoomMgr.GetInstance ().overinfo;
			GameMaima info = overinfo.info.maima;

			if (info.mas != null && info.mas.Count > 0) {
				Maima maima = transform.GetComponent<Maima>();
				maima.showResult(()=>{
					doGameOver();
				});
			} else {
				doGameOverTimeout();
			}
		});

		gm.AddHandler("game_num", data=>{
			refreshBtns();
		});

		gm.AddHandler("game_state", data=>{
			refreshBtns();
			InitSeats();
		});

		gm.AddHandler("ting_notify", data=>{
			InitSingleSeat((int)data);
		});

		gm.AddHandler("user_ready", data=>{
			InitSingleSeat((int)data);
		});

		gm.AddHandler("quick_chat_push", data=>{

		});

		gm.AddHandler("emoji_push", data=>{

		});

		gm.AddHandler("demoji_push", data=>{

		});
	}

	void doGameOverTimeout() {
		StartCoroutine(_doGameOver());
	}

	IEnumerator _doGameOver() {
		yield return new WaitForSeconds(3.0f);
		doGameOver();
	}

	void doGameOver() {
		game_over.SetActive (true);
		game_over.GetComponent<GameOver>().doGameOver();
	}

	void InitSeats() {
		RoomMgr rm = RoomMgr.GetInstance ();

		for (int i = 0; i < rm.players.Count; i++) {
			PlayerInfo p = rm.players[i];
			SeatInfo s = rm.seats[i];

			InitSingleSeat(p, s);
		}
	}

	void InitSingleSeat(int si) {
		RoomMgr rm = RoomMgr.GetInstance ();

		InitSingleSeat (rm.players [si], rm.seats [si]);
	}

	void InitSingleSeat(PlayerInfo player, SeatInfo seat) {
		RoomMgr rm = RoomMgr.GetInstance ();
		int si = player.seatindex;
		int local = rm.getLocalIndex(si);

		Seat s = seats[local];

		if (player.userid <= 0) {
			s.reset ();
			s.gameObject.SetActive (false);
			return;
		}

		s.gameObject.SetActive (true);
		s.setInfo (player.userid, player.name, player.score);
		s.setOffline (!player.online);
		s.setButton (rm.state.button == si);
		s.setReady (rm.state.state == "" ? player.ready : false);
	}

	static bool show = false;

	public void onBtnChat() {
		//GameAlert.GetInstance().show("测试");
/*
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (0);
		//cm._pengGangMgr.CreatePengHand ();
		//cm._handCardMgr.HuPai(21);

		if (show) {
			cm.ActiveChuPaiState (true);
		} else {
			cm.HideChuPaiState ();
		}

		show = !show;
*/
		//GameManager.GetInstance ().PlaySaiZi (0, new int[] { 1, 6 });
/*
		VoiceMgr vm = VoiceMgr.GetInstance ();
		if (!show) {
			Debug.Log ("start record");
			vm.startRecord ();
		} else {
			Debug.Log ("stop record");
			vm.stopRecord ();
		}

		show = !show;
*/
		transform.Find("Chat").gameObject.SetActive(true);
	}

	public void showAction(int si, string act, int card = 0) {
		RoomMgr rm = RoomMgr.GetInstance ();
		int local = rm.getLocalIndex(si);

		GameSeat gs = gseats[local];
		gs.showAction (act, card);
	}

	public void updateFlowers(int si) {
		RoomMgr rm = RoomMgr.GetInstance ();
		int cnt = rm.seats[si].flowers.Count;
		int local = rm.getLocalIndex(si);
		GameSeat gs = gseats[local];

		gs.transform.Find("num").GetComponent<UILabel>().text = "" + cnt;
	}
}
