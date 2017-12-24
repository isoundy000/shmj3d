
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScoreModel {};
public class RoomModel {};

public class MainViewMgr : MonoBehaviour {
    public static MainViewMgr m_Instance = null;

	public int mSeatIndex;

	public Transform prepare;

    void Awake() {
        m_Instance = this;

		InitEventHandlers ();
    }

	public static MainViewMgr GetInstance() {
		return m_Instance;
	}

    void Start() {
		InitView ();

		InitSeats ();

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

		UIButton btnReady = prepare.FindChild ("actions/btnReady").GetComponent<UIButton> ();
		btnReady.onClick.Add (new EventDelegate(this, "onBtnReadyClicked"));

		UIButton btnInvite = prepare.FindChild ("actions/btnInvite").GetComponent<UIButton> ();
		btnInvite.onClick.Add (new EventDelegate(this, "onBtnInviteClicked"));

		//AudioManager.Instance.PlayEffectAudio("ui_click");
    }

	void onBtnInviteClicked() {

	}

	void onBtnReadyClicked() {
		Debug.Log ("onBtnReadyClicked");
		NetMgr.GetInstance ().send ("ready");
	}

	void InitSingleSeat(int id) {
		// todo
	}

	void InitSeats() {
		// todo
	}

	void refreshBtns() {
		RoomMgr rm = RoomMgr.GetInstance ();
		NetMgr nm = NetMgr.GetInstance ();
		bool isIdle = rm.isIdle ();
		PlayerInfo player = rm.getSelfPlayer ();

		Transform actions = prepare.FindChild ("actions");
		GameObject btnReady = actions.FindChild ("btnReady").gameObject;
		GameObject waiting = prepare.FindChild ("waiting").gameObject;

		waiting.SetActive (isIdle);
		actions.gameObject.SetActive (isIdle);
		btnReady.SetActive (!player.ready);
	}

	void InitEventHandlers() {
		GameMgr gm = GameMgr.GetInstance ();

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

		gm.AddHandler("voice_msg", data=>{

		});

		gm.AddHandler("chat_push", data=>{

		});

		gm.AddHandler("quick_chat_push", data=>{

		});

		gm.AddHandler("emoji_push", data=>{

		});

		gm.AddHandler("demoji_push", data=>{

		});
	}
}
