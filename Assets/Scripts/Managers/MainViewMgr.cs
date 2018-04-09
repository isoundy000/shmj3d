
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
	public GameObject user_panel;

	public Transform demojis;

	List<GameObject> seats = new List<GameObject>();
	List<GameObject> gseats = new List<GameObject>();

    void Awake() {
        m_Instance = this;

		gameObject.AddComponent<Dissolve>();

		InitView ();
    }

	public static MainViewMgr GetInstance() {
		return m_Instance;
	}

	void Start() {
		RoomMgr rm = RoomMgr.GetInstance();

		Transform Seats = transform.Find ("Seats");
		for (int i = 0; i < Seats.childCount; i++) {
			Transform s = Seats.GetChild (i);
			seats.Add (s.gameObject);

			Transform icon = s.Find("bghead");
			int j = rm.getSeatIndexByLocal(i);
			Utils.onClick (icon, () => {
				PlayerInfo p = RoomMgr.GetInstance().players[j];
				GetComponent<UserPanel>().show(p.userid);
			});
		}

		Transform gs = transform.Find ("Game/seats");
		for (int i = 0; i < gs.childCount; i++)
			gseats.Add(gs.GetChild(i).gameObject);

		InitSeats ();
		InitEventHandlers ();

		bool replay = ReplayMgr.GetInstance().isReplay();

		GameObject entries = transform.Find ("Entries").gameObject;
		entries.SetActive(!replay);

		prepare.gameObject.SetActive(!replay);

		if (replay)
			return;

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

		roomid.text = rm.info.roomid;
		//gamenum.text = "第" + rm.info.numofgames + "局(" + rm.conf.maxGames + ")";
		gamenum.text = rm.info.numofgames + "/" + rm.conf.maxGames + "局";
    }

	void onBtnInviteClicked() {
		RoomMgr rm = RoomMgr.GetInstance ();
		string title = "<雀达麻友圈> - 房间分享";
		string content = "房号:" + rm.info.roomid + " 玩法:" + rm.getWanfa();
		Dictionary<string, object> args = new Dictionary<string, object>();
		args.Add("room", rm.info.roomid);

		AnysdkMgr.GetInstance ().share(title, content, args);
	}

	void onBtnReadyClicked() {
		Debug.Log ("onBtnReadyClicked");
		NetMgr.GetInstance ().send ("ready");
	}

	void refreshBtns() {
		RoomMgr rm = RoomMgr.GetInstance ();
		NetMgr nm = NetMgr.GetInstance ();
		bool isIdle = rm.isIdle ();
		PlayerInfo player = rm.getSelfPlayer ();

		bool replay = ReplayMgr.GetInstance().isReplay();

		Transform actions = prepare.Find ("actions");
		GameObject btnReady = actions.Find ("btnReady").gameObject;
		GameObject waiting = prepare.Find ("waiting").gameObject;

		waiting.SetActive (!replay && isIdle);
		actions.gameObject.SetActive (!replay && isIdle);
		btnReady.SetActive (!replay && !player.ready);

		if (actions.gameObject.activeSelf)
			actions.GetComponent<UIGrid>().Reposition();
	}

	void InitEventHandlers() {
		GameMgr gm = GameMgr.GetInstance ();
		RoomMgr rm = RoomMgr.GetInstance ();

		gm.AddHandler ("mj_count", data => {
			mjcnt.text = rm.state.numofmj + "张";
		});

		gm.AddHandler ("game_num", data => {
			//gamenum.text = "第" + rm.info.numofgames + "局(" + rm.conf.maxGames + ")";
			gamenum.text = rm.info.numofgames + "/" + rm.conf.maxGames + "局";
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
			mjcnt.text = rm.state.numofmj + "张";
			//gamenum.text = "第" + rm.info.numofgames + "局(" + rm.conf.maxGames + ")";
			gamenum.text = rm.info.numofgames + "/" + rm.conf.maxGames + "局";
		});

		gm.AddHandler ("hupai", data => {
			HuPushInfo info = (HuPushInfo)data;
			InitSingleSeat(info.seatindex);
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

		gm.AddHandler("chat", data=>{
			ChatInfo info = (ChatInfo)data;

			chat(info.sender, info.content);
		});

		gm.AddHandler ("voice_msg", data => {
			voice((VoiceMsgPush)data);
		});

		gm.AddHandler("quick_chat_push", data=>{

		});

		gm.AddHandler("emoji_push", data=>{
			EmojiPush info = (EmojiPush)data;

			emoji(info.sender, info.content + 1);
		});

		gm.AddHandler("demoji_push", data=>{
			DEmojiPush info = (DEmojiPush)data;

			demoji(info.sender, info.target, info.id);
		});
	}

	void demoji(int sender, int target, int id) {
		RoomMgr rm = RoomMgr.GetInstance();
		int locals = rm.getLocalIndexByID(sender);
		int localt = rm.getLocalIndexByID(target);

		string[] anims = new string[]{ "gun" };
		if (id >= anims.Length)
			return;

		string path = "Prefab/anim/" + anims[id] + "_" + locals + "_" + localt;

		UnityEngine.Object ob = Resources.Load (path);
		if (ob == null)
			return;

		GameObject obj = Instantiate(ob, demojis) as GameObject;
	}

	void emoji(int sender, int id) {
		RoomMgr rm = RoomMgr.GetInstance();
		int local = rm.getLocalIndexByID(sender);

		Seat s = seats [local].GetComponent<Seat>();
		s.emoji (id);
	}

	void voice(VoiceMsgPush vmp) {
		RoomMgr rm = RoomMgr.GetInstance();
		int local = rm.getLocalIndexByID(vmp.sender);

		Seat s = seats [local].GetComponent<Seat>();
		s.voice((float)vmp.content.time / 1000);
	}

	void chat(int sender, string content) {
		RoomMgr rm = RoomMgr.GetInstance();
		int local = rm.getLocalIndexByID(sender);

		Seat s = seats [local].GetComponent<Seat>();
		s.chat(content);
	}

    public void GameOver() {
        GameOverInfo overinfo = RoomMgr.GetInstance ().overinfo;
		GameMaima info = overinfo.info.maima;

        bool ma = info.mas != null && info.mas.Count > 0;

		if (ma) {
			Maima maima = transform.GetComponent<Maima>();
			maima.showResult(()=>{
				doGameOver();
			});
		} else {
			doGameOver();
		}
    }

	void doGameOver() {
		game_over.SetActive (true);
		game_over.GetComponent<GameOver>().doGameOver();
	}

	void InitSeats() {
		RoomMgr rm = RoomMgr.GetInstance ();

		foreach (GameObject s in seats)
			s.SetActive(false);

		foreach (GameObject gs in gseats)
			gs.SetActive(false);

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
		GameObject gs = gseats[local];
		bool isIdle = rm.isIdle ();

		Seat s = seats[local].GetComponent<Seat>();

		if (player.userid <= 0) {
			s.reset ();
			s.gameObject.SetActive (false);
			gs.SetActive (false);
			return;
		}

		s.gameObject.SetActive (true);	
		s.setInfo (player.userid, player.name, player.score);
		s.setOffline (!player.online);
		s.setButton (rm.state.button == si);
		s.setReady (rm.state.state == "" ? player.ready : false);
		s.setTing (seat.tingpai);
		s.setHu (seat.hued);

		gs.SetActive(!isIdle);
	}

	static bool show = false;
	static int id = 11;
	public void onBtnChat() {
		//GameAlert.GetInstance().show("测试");

		//DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (0);

		DHM_CardManager cm = GameObject.Find ("EastPlayer").GetComponent<DHM_CardManager> ();
		PengGangManager pg = cm._pengGangMgr;

		cm._handCardMgr.FaPai();
		//cm._handCardMgr ();
		//gseats[0].GetComponent<GameSeat>().showAction ("peng", 11);
		//gseats[1].GetComponent<GameSeat>().showAction ("chi", 11);
		//gseats[2].GetComponent<GameSeat>().showAction ("gang", 11);
		//gseats[3].GetComponent<GameSeat>().showAction ("qiao", 11);

		//InteractMgr.GetInstance().showQiaoHelp();

		/*
		List<int> tings = new List<int>();

		tings.Add (22);
		tings.Add (33);
		tings.Add (44);

		InteractMgr.GetInstance ().showPrompt (tings);
		*/

		//Transform tm = cm._handCardMgr._HandCardPlace.transform;
		/*
		tm.Translate(0, 0.0225f, 0);
		tm.Rotate(90, 0, 0);

		tm.Translate(0, 0.05f, 0.0225f);
		tm.Rotate(-180, 0, 0);
		*/
		//tm.Translate (0, 0, 0.05f);
		//tm.Rotate (-90, 0, 0);

		//GameManager.GetInstance ().SwitchTo (2);

		//pg.Peng(134);
		//pg.Peng (341);
		//pg.CreateWanGangCard (341);

		//pg.Peng (141);
		//cm._pengGangMgr.Chi (13);
		//
//		cm._pengGangMgr.Gang (312, false);
//		cm._pengGangMgr.Chi (33);
//		cm._pengGangMgr.Peng (327);

		//cm._pengGangMgr.Chi (11);
		//cm._pengGangMgr.Peng (327);

		//cm._handCardMgr.HuPai(21);

/*
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
//		transform.Find("Chat").gameObject.SetActive(true);
	}

	public void onBtnHistories() {
		GameObject ob = transform.Find("histories").gameObject;

		RoomMgr rm = RoomMgr.GetInstance ();
		int cnt = rm.histories.Count;
		if (cnt > 0)
			ob.SetActive (true);
	}

	public void showAction(int si, string act, int card = 0) {
		RoomMgr rm = RoomMgr.GetInstance ();
		int local = rm.getLocalIndex(si);

		GameSeat gs = gseats[local].GetComponent<GameSeat>();
		Debug.Log ("showAction si=" + si + " act=" + act);
		gs.showAction (act, card);
	}

	public void switchTo(int si) {
		RoomMgr rm = RoomMgr.GetInstance ();
		int local = -1;

		if (si >= 0 && si < seats.Count)
			local = rm.getLocalIndex(si);

		for (int i = 0; i < seats.Count; i++) {
			Seat s = seats[i].GetComponent<Seat>();
			s.setFire(i == local);
		}
	}

	public void updateFlowers(int si) {
		RoomMgr rm = RoomMgr.GetInstance ();
		int cnt = rm.seats[si].flowers.Count;
		int local = rm.getLocalIndex(si);
		GameObject gs = gseats[local];

		gs.SetActive(true);
		gs.transform.Find("flower/num").GetComponent<UILabel>().text = "" + cnt;
	}
}
