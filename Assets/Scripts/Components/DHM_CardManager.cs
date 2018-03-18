
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;

public class DHM_CardManager : MonoBehaviour {
    [Header("手牌管理")]
    public DHM_HandCardManager _handCardMgr = null;
    [Header("桌牌管理")]
    public DHM_RecyleHandCardManager _recyleCardMgr = null;
    [Header("碰牌区管理")]
    public PengGangManager _pengGangMgr = null;
	
    DHM_HandAnimationCtr _handAnimationCtr = null;
    [SerializeField]
    GameObject m_Tip = null;
    public DHM_HandCardManager.PlayerType m_Player;
    public Highlighter highLighter;
    public Color m_highLighteColerMin;
    public Color m_highLighteColerMax;
    GameObject tip;

	public int seatindex;
	int localindex;

	void Start () {
        tip = Instantiate(m_Tip);
        tip.SetActive(false);

		switch (m_Player) {
		case DHM_HandCardManager.PlayerType.East:
			localindex = 0;
			break;
		case DHM_HandCardManager.PlayerType.South:
			localindex = 1;
			break;
		case DHM_HandCardManager.PlayerType.West:
			localindex = 2;
			break;
		case DHM_HandCardManager.PlayerType.North:
			localindex = 3;
			break;
		}

		seatindex = RoomMgr.GetInstance().getSeatIndexByLocal(localindex);

        _handCardMgr = GetComponentInChildren<DHM_HandCardManager>();
        _recyleCardMgr = GetComponentInChildren<DHM_RecyleHandCardManager>();
        _pengGangMgr = GetComponentInChildren<PengGangManager>();
        _handAnimationCtr = GetComponentInChildren<DHM_HandAnimationCtr>();

		_handCardMgr.seatindex = seatindex;
		_recyleCardMgr.seatindex = seatindex;
		_pengGangMgr.seatindex = seatindex;

        _handCardMgr.chuPaiEvent += _recyleCardMgr.ChuPai;
        _recyleCardMgr.ChuPaiCallBackEvent += _handCardMgr.ChuPaiCallBackEventHandle;

		initHighLight();

		Debug.Log ("direction: " + m_Player);
		Debug.Log ("CardManager start: local=" + localindex);
		Debug.Log ("seatindex=" + seatindex);

		if (RoomMgr.GetInstance ().seatindex == seatindex)
			SetLayer ();
	}

	public DHM_HandCardManager getHCM() {
		return _handCardMgr;
	}

    void initHighLight() {
/*
        GameObject obj = null;

        switch (seatindex)
        {
            case 0:
                obj = GameObject.Find("eastlight");
                m_highLighteColerMin = new Color(0, 1, 0,0);
                m_highLighteColerMax = Color.green;
                break;
			case 1:
				obj = GameObject.Find("southlight");
				m_highLighteColerMin = new Color(1, 0.92f, 0.016f, 0);
				m_highLighteColerMax = Color.yellow;
				break;
            case 2:
                obj = GameObject.Find("westlight");
                m_highLighteColerMin = new Color(0, 0, 1, 0);
                m_highLighteColerMax = Color.blue;
                break;
            case 3:
                obj = GameObject.Find("northlight");
                m_highLighteColerMin = new Color(1, 0, 0, 0);
                m_highLighteColerMax = Color.red;
                break;
        }

        highLighter = obj.GetComponent<Highlighter>();
        if (highLighter == null)
            highLighter = obj.AddComponent<Highlighter>();
*/
    }

	public void sync() {
        _handCardMgr.sync();
		_recyleCardMgr.sync();
		_pengGangMgr.sync();
	}

	public void FaPai() {
		_handCardMgr.FaPai();
    }

    public void MoPai(int id) {
        _handCardMgr.SetMoHandCard(id);
    }

    public void HideChuPaiState() {
/*
        if (highLighter == null)
            initHighLight();

        highLighter.FlashingOff();
*/
        _handCardMgr.IsState = false;
    }

    public void ActiveChuPaiState(bool isState = true) {
        Debug.Log("my turn: "+ m_Player);

		DHM_CardManager cm = GameManager.GetInstance ().m_ProState;
        if (cm != null) {
            //Debug.Log("上一回合是："+ cm.m_Player);
            cm.HideChuPaiState();
        }

/*
        if (highLighter == null)
            initHighLight();

        highLighter.FlashingOn(m_highLighteColerMin, m_highLighteColerMax);
*/
        _handCardMgr.IsState = isState;
    }

	#if UNIT_TEST
    public void SetHandCardID(List<int> handCardIdList) {
        _handCardMgr.SetIDArray(handCardIdList);
    }
	#endif

    public void SetLayer() {
        _handCardMgr.SetLayer(LayerMask.NameToLayer("Self"));
    }

	void DelRecycle() {
		DHM_CardManager cm = GameManager.GetInstance ().m_ProState;
		if (cm != null)
			cm._recyleCardMgr.DeleteCard();
	}

	public void ChiPai(int id) {
		_handCardMgr.Chi (id);
		DelRecycle();
		_pengGangMgr.Chi(id);
	}

    public void PengPai(int id) {
        _handCardMgr.Peng(id);
		DelRecycle();
        _pengGangMgr.Peng(id);
    }

	public void GangPai(int id, int type) {
        if (type == 1) {
            MingBar(id);
        } else if (type == 2) {
            DarkBar(id);
        } else if (type == 3) {
            AddBar(id);
        }
    }

    void MingBar(int id) {
        _handCardMgr.Gang(id, 1);
		DelRecycle();
        _pengGangMgr.Gang(id, false);
    }

    void DarkBar(int id) {
        _handCardMgr.Gang(id, 2);
        _pengGangMgr.Gang(id, true);
    }

    void AddBar(int id) {
		_handCardMgr.Gang(id, 3);
        _pengGangMgr.CreateWanGangCard(id);
    }

    public void MoNiChuPai(int id) {
        _handCardMgr.MoNiChuPai(id);
    }

	public void HuPai(HuPushInfo info) {
		int id = info.hupai;
		if (!info.iszimo) {
            _handCardMgr.HuPai(id);
			//DelRecycle();
        } else {
            ZiMo(id);
        }

        HideChuPaiState();
    }

	public void ZiMo(int id) {
        _handCardMgr.RemoveMoHandCard(id);
        _handCardMgr.HuPai(id);
    }

	public void Ting() {
		_handCardMgr.Ting();
	}

    public void RePlay()
    {
        _handCardMgr.ResetInfo();
        _recyleCardMgr.ResetInfo();
		_pengGangMgr.ResetInfo();
    }

	public IEnumerator AddFlower(int id) {
		return _handCardMgr._AddFlower(id);
	}

	public void UpdateFlowers() {
		_handCardMgr.UpdateFlowers();
	}

	public void HighlightRecycle(int id, bool enable) {
		_recyleCardMgr.highlight(id, enable);
		_pengGangMgr.highlight(id, enable);
	}
}


