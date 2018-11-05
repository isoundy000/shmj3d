
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class SyncItem {
	public string cmd;
	public object data;
	public Action<SyncItem> handler;
	public bool sync;
	public bool called;
	public bool done;

	public SyncItem(string _cmd, object _data, Action<SyncItem> _handler, bool _sync = false) {
		cmd = _cmd;
		data = _data;
		handler = _handler;
		sync = _sync;
		called = false;
		done = false;
	}
}

public class GameManager : MonoBehaviour {
    public static GameManager m_instance = null;
    //[Header("播放骰子的动画预设体")]
    //public GameObject handBegin = null;
    [Header("骰子1点数：")]
    public int _number1 = 0;
    [Header("骰子2点数：")]
    public int _number2 = 0;
    public DHM_CardManager m_ProState;
    public bool isGang = false;
    private bool isReset = false;
    public int m_CurrentCount = 1;
    public int m_currentBanker;
    public GameState m_GameState = GameState.WAITING;
	Queue<SyncItem> syncQueue = new Queue<SyncItem>();

	public GameObject switcher;
	Switcher mSwitcher;

	bool synchronized = false;

    public enum GameState {
        WAITING = 0,
        GAMESTART = 1
    }

	public static GameManager GetInstance() {
		return m_instance;
	}

    void Awake() {
        m_instance = this;
    }

	void Start() {
		mSwitcher = switcher.GetComponent<Switcher>();

		InitEventHandlers ();
	}

	public void outSync() {
		synchronized = false;
	}

	void SwitchTo(int seat) {
		Debug.Log ("SwitchTo " + seat);

		mSwitcher.switchTo(seat);
		MainViewMgr.GetInstance().switchTo(seat);
	}

	void onGameBegin() {
		DHM_CardManager[] cms = PlayerManager.GetInstance().getCardManagers();
		foreach (DHM_CardManager cm in cms)
			cm.RePlay();

		AudioManager.GetInstance ().PlayEffectAudio ("duijukaishi");
	}

	void onGameSync() {
		ResourcesMgr.GetInstance().StopAllHands();

		DHM_CardManager[] cms = PlayerManager.GetInstance().getCardManagers();
		foreach (DHM_CardManager cm in cms)
			cm.sync();

		RoomMgr rm = RoomMgr.GetInstance();
		bool check = rm.state.state == "playing" && rm.isMyTurn() && rm.waitChupai();

		InteractMgr.GetInstance().checkChuPai(check);
	}

	void syncDone(SyncItem item) {
		item.done = true;
	}

	void EnQueueCmd(string cmd, object data, Action<SyncItem> handler, bool sync = true) {
		SyncItem item = new SyncItem(cmd, data, handler, sync);
		syncQueue.Enqueue(item);
	}

    void InitEventHandlers() {
		RoomMgr rm = RoomMgr.GetInstance ();
		GameMgr gm = GameMgr.GetInstance ();
		PlayerManager pm = PlayerManager.GetInstance ();

		gm.AddHandler ("game_hand_cards", data => {
			EnQueueCmd("game_hand_cards", data, item => {
				int cnt = rm.seats.Count;

				for (int i = 0; i < cnt; i++) {
					DHM_CardManager cm = pm.getCardManager(i);
					Action act = null;
					if (i == rm.seatindex)
						act = () => syncDone(item);

					cm.FaPai(act);
				}
			}, false);
		});

		gm.AddHandler ("game_seats_reset", data => {
			EnQueueCmd("game_seats_reset", data, item => {
				var cms = pm.getCardManagers();

				for (int i = 0; i < cms.Length; i++) {
					var cm = cms[i];
					cm.Reset();
				}
			});
		});

		gm.AddHandler ("game_begin", data => {
			EnQueueCmd("game_begin", data, item => {
				onGameBegin();
				synchronized = true;
			});
		});

		gm.AddHandler ("game_sync", data => {
			EnQueueCmd("game_sync", data, item => {
				if (rm.isPlaying())
					sync();

				synchronized = true;
			});
		});

		gm.AddHandler ("game_action", data => {
			EnQueueCmd("game_action", data, item => {
				InteractMgr.GetInstance().ShowAction();
			});
		});

		gm.AddHandler ("game_turn_change", data => {
			EnQueueCmd("game_turn_change", data, item => {
				TurnChange(rm.state.turn);
			});
		});

		gm.AddHandler ("game_mopai", data => {
			EnQueueCmd("game_mopai", data, item => {
				if (!synchronized)
					return;

				ActionInfo info = (ActionInfo)item.data;

				MoPai(info.seatindex, info.pai);
				if (info.seatindex == rm.seatindex)
					InteractMgr.GetInstance().checkChuPai(false);
			});
		});

		gm.AddHandler ("user_hf_updated", data => {
			EnQueueCmd("user_hf_updated", data, item => {
				if (item.data == null) {
					foreach (DHM_CardManager cm in pm.getCardManagers())
						cm.UpdateFlowers();

					syncDone(item);
				} else {
					ActionInfo info = (ActionInfo)item.data;
					AddFlower(info.seatindex, info.pai, ()=>syncDone(item));
				}
			}, false);
		});

		gm.AddHandler ("hupai", data => {
			EnQueueCmd("hupai", data, item => {
				Hu((HuPushInfo)item.data, ()=>syncDone(item));
			}, false);
		});

		gm.AddHandler ("game_chupai_notify", data => {
			EnQueueCmd("game_chupai_notify", data, item => {
				if (!synchronized) {
					syncDone(item);
					return;
				}

				ActionInfo info = (ActionInfo)item.data;
				SomeOneChuPai(info.seatindex, info.pai, ()=>syncDone(item));
			}, false);
		});

		gm.AddHandler ("guo_notify", data => {
			EnQueueCmd("guo_notify", data, item => {
				if (!synchronized)
					return;
				
				Guo();
			});
		});
			
		gm.AddHandler ("peng_notify", data => {
			EnQueueCmd("peng_notify", data, item => {
				if (!synchronized)
					return;

				ActionInfo info = (ActionInfo)item.data;
				int si = info.seatindex;

				Peng(si, info.pai);
			});
		});

		gm.AddHandler ("ting_notify", data => {
			EnQueueCmd("ting_notify", data, item => {
				if (!synchronized)
					return;
	
				int si = (int)item.data;

				//MainViewMgr.GetInstance().showAction (si, "ting");

				if (si == rm.seatindex) {
					InteractMgr im = InteractMgr.GetInstance();

					im.showPrompt();
					//im.checkChuPai(true);
				}

				DHM_CardManager cm = pm.getCardManager (si);
				cm.Ting();
			});
		});

		gm.AddHandler ("chi_notify", data => {
			EnQueueCmd("chi_notify", data, item => {
				if (!synchronized)
					return;

				ActionInfo info = (ActionInfo)item.data;
				int si = info.seatindex;

				Chi(si, info.pai);
			});
		});

		gm.AddHandler ("gang_notify", data => {
			EnQueueCmd("gang_notify", data, item => {
				if (!synchronized)
					return;
				
				GangInfo info = (GangInfo)item.data;
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
		});

		gm.AddHandler ("game_dice", data => {
			EnQueueCmd("game_dice", data, item => {
				PlaySaiZi(rm.state.button, new int[]{ rm.state.dice1, rm.state.dice2 });
			});
		});

		gm.AddHandler ("game_over", data => {
			EnQueueCmd("game_over", data, item => {
				MainViewMgr.GetInstance().GameOver();
			});
		});

        gm.AddHandler("game_chicken", data => {
			GameChickenPush gcp = (GameChickenPush)data;
            EnQueueCmd("game_chicken", data, item => {
				InteractMgr.GetInstance().showChicken(gcp.si, gcp.key, ()=>syncDone(item));
            }, false);
        });

		gm.AddHandler("game_wait_maima", data => {
			EnQueueCmd("game_wait_maima", data, item => {
				MainViewMgr.GetInstance().showMaimaWait();
			});
		});

		gm.AddHandler("game_maima", data => {
			EnQueueCmd("game_maima", data, item => {
				MainViewMgr.GetInstance().showMaimaResult(()=>syncDone(item));
			}, false);
		});

		gm.AddHandler ("game_dingque", data => {
			EnQueueCmd("game_dingque", data, item => {
				Sort();
			});
		});
	}

	void Update() {
		while (syncQueue.Count > 0) {
			SyncItem item = syncQueue.Peek();
			if (item.sync) {
				item.handler(item);
				syncQueue.Dequeue();
			} else {
				if (!item.called) {
					item.handler(item);
					item.called = true;
					break;
				} else {
					if (item.done)
						syncQueue.Dequeue ();
					else
						break;
				}
			}
		}
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
  
	void AddFlower(int si, int pai, Action cb) {
		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(si);
		cm.AddFlower(pai, cb);
	}

	void sync() {
		onGameSync ();
	}

	void Sort() {
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (RoomMgr.GetInstance().seatindex);

		cm.Sort ();
	}

	public void MoPai(int seat, int id) {
		DHM_CardManager cm = PlayerManager.GetInstance ().getCardManager (seat);

		cm.MoPai (id);
        isGang = false;
    }

	public void Chi(int seat, int id) {
		AudioManager.GetInstance().PlayDialect(seat, "chi");
		MainViewMgr.GetInstance().showAction(seat, "chi");
		hideChupai ();

		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);
		cm.ChiPai(id);

		SwitchTo(seat);

		InteractMgr im = InteractMgr.GetInstance();

		if (seat == RoomMgr.GetInstance().seatindex)
			im.checkChuPai(false);
		else
			im.updatePrompt(id);
	}

	public void Peng(int seat, int id) {
		AudioManager.GetInstance().PlayDialect(seat, "peng");
		MainViewMgr.GetInstance().showAction (seat, "peng");
		hideChupai ();

		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);
		cm.PengPai(id);

		SwitchTo(seat);

		if (seat == RoomMgr.GetInstance ().seatindex)
			InteractMgr.GetInstance ().checkChuPai (false);
	}

	public void Gang(int seat, int id, int type) {
        isGang = true;

		MainViewMgr.GetInstance().showAction (seat, "gang");

		string gt = "minggang";
		if (type == 2)
			gt = "angang";
		else if (type == 3)
			gt = "wangang";

		AudioManager.GetInstance().PlayDialect(seat, gt);
		hideChupai ();

		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);
		cm.GangPai(id, type);

		SwitchTo(seat);

		if (seat == RoomMgr.GetInstance().seatindex)
			InteractMgr.GetInstance().checkChuPai(false);
    }

	public void Hu(HuPushInfo info, Action cb) {
		int seat = info.seatindex;

		//MainViewMgr.GetInstance().showAction (seat, "hu");
		//AudioManager.GetInstance().PlayEffectAudio("hu");
		hideChupai ();
		DHM_CardManager cm = PlayerManager.GetInstance().getCardManager(seat);

		SwitchTo(seat);
		cm.HuPai(info, cb);
    }

    public void Guo()
    {
		SwitchTo(4);
    }

    void TurnChange(int seat) {
		SwitchTo(seat);
    }

	void hideChupai() {
		MainViewMgr.GetInstance ().hideChupai ();
	}

	public void SomeOneChuPai(int seat, int id, Action cb) {
		MainViewMgr mm = MainViewMgr.GetInstance ();
		mm.hideChupai ();
		mm.showChupai (seat, id);

		foreach (DHM_CardManager cm in PlayerManager.GetInstance().getCardManagers()) {
			if (cm.seatindex == seat)
				cm.ChuPai(id, cb);
		}
    }

	public void exit(float delay = 0) {
		StartCoroutine(_exit(delay));
	}

	IEnumerator _exit(float delay) {
		while (syncQueue.Count > 0) {
			yield return new WaitForEndOfFrame();
		}

		ReplayMgr rm = ReplayMgr.GetInstance();
		GameMgr gm = GameMgr.GetInstance();
		RoomMgr room = RoomMgr.GetInstance();

		ResourcesMgr.GetInstance ().release ();

		rm.clear();
		gm.Reset();
		room.reset();

		if (delay > 0) {
			PUtils.setTimeout (() => {
				LoadingScene.LoadNewScene ("02.lobby");
			}, delay);
		} else
			LoadingScene.LoadNewScene ("02.lobby");
	}

    public void GameEnd()
    {
		hideChupai ();
		SwitchTo(4);
    }

    public void RePlay()
    {
        if (!isReset) {
            isReset = true;

			foreach (DHM_CardManager cm in PlayerManager.GetInstance().getCardManagers())
				cm.RePlay();

            GameEnd();
        }
    }
}


