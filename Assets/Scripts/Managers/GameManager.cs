
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FightModel {
	public int[] Dice;
	public int Banker;
	public List<int> DongHands;
	public List<int> XiHands;
	public List<int> NanHands;
	public List<int> BeiHands;
}

public class BoutModel{
	public int CurrentPlate;
	public int CurrentBout;
	public int GangType;
	public int HuType;
	public int LittleCanon;
};

public class GameManager : MonoBehaviour {
    public static GameManager m_instance = null;
    [Header("播放骰子的动画预设体")]
    public GameObject handBegin = null;
    [Header("骰子1点数：")]
    public int _number1 = 0;
    [Header("骰子2点数：")]
    public int _number2 = 0;
    public DHM_CardManager m_ProState;
    public bool isGang = false;
    public bool islock = false;
    private bool isReset = false;
    public int m_CurrentCount = 1;
    public int m_currentBanker;
    public GameState m_GameState = GameState.WAITING;

	public GameObject pointer;

	public List<Texture> pointers;

    public enum GameState {
        WAITING = 0,
        GAMESTART = 1
    }

	public static GameManager GetInstance() {
		return m_instance;
	}

    void Awake() {
        m_instance = this;

		InitView ();
		InitEventHandlers ();
    }
		
	void SwitchTo(int seat) {
		Debug.Log ("SwitchTo " + seat);

		int id = 4;
		if (seat >= 0 && seat < 4)
			id = seat;

		pointer.GetComponent<Renderer>().materials[0].mainTexture = pointers[id];
	}

	void InitView() {
		pointers = new List<Texture>();

		string[] mats = new string[]{ "pointer_east", "pointer_south", "pointer_west", "pointer_north", "pointer" };

		foreach (string mat in mats) {
			pointers.Add(Resources.Load ("Materials/" + mat) as Texture);
		}
	}

	void onGameBegin() {
		PlayerManager pm = PlayerManager.GetInstance ();

		for (int i = 0; i < 4; i++) {
			DHM_CardManager cm = pm.getCardManager(i);
			cm.RePlay();
		}
	}

	void onGameSync() {
		PlayerManager pm = PlayerManager.GetInstance ();

		for (int i = 0; i < 4; i++) {
			DHM_CardManager cm = pm.getCardManager(i);
			cm.sync();
		}
	}

	public DHM_CardManager getSelfCardManager() {
		int seatindex = RoomMgr.GetInstance ().seatindex;
		PlayerManager pm = PlayerManager.GetInstance ();

		return pm.getCardManager(seatindex);
	}

    void InitEventHandlers() {
		RoomMgr rm = RoomMgr.GetInstance ();
		GameMgr gm = GameMgr.GetInstance ();
		PlayerManager pm = PlayerManager.GetInstance ();

		gm.AddHandler ("game_holds", data => {
			DHM_CardManager cm = pm.getCardManager((int)data);
			cm.sync();
		});

		gm.AddHandler ("game_begin", data => {
			onGameBegin();
		});

		gm.AddHandler ("game_sync", data => {
			if (rm.isPlaying())
				onGameSync();
		});

		gm.AddHandler ("game_turn_change", data => {
			ChuPai(rm.state.turn);
		});

		gm.AddHandler ("game_mopai", data => {
			ActionInfo info = (ActionInfo)data;

			MoPai(info.seatindex, info.pai);

			if (info.seatindex == rm.seatindex)
				InteractMgr.GetInstance().checkChuPai(true);
		});

		gm.AddHandler ("game_action", data => {
			Debug.Log("get game_action");
		});

		gm.AddHandler ("user_hf_updated", data => {
			if (data == null) {
				for (int i = 0; i < rm.info.numofseats; i++) {
					DHM_CardManager cm = pm.getCardManager(i);

					cm.UpdateFlowers();
				}
			} else {
				ActionInfo info = (ActionInfo)data;
				AddFlower(info.seatindex, info.pai);
			}
		});

		gm.AddHandler ("hupai", data => {
			Hu((HuPushInfo)data);
		});

		gm.AddHandler ("mj_count", data => {
			
		});

		gm.AddHandler ("game_num", data => {
			
		});

		gm.AddHandler ("game_over", data => {
			
		});

		gm.AddHandler ("game_chupai_notify", data => {
			ActionInfo info = (ActionInfo)data;

			SomeOneChuPai(info.seatindex, info.pai);

			if (info.seatindex == rm.seatindex)
				InteractMgr.GetInstance().checkChuPai(false);
		});

		gm.AddHandler ("guo_notify", data => {
			Guo();
		});

		gm.AddHandler ("guo_result", data => {
			
		});

		gm.AddHandler ("peng_notify", data => {
			ActionInfo info = (ActionInfo)data;
			int si = info.seatindex;

			MainViewMgr.GetInstance().showAction (si, "peng");

			Peng(si, info.pai);

			if (info.seatindex == rm.seatindex)
				InteractMgr.GetInstance().checkChuPai(true);
		});

		gm.AddHandler ("ting_notify", data => {
			int si = (int)data;

			MainViewMgr.GetInstance().showAction (si, "ting");

			if (si == rm.seatindex)
				InteractMgr.GetInstance().checkChuPai(true);
		});

		gm.AddHandler ("chi_notify", data => {
			ActionInfo info = (ActionInfo)data;
			int si = info.seatindex;

			MainViewMgr.GetInstance().showAction (si, "chi");

			Chi(si, info.pai);

			if (info.seatindex == rm.seatindex)
				InteractMgr.GetInstance().checkChuPai(true);
		});

		gm.AddHandler ("gang_notify", data => {
			GangInfo info = (GangInfo)data;
			int type = 0;

			int si = info.seatindex;

			MainViewMgr.GetInstance().showAction (si, "gang");

			switch (info.gangtype) {
			case "diangang":
				type = 1;
				break;
			case "angang":
				type = 2;
				break;
			case "wangang":
				type = 3;
				break;
			}

			Gang(si, info.pai, type);

			if (info.seatindex == rm.seatindex)
				InteractMgr.GetInstance().checkChuPai(false);
		});

		gm.AddHandler ("hangang_notify", data => {
			
		});

		gm.AddHandler ("game_dice", data => {
			PlaySaiZi(RoomMgr.GetInstance().state.button, new int[]{ RoomMgr.GetInstance().state.dice1, RoomMgr.GetInstance().state.dice2 });
		});
	}

	public void PlaySaiZi(int banker, int[] dices) {
        ResourcesMgr rm = ResourcesMgr.GetInstance();
        GameObject hand = rm.InstantiateGameObjectWithType("AnShaiZiHand", ResourceType.Hand);        

		hand.GetComponent<PlayDicAnimationClick>().SetSaiZiNumber(dices);

		Transform tm = hand.transform;

        switch(banker) {
            case 0:
                tm.position = new Vector3(-0.101f, -0.015f, 0.607f);
                tm.rotation = Quaternion.Euler(Vector3.zero);
                break;
            case 1:
                tm.position = new Vector3(-0.611f, -0.015f, -0.103f);
                tm.rotation = Quaternion.Euler(new Vector3(0,-90,0));
                break;
            case 2:
                tm.position = new Vector3(0.106f, -0.015f, -0.613f);
                tm.rotation = Quaternion.Euler(new Vector3(0,-180,0));
                break;
            case 3:
                tm.position = new Vector3(0.614f, -0.015f, 0.101f);
                tm.rotation = Quaternion.Euler(new Vector3(0,90,0));
                break;
        }

        hand.GetComponent<Animation>().Play("ananniu");
    }

    public int GetDic1() {
        return _number1;
    }

    public int GetDic2() {
        return _number2;
    }

    public void FaPai(FightModel fightModel) {
        //初始化骰子点数
        int[] dics = fightModel.Dice;
        if(dics.Length!=0)
        {
            _number1 = dics[0];
            _number2 = dics[1];
        }
        //播放动画
        Debug.Log("fightModel.Banker" + fightModel.Banker);
        //PlaySaiZi(fightModel.Banker);
        //初始化玩家手牌
        PlayerManager.m_instance.m_EastPlayer.SetHandCardID(fightModel.DongHands);
        PlayerManager.m_instance.m_WestPlayer.SetHandCardID(fightModel.XiHands);
        PlayerManager.m_instance.m_SouthPlayer.SetHandCardID(fightModel.NanHands);
        PlayerManager.m_instance.m_NorthPlayer.SetHandCardID(fightModel.BeiHands);
        //设置当前玩家相机的culling Mask 1、2D只渲染自身的手牌 2、 3D不渲染自身的手和手牌
		switch(MainViewMgr.m_Instance.mSeatIndex)
        {
            
            case 0:
                Debug.Log("初始化东家Layer");
                PlayerManager.m_instance.m_EastPlayer.SetLayer();
                break;
            case 3:
                PlayerManager.m_instance.m_NorthPlayer.SetLayer();
                break;
            case 1:
                PlayerManager.m_instance.m_SouthPlayer.SetLayer();
                break;
            case 2:
                PlayerManager.m_instance.m_WestPlayer.SetLayer();
                break;
        }
    }
  
	void AddFlower(int si, int pai) {
		StartCoroutine(AddFlowerLogic(si, pai));
	}

	IEnumerator AddFlowerLogic(int si, int pai) {
		while (islock) {
			yield return new WaitForEndOfFrame();
		}

		islock = true;

		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(si);
		yield return cm.AddFlower(pai);

		yield break;
	}

	public void MoPai(int seat, int id) {
        StartCoroutine(MoPaiLogic(seat, id));
    }

    IEnumerator MoPaiLogic(int seat, int id) {
		int cnt = 0;
		while (islock) {
			cnt++;
			if (cnt > 100) {
				Debug.Log ("mopai cnt > 100");
				cnt = 0;
			}

            yield return new WaitForEndOfFrame();
        }
        islock = true;
/*
		MainViewMgr.m_Instance.ActivePlayerSeatUI(RoomMgr.mInstance.seatindex, seat);
*/
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (seat);

		cm.MoPai (id);
		//cm.ActiveChuPaiState ();

        isGang = false;
        islock = false;
        yield break;
    }

	public void Chi(int seat, int id) {
		StartCoroutine(ChiLogic(seat, id));
	}

	IEnumerator ChiLogic(int seat, int id) {
		int cnt = 0;
		while (islock) {
			cnt++;
			if (cnt > 100) {
				Debug.Log ("chi cnt > 100");
				cnt = 0;
			}
			yield return new WaitForEndOfFrame();
		}
		islock = true;
		AudioManager.Instance.PlayEffectAudio("chi");
/*
        MainViewMgr.m_Instance.ActivePlayerSeatUI(RoomMgr.mInstance.seatindex, seat);
*/
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (seat);
		cm.ChiPai (id);
		cm.ActiveChuPaiState ();
		Debug.Log ("chi");
		SwitchTo(seat);

		islock = false;
		yield break;
	}

	public void Peng(int seat, int id) {
        StartCoroutine(PengLogic(seat, id));
    }

	IEnumerator PengLogic(int seat, int id) {
		int cnt = 0;
        while (islock) {
			cnt++;
			if (cnt > 100) {
				Debug.Log ("peng cnt > 100");
				cnt = 0;
			}

            yield return new WaitForEndOfFrame();
        }
        islock = true;
        AudioManager.Instance.PlayEffectAudio("peng");
/*
        MainViewMgr.m_Instance.ActivePlayerSeatUI(RoomMgr.mInstance.seatindex, seat);
*/
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (seat);
		cm.PengPai (id);
		cm.ActiveChuPaiState ();
		Debug.Log ("peng");
		SwitchTo(seat);

        islock = false;
        yield break;
	}

	public void Gang(int seat, int id, int type) {
		StartCoroutine(GangLogic(seat, id, type));
    }

	IEnumerator GangLogic(int seat, int id, int type) {
		int cnt = 0;
        while (islock) {
			cnt++;
			if (cnt > 100) {
				Debug.Log ("gang cnt > 100");
				cnt = 0;
			}

            yield return new WaitForEndOfFrame();
        }

        islock = true;
        isGang = true;//摸牌时需要从尾部删除
        AudioManager.Instance.PlayEffectAudio("gang");
/*
		MainViewMgr.m_Instance.ActivePlayerSeatUI(RoomMgr.mInstance.seatindex, seat);
*/
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (seat);
		cm.GangPai(id, type);
		cm.ActiveChuPaiState(false);
		Debug.Log ("gang");
		SwitchTo(seat);

        islock = false;
        yield break;
    }

	public void Hu(HuPushInfo info) {
        StartCoroutine(HuLogic(info));
    }

	IEnumerator HuLogic(HuPushInfo info) {
		int cnt = 0;
        while (islock) {
			cnt++;
			if (cnt > 100) {
				Debug.Log ("hu cnt > 100");
				cnt = 0;
			}

            yield return new WaitForEndOfFrame();
        }

        islock = true;
        AudioManager.Instance.PlayEffectAudio("hu");

		int seat = info.seatindex;
/*
        MainViewMgr.m_Instance.ActivePlayerSeatUI(RoomMgr.mInstance.seatindex, seat);
*/
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (info.seatindex);

		cm.ActiveChuPaiState (false);
		Debug.Log ("hu");
		SwitchTo(seat);
		cm.HuPai (info);

        islock = false;
        yield break;
    }

    public void Guo()
    {
		for (int i = 0; i < 4; i++) {
			PlayerManager.GetInstance ().getCardManager(i).HideChuPaiState();
		}

		SwitchTo(4);
    }

    public void ChuPai(int seat) {
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (seat);
		cm.ActiveChuPaiState ();
		Debug.Log ("chupai");
		SwitchTo(seat);
    }

    public void SomeOneChuPai(int seat, int id) {
        StartCoroutine(SomeOneChuPaiLogic(seat, id));
    }

    IEnumerator SomeOneChuPaiLogic(int seat, int id) {
		int cnt = 0;
        while (islock) {
			cnt++;
			if (cnt > 100) {
				Debug.Log ("chupai cnt > 100");
				cnt = 0;
			}

            yield return new WaitForEndOfFrame();
        }

        islock = true;
/*
		if ((int)(MainViewMgr.m_Instance.mSeatIndex + 1) == seat)
        {
            islock = false;
            yield break;
        }
*/
		for (int i = 0; i < 4; i++) {
			DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (i);

			if (i == seat)
				cm.MoNiChuPai (id);
			else
				cm._recyleCardMgr.hideFocus ();
		}

		//DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (seat);
		//cm.MoNiChuPai (id);

		//islock = false;
        yield break;
    }

    public void FightEnd(int operation)
    {
        //0  有人胡，打完了
        //1  没人胡，打完了
        if (operation==0)
        {
/*       todo
			//
            if (MainViewMgr.m_Instance.m_MySeat.Equals(MainSceneMger.PlayerSeat.PlayerEast))
               NetManager.m_Instance.SendMessage(Protocol.TYPE_FIGHT, 0, FightProtocol.SETTLE_ACCOUNTS_CREQ, null);
*/
        }
        else if(operation==1 )//流局
        {
/*
            if (m_CurrentCount < 4)
                MainViewMgr.m_Instance.ActiveNext_Round();
            MainViewMgr.m_Instance.ActiveLiuJu(true);
*/
        }
    }

    public void GameEnd()
    {
        Debug.Log("对局结束");
        PlayerManager.m_instance.m_EastPlayer.HideChuPaiState();
        PlayerManager.m_instance.m_SouthPlayer.HideChuPaiState();
        PlayerManager.m_instance.m_WestPlayer.HideChuPaiState();
        PlayerManager.m_instance.m_NorthPlayer.HideChuPaiState();
		SwitchTo(4);
    }

    public void RePlay()
    {
        //PlayerManager.m_instance.m_WestPlayer
        if (!isReset)
        {
            isReset = true;
/*		todo
            DeletePai.m_instance.ClearList();
*/
            PlayerManager.m_instance.m_EastPlayer.RePlay();
            PlayerManager.m_instance.m_SouthPlayer.RePlay();
            PlayerManager.m_instance.m_WestPlayer.RePlay();
            PlayerManager.m_instance.m_NorthPlayer.RePlay();
            //m_CurrentCount++;
/*
            MainViewMgr.m_Instance.SetGameCount(++m_CurrentCount);
*/
            GameEnd();
            ClearPaiDuo( GameObject.Find("tableslot_up").transform,GameObject.Find("tableslot_up 1").transform);
            ClearPaiDuo(GameObject.Find("tableslot_right").transform, GameObject.Find("tableslot_right 1").transform);
            ClearPaiDuo(GameObject.Find("tableslot_left").transform, GameObject.Find("tableslot_left 1").transform);
            ClearPaiDuo(GameObject.Find("tableslot_down").transform, GameObject.Find("tableslot_down 1").transform);
        }
    }
    void ClearPaiDuo(Transform target,Transform bro)
    {
        Transform[] trans = target.GetComponentsInChildren<Transform>();
        for (int i = trans.Length - 1; i >= 0; i--)
        {
            if (trans[i] != target && trans[i]!=bro)
            {
                Destroy(trans[i].gameObject);
            }
        }
    }
    public void Next_Round(FightModel fightModel)
    {
/*  todo
        RoomModel roomModel = new RoomModel();

        roomModel.DongUser = new UserModel();
        roomModel.DongUser.IsReady = fightModel.DongReady;

        roomModel.NanUser = new UserModel();
        roomModel.NanUser.IsReady = fightModel.NanReady;

        roomModel.XiUser = new UserModel();
        roomModel.XiUser.IsReady = fightModel.XiReady;

        roomModel.BeiUser = new UserModel();
        roomModel.BeiUser.IsReady = fightModel.BeiReady;

        MainViewMgr.m_Instance.initWaitInfo(roomModel);
        if(fightModel.DongReady && fightModel.NanReady && fightModel.XiReady && fightModel.BeiReady)
        {
            if(RoomInfoMgr.mInstance.m_MySeat.Equals(MainSceneMger.PlayerSeat.PlayerEast))
            {
                NetManager.m_Instance.SendMessage(Protocol.TYPE_FIGHT, RoomInfoMgr.mInstance.m_RoomID, FightProtocol.START_CREQ, null);
            }
        }
*/
    }
}
