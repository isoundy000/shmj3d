
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DG.Tweening;

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
	public UILabel wanfa;
	public UILabel version;

	public GameObject game_over;
	public GameObject game_result;
	public GameObject user_panel;

	Transform demojis;

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
			PUtils.onClick (icon, () => {
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

		var en = transform.Find ("Entries");
		en.gameObject.SetActive(!replay);

		prepare.gameObject.SetActive(!replay);

		if (replay)
			return;

		PUtils.setActive(en, "btn_location", rm.conf.limit_gps);

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

		// 1. set border
		Transform bd = transform.Find("border");
		bd.GetComponent<UIWidget> ().SetAnchor (GameObject.Find("UI Root"));

		// 2. set seat index
		mSeatIndex = rm.seatindex;
		Debug.LogWarning("我的座位是：" + mSeatIndex);

		// 3. set player info

		// 4. set button event handler
		PUtils.setBtnEvent (prepare, "actions/btnReady", onBtnReadyClicked);
		PUtils.setBtnEvent (prepare, "actions/btnInvite", onBtnInviteClicked);
		PUtils.setBtnEvent (prepare, "btnCopy", onBtnCopy);

		roomid.text = rm.info.roomid;
		//gamenum.text = "第" + rm.info.numofgames + "局(" + rm.conf.maxGames + ")";
		gamenum.text = "[D82828]" + rm.info.numofgames + "[FFFFFF]/" + rm.conf.maxGames + "局";
		wanfa.text = rm.getWanfa ();
		version.text = "V" + GameSettings.Instance.version;

		demojis = GameObject.Find("demojis").transform;
    }

	void onBtnInviteClicked() {
		RoomMgr rm = RoomMgr.GetInstance ();
		string title = "<" + GameSettings.Instance.appname + "> - 房间分享";
		string content = "房号:" + rm.info.roomid + " 玩法:" + rm.getWanfa();
		Dictionary<string, object> args = new Dictionary<string, object>();
		args.Add("room", rm.info.roomid);

		AnysdkMgr.GetInstance ().share(title, content, args);
	}

	void onBtnReadyClicked() {
		Debug.Log ("onBtnReadyClicked");
		NetMgr.GetInstance ().send ("ready");
	}

	void onBtnCopy() {
		RoomMgr rm = RoomMgr.GetInstance ();

		string title = "<" + GameSettings.Instance.appname + "> - 房间分享";
		string content = "房号:" + rm.info.roomid + " 玩法:" + rm.getWanfa();
		//string tips = "复制本消息直接进入房间";

		AnysdkMgr.setClipBoard (title + "\n" + content);
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

		PUtils.setActive(prepare, "btnCopy", !rm.isClubRoom());

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
			if (this != null)
				mjcnt.text = rm.state.numofmj + "张";
		});

		gm.AddHandler ("game_num", data => {
			if (this != null)
				gamenum.text = "[D82828]" + rm.info.numofgames + "[FFFFFF]/" + rm.conf.maxGames + "局";
		});

		gm.AddHandler("new_user", data=>{
			if (this != null)
				InitSingleSeat((int)data);
		});

		gm.AddHandler("user_state_changed", data => {
			if (this != null) {
				refreshBtns();
				InitSingleSeat((int)data);
			}
		});

		gm.AddHandler("game_begin", data=>{
			if (this != null) {
				enablePrepare(false);
				hideChupai();
				refreshBtns();
				InitSeats();
			}
		});

		gm.AddHandler("game_sync", data=>{
			if (this != null) {
				enablePrepare(false);
				refreshBtns();
				InitSeats();
				mjcnt.text = rm.state.numofmj + "张";
				//gamenum.text = "第" + rm.info.numofgames + "局(" + rm.conf.maxGames + ")";
				gamenum.text = "[D82828]" + rm.info.numofgames + "[FFFFFF]/" + rm.conf.maxGames + "局";
			}
		});

		gm.AddHandler ("hupai", data => {
			HuPushInfo info = (HuPushInfo)data;

			if (this != null)
				InitSingleSeat(info.seatindex);
		});

		gm.AddHandler("game_num", data=>{
			if (this != null)
				refreshBtns();
		});

		gm.AddHandler("game_state", data=>{
			if (this != null) {
				refreshBtns();
				InitSeats();
			}
		});

		gm.AddHandler("ting_notify", data=>{
			if (this != null)
				InitSingleSeat((int)data);
		});

		gm.AddHandler("user_ready", data=>{
			if (this != null)
				InitSingleSeat((int)data);
		});

		gm.AddHandler ("game_dingque", data => {
			if (this != null)
				InitSeats();
		});

		gm.AddHandler("chat", data=>{
			ChatInfo info = (ChatInfo)data;

			if (this != null)
				chat(info.sender, info.content);
		});

		gm.AddHandler ("voice_msg", data => {
			if (this != null)
				voice((VoiceMsgPush)data);
		});

		gm.AddHandler("quick_chat_push", data => {
			QuickChatInfo info = (QuickChatInfo)data;
			if (this != null)
				quickchat(info.sender, info.content);
		});

		gm.AddHandler("emoji_push", data=>{
			EmojiPush info = (EmojiPush)data;

			if (this != null)
				emoji(info.sender, info.content + 1);
		});

		gm.AddHandler("demoji_push", data=>{
			DEmojiPush info = (DEmojiPush)data;

			if (this != null)
				demoji(info.sender, info.target, info.id);
		});
	}

	void quickchat(int sender, int id) {
		RoomMgr rm = RoomMgr.GetInstance();
		int local = rm.getLocalIndexByID(sender);
		int si = rm.getSeatIndexByID(sender);

		Seat s = seats [local].GetComponent<Seat>();

		Chat chat = GetComponent<Chat>();
		QuickChat qc = chat.getQuickChat(id);

		if (qc != null) {
			s.chat (qc.text);
			AudioManager.GetInstance().PlayQuickChat(si, qc.audio);
		}
	}

	void demoji(int sender, int target, int id) {
		RoomMgr rm = RoomMgr.GetInstance();
		int locals = rm.getLocalIndexByID(sender);
		int localt = rm.getLocalIndexByID(target);

		if (sender == target)
			demoji_own(localt, id);
		else if (id < 1)
			demoji_spec(locals, localt, id);
		else
			demoji_normal(locals, localt, id);
	}

	void demoji_own(int target, int id) {
		id -= 100;

		string[] anims = new string[]{ "fozu" };
		if (id < 0 || id >= anims.Length)
			return;

		string path = "Prefab/anim/" + anims[id] + "_d";
		var dseat = demojis.GetChild (target);

		GameObject dob = Instantiate(Resources.Load(path), dseat) as GameObject;
	}

	void demoji_spec(int sender, int target, int id) {
		string[] anims = new string[]{ "gun" };
		if (id >= anims.Length)
			return;

		string path = "Prefab/anim/" + anims[id] + "_" + sender + "_" + target;

		UnityEngine.Object ob = Resources.Load (path);
		if (ob == null)
			return;

		GameObject obj = Instantiate(ob, demojis) as GameObject;
	}

	void demoji_normal(int sender, int target, int id) {
		string[] anims = new string[]{ "meigui", "beer", "kiss", "egg", "gift", "shit"};

		if (id - 1 >= anims.Length)
			return;

		string spath = "Prefab/anim/" + anims [id - 1] + "_s";
		string dpath = "Prefab/anim/" + anims [id - 1] + "_d";
		var sseat = demojis.GetChild (sender);
		var dseat = demojis.GetChild (target);

		GameObject sob = Instantiate (Resources.Load (spath), sseat) as GameObject;
		Sequence seq = DOTween.Sequence ();

		seq.Insert(0, sob.transform.DOMove(dseat.position, 0.3f).SetEase(Ease.Linear));
		seq.InsertCallback (0.3f, () => {
			Destroy(sob);
		});

		seq.InsertCallback (0.31f, () => {
			GameObject dob = Instantiate(Resources.Load(dpath), dseat) as GameObject;
		});

		seq.Play();
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

	public void showMaimaWait() {
		Maima maima = GetComponent<Maima>();
		maima.showWait();
	}

	public void showMaimaResult(Action cb) {
		Maima maima = transform.GetComponent<Maima>();
		maima.showResult(cb);
	}

    public void GameOver() {
		doGameOver();
    }

	void doGameOver() {
		game_over.SetActive (true);
		game_over.GetComponent<GameOver> ().doGameOver ();
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

		if (local < 0 || local >= gseats.Count || local >= seats.Count)
			return;

		GameObject gs = gseats[local];
		bool isIdle = rm.isIdle ();

		Seat s = seats[local].GetComponent<Seat>();

		if (s == null)
			return;

		if (player.userid <= 0) {
			s.reset ();
			s.gameObject.SetActive (false);
			gs.SetActive (false);
			return;
		}

		bool self = rm.seatindex == si;

		s.gameObject.SetActive (true);	
		s.setInfo (player.userid, player.name, player.score);
		s.setOffline (!player.online);
		s.setButton (rm.state.button == si);
		s.setReady (rm.state.state == "" ? player.ready : false);
		s.setTing (seat.tingpai);
		s.setHu (seat.hued);
		s.setQue ((self || rm.dingqueDone) ? seat.que : 0);

		gs.SetActive(!isIdle);
	}

	public void onBtnLocations() {
		GameObject ob = transform.Find("location").gameObject;

		ob.SetActive (true);
	}

	static bool show = false;
	static int id = 11;
	public void onBtnChat() {
		//GameAlert.GetInstance().show("测试");

		//DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (0);

		DHM_CardManager cm = GameObject.Find ("WestPlayer").GetComponent<DHM_CardManager> ();
		PengGangManager pg = cm._pengGangMgr;

		//cm._handCardMgr.unittest ();

		DHM_RecyleHandCardManager.playHu ();
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

	public void showChupai(int si, int card) {
		RoomMgr rm = RoomMgr.GetInstance ();
		int local = rm.getLocalIndex(si);

		if (local == 0)
			return;

		GameSeat gs = gseats[local].GetComponent<GameSeat>();
		gs.showChupai(card);
	}

	public void hideChupai() {
		foreach (GameObject s in gseats)
			s.GetComponent<GameSeat>().hideChupai();
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

		bool show = cnt > 0;
		var flower = gs.transform.Find ("flower");
		flower.gameObject.SetActive (show);

		if (show)
			flower.Find("num").GetComponent<UILabel>().text = "" + cnt;
	}
}
