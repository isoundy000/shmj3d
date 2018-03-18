
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
    }

	void Start() {
		InitEventHandlers ();
	}

	void SwitchTo(int seat) {
		Debug.Log ("SwitchTo " + seat);

		int id = seat >= 4 ? 4 : RoomMgr.GetInstance().getLocalIndex(seat);

		pointer.GetComponent<Renderer>().materials[0].mainTexture = pointers[id];

		MainViewMgr.GetInstance().switchTo(seat);
	}

	void InitView() {
		pointers = new List<Texture>();

		string[] mats = new string[]{ "pointer_east", "pointer_south", "pointer_west", "pointer_north", "pointer" };

		foreach (string mat in mats) {
			pointers.Add(Resources.Load ("Materials/" + mat) as Texture);
		}
	}

	void onGameBegin() {
		DHM_CardManager[] cms = PlayerManager.GetInstance().getCardManagers();
		foreach (DHM_CardManager cm in cms)
			cm.RePlay();
	}

	void onGameSync() {
		ResourcesMgr.GetInstance().StopAllHands();

		DHM_CardManager[] cms = PlayerManager.GetInstance().getCardManagers();
		foreach (DHM_CardManager cm in cms)
			cm.sync();

		RoomMgr rm = RoomMgr.GetInstance();
		InteractMgr.GetInstance().checkChuPai(rm.isMyTurn());
	}

    void InitEventHandlers() {
		RoomMgr rm = RoomMgr.GetInstance ();
		GameMgr gm = GameMgr.GetInstance ();
		PlayerManager pm = PlayerManager.GetInstance ();

		gm.AddHandler ("game_holds", data => {
			DHM_CardManager cm = pm.getCardManager((int)data);
			cm.FaPai();
		});

		gm.AddHandler ("game_begin", data => {
			onGameBegin();
		});

		gm.AddHandler ("game_sync", data => {
			if (rm.isPlaying())
				sync(); //onGameSync();
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
				foreach (DHM_CardManager cm in pm.getCardManagers())
					cm.UpdateFlowers();
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
		});

		gm.AddHandler ("guo_notify", data => {
			Guo();
		});

		gm.AddHandler ("guo_result", data => {
			
		});

		gm.AddHandler ("peng_notify", data => {
			ActionInfo info = (ActionInfo)data;
			int si = info.seatindex;

			Peng(si, info.pai);
		});

		gm.AddHandler ("ting_notify", data => {
			int si = (int)data;

			MainViewMgr.GetInstance().showAction (si, "ting");

			if (si == rm.seatindex) {
				InteractMgr im = InteractMgr.GetInstance();

				im.showPrompt();
				im.checkChuPai(true);
			}

			DHM_CardManager cm = pm.getCardManager (si);
			cm.Ting();
		});

		gm.AddHandler ("chi_notify", data => {
			ActionInfo info = (ActionInfo)data;
			int si = info.seatindex;

			Chi(si, info.pai);
		});

		gm.AddHandler ("gang_notify", data => {
			GangInfo info = (GangInfo)data;
			int type = 0;
			int si = info.seatindex;

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
		});

		gm.AddHandler ("hangang_notify", data => {
			
		});

		gm.AddHandler ("game_dice", data => {
			PlaySaiZi(rm.state.button, new int[]{ rm.state.dice1, rm.state.dice2 });
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

	void sync() {
		StartCoroutine(syncLogic());
	}

	IEnumerator syncLogic() {
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

		onGameSync ();

		islock = false;
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

		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (seat);

		cm.MoPai (id);

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

		AudioManager.GetInstance().PlayEffectAudio("chi");
		MainViewMgr.GetInstance().showAction(seat, "chi");

		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);
		cm.ChiPai(id);
		cm.ActiveChuPaiState();

		SwitchTo(seat);

		if (seat == RoomMgr.GetInstance().seatindex)
			InteractMgr.GetInstance().checkChuPai(true);

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

		AudioManager.GetInstance().PlayEffectAudio("peng");
		MainViewMgr.GetInstance().showAction (seat, "peng");

		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);
		cm.PengPai(id);
		cm.ActiveChuPaiState();

		SwitchTo(seat);

		if (seat == RoomMgr.GetInstance().seatindex)
			InteractMgr.GetInstance().checkChuPai(true);

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

		MainViewMgr.GetInstance().showAction (seat, "gang");
		AudioManager.GetInstance().PlayEffectAudio("gang");

		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);
		cm.GangPai(id, type);
		cm.ActiveChuPaiState(false);

		SwitchTo(seat);

		if (seat == RoomMgr.GetInstance().seatindex)
			InteractMgr.GetInstance().checkChuPai(false);
		
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

		int seat = info.seatindex;

		MainViewMgr.GetInstance().showAction (seat, "hu");
		AudioManager.GetInstance().PlayEffectAudio("hu");
		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);

		cm.ActiveChuPaiState(false);

		SwitchTo(seat);
		cm.HuPai(info);

        islock = false;
        yield break;
    }

    public void Guo()
    {
		foreach (DHM_CardManager cm in PlayerManager.GetInstance().getCardManagers())
			cm.HideChuPaiState();

		SwitchTo(4);
    }

    public void ChuPai(int seat) {
		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);
		cm.ActiveChuPaiState();

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

		foreach (DHM_CardManager cm in PlayerManager.GetInstance().getCardManagers()) {
			if (cm.seatindex == seat)
				cm.MoNiChuPai(id);
			//else
			//	cm._recyleCardMgr.hideFocus();
		}

        yield break;
    }

	public void exit(float delay = 0) {
		StartCoroutine(_exit(delay));
	}

	IEnumerator _exit(float delay) {
		int cnt = 0;
		while (islock) {
			cnt++;
			if (cnt > 100) {
				Debug.Log ("exit cnt > 100");
				cnt = 0;
			}

			yield return new WaitForEndOfFrame();
		}

		islock = true;

		ReplayMgr rm = ReplayMgr.GetInstance();
		GameMgr gm = GameMgr.GetInstance();
		RoomMgr room = RoomMgr.GetInstance();

		ResourcesMgr.GetInstance ().release ();

		rm.clear();
		gm.Reset();
		room.reset();

		if (delay > 0) {
			Utils.setTimeout (() => {
				LoadingScene.LoadNewScene ("02.lobby");
			}, 2.0f);
		} else
			LoadingScene.LoadNewScene ("02.lobby");
	}

    public void GameEnd()
    {
		foreach (DHM_CardManager cm in PlayerManager.GetInstance().getCardManagers())
			cm.HideChuPaiState();

		SwitchTo(4);
    }

    public void RePlay()
    {
        if (!isReset) {
            isReset = true;

			foreach (DHM_CardManager cm in PlayerManager.GetInstance().getCardManagers())
				cm.RePlay();

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
}


