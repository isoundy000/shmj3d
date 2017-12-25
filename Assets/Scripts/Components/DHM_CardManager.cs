
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using HighlightingSystem;

public class DHM_CardManager : MonoBehaviour {
    [Header("手牌管理")]
    DHM_HandCardManager _handCardMgr = null;
    [Header("桌牌管理")]
    DHM_RecyleHandCardManager _recyleCardMgr = null;
    [Header("碰牌区管理")]
    PengGangManager _pengGangMgr = null;
    DHM_HandAnimationCtr _handAnimationCtr = null;
    [SerializeField]
    GameObject m_Tip = null;
    public DHM_HandCardManager.PlayerType m_Player;
//  public Highlighter highLighter;
    public Color m_highLighteColerMin;
    public Color m_highLighteColerMax;
    GameObject tip;

	int seatindex;

	void Start () {
        tip = Instantiate(m_Tip);
        tip.SetActive(false);

		switch (m_Player) {
		case DHM_HandCardManager.PlayerType.East:
			seatindex = 0;
			break;
		case DHM_HandCardManager.PlayerType.South:
			seatindex = 1;
			break;
		case DHM_HandCardManager.PlayerType.West:
			seatindex = 2;
			break;
		case DHM_HandCardManager.PlayerType.North:
			seatindex = 3;
			break;
		}

        _handCardMgr = GetComponentInChildren<DHM_HandCardManager>();
        _recyleCardMgr = GetComponentInChildren<DHM_RecyleHandCardManager>();
        _pengGangMgr = GetComponentInChildren<PengGangManager>();
        _handAnimationCtr = GetComponentInChildren<DHM_HandAnimationCtr>();

		_handCardMgr.seatindex = seatindex;
		_recyleCardMgr.seatindex = seatindex;
		_pengGangMgr.seatindex = seatindex;

        _handCardMgr.chuPaiEvent += _recyleCardMgr.ChuPai;
        _recyleCardMgr.ChuPaiCallBackEvent += _handCardMgr.ChuPaiCallBackEventHandle;

		//initHighLight();

		if (RoomMgr.GetInstance ().seatindex == seatindex)
			SetLayer ();
	}

    void initHighLight() {
        GameObject obj = null;
        switch (seatindex)
        {
            case 0:
                obj = GameObject.Find("eastlight");
                m_highLighteColerMin = new Color(1, 0.92f, 0.016f,0);
                m_highLighteColerMax = Color.yellow;
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

/*  fuck todo
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
		_handCardMgr.FaPai ();
    }

    public void MoPai(int id) {
        _handCardMgr.SetMoHandCard(id);
    }

    public void HideChuPaiState() {
/*  fuck todo
        if(highLighter==null)
        {
            initHighLight();
        }
        highLighter.FlashingOff();
*/
        _handCardMgr.IsState = false;
    }

    public void ActiveChuPaiState(bool isState = true) {
        Debug.Log("************我的回合，我是："+ m_Player);

		DHM_CardManager cm = GameManager.GetInstance ().m_ProState;
        if (cm != null) {
            Debug.Log("上一回合是："+ cm.m_Player);
            cm.HideChuPaiState();
        }

/* fuck todo
        if (highLighter == null)
        {
            initHighLight();
        }
        highLighter.FlashingOn(m_highLighteColerMin, m_highLighteColerMax);
*/
        _handCardMgr.IsState = isState;
    }

    public void SetHandCardID(List<int> handCardIdList) {
        _handCardMgr.SetIDArray(handCardIdList);
    }

    public void SetLayer() {
        _handCardMgr.SetLayer(LayerMask.NameToLayer("Self"));
    }

	public void ChiPai(int id) {
		_handCardMgr.Chi (id);

		DHM_CardManager cm = GameManager.GetInstance ().m_ProState;
		if (cm != null)
			cm._recyleCardMgr.DeleteCard();

		_pengGangMgr.Chi(id);
	}

    public void PengPai(int id) {
        //手牌删除2张
        _handCardMgr.Peng(id);
        //桌牌删一张

		DHM_CardManager cm = GameManager.GetInstance ().m_ProState;
        if (cm != null)
            cm._recyleCardMgr.DeleteCard();
        //碰牌区生成3张，特效
        _pengGangMgr.Peng(id);
    }

	public void GangPai(int id, int type) {
        if (type == 1) //明杠
        {
            MingBar(id);
        }
		else if (type == 2) //暗杠
        {
            DarkBar(id);
        }
		else if (type == 3) //加杠
        {
            AddBar(id);
        }
    }

    public void MingBar(int id) {
        //手牌删除3张
        _handCardMgr.Gang(id, false);
        //某一位玩家的桌牌删除一张

		DHM_CardManager cm = GameManager.GetInstance ().m_ProState;

        if (cm != null)
            cm._recyleCardMgr.DeleteCard();

        //碰牌区生成四张牌。特效
        _pengGangMgr.Gang(id, false);
        //激活手牌管理，摸
    }

    public void DarkBar(int id) {
        //手牌删除4张
        _handCardMgr.Gang(id, true);
        //碰牌区生成四张牌。特效
        _pengGangMgr.Gang(id, true);
        //激活手牌管理，摸
    }

    public void AddBar(int id) {
        //手牌删除3张
        _handCardMgr.RemoveMoHandCard(id);
        //碰牌区生成1张牌。特效
        _pengGangMgr.CreateWanGangCard(id);
        //激活手牌管理，摸
    }

    public void MoNiChuPai(int id) {
        _handCardMgr.MoNiChuPai(id);
    }

	public void HuPai(HuPushInfo info) {
		int id = info.hupai;
		if (!info.iszimo) {
            _handCardMgr.HuPai(id);

			DHM_CardManager cm = GameManager.GetInstance ().m_ProState;
            if (cm != null)
                cm._recyleCardMgr.DeleteCard();
        } else {
            ZiMo(id);
        }

        HideChuPaiState();
    }

	public void ZiMo(int id) {
        _handCardMgr.RemoveMoHandCard(id);
        _handCardMgr.HuPai(id);
    }

    public void RePlay()
    {
        _handCardMgr.ResetInfo();
        _recyleCardMgr.ResetInfo();
        _pengGangMgr.ResetPengGangInfo();
/*  fuck todo
        ShowRemainCard.instance.ResetCardCount();
        ShowRemainCard.instance.HideCardCount();
*/
    }
}
