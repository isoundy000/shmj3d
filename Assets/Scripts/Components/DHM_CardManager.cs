
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DHM_CardManager : MonoBehaviour {
    [Header("手牌管理")]
    public DHM_HandCardManager _handCardMgr = null;
    [Header("桌牌管理")]
    public DHM_RecyleHandCardManager _recyleCardMgr = null;
    [Header("碰牌区管理")]
    public PengGangManager _pengGangMgr = null;
	
    DHM_HandAnimationCtr _handAnimationCtr = null;

    public DHM_HandCardManager.PlayerType m_Player;

	public int seatindex;
	int localindex;

	void Start() {
		Init ();
	}

	void Init () {

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

		var rm = RoomMgr.GetInstance ();

		seatindex = rm.getSeatIndexByLocal(localindex);

        _handCardMgr = GetComponentInChildren<DHM_HandCardManager>();
        _recyleCardMgr = GetComponentInChildren<DHM_RecyleHandCardManager>();
        _pengGangMgr = GetComponentInChildren<PengGangManager>();
        _handAnimationCtr = GetComponentInChildren<DHM_HandAnimationCtr>();

		_handCardMgr.seatindex = seatindex;
		_recyleCardMgr.seatindex = seatindex;
		_pengGangMgr.seatindex = seatindex;

        _handCardMgr.chuPaiEvent += _recyleCardMgr.ChuPai;
        _recyleCardMgr.ChuPaiCallBackEvent += _handCardMgr.ChuPaiCallBackEventHandle;

		if (rm.seatindex == seatindex)
			SetLayer ();
	}

	public void Reset() {
		var rm = RoomMgr.GetInstance ();

		seatindex = rm.getSeatIndexByLocal(localindex);
		_handCardMgr.seatindex = seatindex;
		_recyleCardMgr.seatindex = seatindex;
		_pengGangMgr.seatindex = seatindex;
	}

	public DHM_HandCardManager getHCM() {
		return _handCardMgr;
	}

	public void sync() {
        _handCardMgr.sync();
		_recyleCardMgr.sync();
		_pengGangMgr.sync();
	}

	public void FaPai(Action act) {
		_handCardMgr.FaPai(act);
    }

    public void MoPai(int id) {
        _handCardMgr.SetMoHandCard(id);
    }

    public void SetLayer() {
        _handCardMgr.SetLayer(LayerMask.NameToLayer("Self"));
    }

	void DelRecycle() {
		DHM_CardManager cm = GameManager.GetInstance().m_ProState;
		if (cm != null)
			cm._recyleCardMgr.DeleteCard();
	}

	public void ChiPai(int id) {
		_handCardMgr.Chi(id);
		DelRecycle();
		_pengGangMgr.Chi(id);
	}

    public void PengPai(int id) {
        _handCardMgr.Peng(id);
		DelRecycle();
        _pengGangMgr.Peng(id);
    }

	public void Sort() {
		_handCardMgr.SortCards ();
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

	public void ChuPai(int id, Action cb) {
        _handCardMgr.ChuPai(id, cb);
    }

	public void HuPai(HuPushInfo info, Action cb) {
        _handCardMgr.HuPai(info, cb);
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

	public void AddFlower(int id, Action cb) {
		_handCardMgr.AddFlower(id, cb);
	}

	public void UpdateFlowers() {
		_handCardMgr.UpdateFlowers();
	}

	public void HighlightRecycle(int id, bool enable) {
		_recyleCardMgr.highlight(id, enable);
		_pengGangMgr.highlight(id, enable);
	}
}


